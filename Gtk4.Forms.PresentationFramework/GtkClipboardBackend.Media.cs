using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace System.Windows;

// Images, file lists and audio. They are pushed to the native clipboard as GDK content providers and
// read back, when another process owns the clipboard, through a nested GLib loop.
internal sealed partial class GtkClipboardBackend
{
	const string UriListMime = "text/uri-list";
	const string WavMime = "audio/wav";

	static bool IsMedia(string format) =>
		format == DataFormats.Bitmap || format == DataFormats.FileDrop || format == DataFormats.WaveAudio;

	static string[] MimeTypes(Gdk.ContentFormats formats)
	{
		ulong count;
		return formats.GetMimeTypes(out count) ?? Array.Empty<string>();
	}

	/// <summary>
	/// Whether <paramref name="formats"/> offers something that maps to the WPF <paramref name="format"/>.
	/// </summary>
	internal static bool FormatsContain(Gdk.ContentFormats formats, string format)
	{
		var mimes = MimeTypes(formats);
		if (format == DataFormats.Bitmap) return mimes.Any(m => m.StartsWith("image/", StringComparison.Ordinal));
		if (format == DataFormats.WaveAudio) return mimes.Any(m => m.StartsWith("audio/", StringComparison.Ordinal));
		if (format == DataFormats.FileDrop) return mimes.Contains(UriListMime);
		return false;
	}

	/// <summary>
	/// Converts a <c>text/uri-list</c> body to local file paths, skipping comments and non-file URIs.
	/// </summary>
	internal static string[] ParseUriList(string text) =>
		text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
			.Where(line => !line.StartsWith("#", StringComparison.Ordinal))
			.Select(line => Uri.TryCreate(line.Trim(), UriKind.Absolute, out var uri) && uri.IsFile
				? uri.LocalPath : null)
			.OfType<string>()
			.ToArray();

	bool ForeignHas(string format)
	{
		if (!IsMedia(format))
			return false;
		try
		{
			var native = NativeClipboard();
			return native != null && !native.IsLocal && FormatsContain(native.Formats, format);
		}
		catch { return false; }
	}

	void PushMedia(IDataObject data)
	{
		try
		{
			var native = NativeClipboard();
			if (native == null)
				return;
			if (data.GetData(DataFormats.FileDrop) is string[] paths)
				native.SetContent(Provider(UriListMime, Encoding.UTF8.GetBytes(ToUriList(paths))));
			else if (data.GetData(DataFormats.WaveAudio) is Stream audio)
				native.SetContent(Provider(WavMime, ReadAll(audio)));
			else if (data.GetData(DataFormats.Bitmap) is BitmapImage image && ImageBytes(image) is { } bytes)
				native.Texture = new Gdk.Texture(new GLib.Bytes(bytes));
		}
		catch { /* best effort */ }
	}

	static Gdk.ContentProvider Provider(string mime, byte[] bytes) =>
		new Gdk.ContentProvider(mime, new GLib.Bytes(bytes));

	static string ToUriList(string[] paths) =>
		string.Concat(paths.Select(p => new Uri(Path.GetFullPath(p)).AbsoluteUri + "\r\n"));

	static byte[] ReadAll(Stream stream)
	{
		long position = stream.CanSeek ? stream.Position : 0;
		using var copy = new MemoryStream();
		stream.CopyTo(copy);
		if (stream.CanSeek)
			stream.Position = position;
		return copy.ToArray();
	}

	internal static byte[]? ImageBytes(BitmapImage image)
	{
		if (image.LoadedBytes != null)
			return image.LoadedBytes;
		if (image.StreamSource != null)
			return ReadAll(image.StreamSource);
		return image.UriSource is { IsFile: true } uri && File.Exists(uri.LocalPath)
			? File.ReadAllBytes(uri.LocalPath) : null;
	}

	/// <inheritdoc/>
	public object? GetData(string format)
	{
		var stored = _mirror.GetData(format);
		if (stored != null || !ForeignHas(format))
			return stored;
		try { return ReadForeign(format); }
		catch { return null; }
	}

	object? ReadForeign(string format)
	{
		var native = NativeClipboard()!;
		var mimes = MimeTypes(native.Formats);
		if (format == DataFormats.FileDrop)
		{
			var body = ReadMime(native, UriListMime);
			return body == null ? null : ParseUriList(Encoding.UTF8.GetString(body));
		}
		var prefix = format == DataFormats.Bitmap ? "image/" : "audio/";
		var mime = mimes.FirstOrDefault(m => m == prefix + "png")
			?? mimes.FirstOrDefault(m => m.StartsWith(prefix, StringComparison.Ordinal));
		var bytes = mime == null ? null : ReadMime(native, mime);
		if (bytes == null)
			return null;
		return format == DataFormats.Bitmap
			? new BitmapImage { StreamSource = new MemoryStream(bytes) } : new MemoryStream(bytes);
	}

	// The clipboard only offers async reads, so spin a nested loop until the stream arrives or time runs out.
	static byte[]? ReadMime(Gdk.Clipboard native, string mime)
	{
		GLib.InputStream? stream = null;
		var loop = new GLib.MainLoop();
		native.ReadAsync(new[] { mime }, 0, null!, (source, result, userData) =>
		{
			try { stream = native.ReadFinish(result, out _); }
			catch { stream = null; }
			finally { loop.Quit(); }
		});
		var timeout = GLib.Timeout.Add(3000, new GLib.TimeoutHandler(() => { loop.Quit(); return false; }));
		loop.Run();
		GLib.Source.Remove(timeout);
		if (stream == null)
			return null;
		using var buffer = new MemoryStream();
		while (stream.ReadBytes(65536, null!) is { Size: > 0 } chunk)
			buffer.Write(chunk.Data, 0, (int)chunk.Size);
		return buffer.ToArray();
	}
}
