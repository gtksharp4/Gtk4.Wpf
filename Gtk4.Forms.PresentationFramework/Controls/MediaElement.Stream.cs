using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace System.Windows.Controls;

public partial class MediaElement
{
	void OpenStream()
	{
		if (_stream != null || _picture == null) return;
		var uri = ActiveSource();
		if (uri == null) return;
		try
		{
			var baseUri = ActiveBase();
			if (!uri.IsAbsoluteUri && baseUri != null && baseUri.IsAbsoluteUri)
				uri = new Uri(baseUri, uri);
			var stream = CreateStream(uri);
			if (stream == null) return;
			_stream = stream;
			_prepared = false;
			_picture.Paintable = stream;
			ApplyAudio();
			stream.AddNotification("prepared", (_, _) => Guard(() => OnPrepared(stream)));
			stream.AddNotification("error", (_, _) => Guard(() => OnError(stream)));
			stream.AddNotification("ended", (_, _) => Guard(() => OnEnded(stream)));
			if (stream.Error != IntPtr.Zero) OnError(stream);
			else if (stream.IsPrepared) OnPrepared(stream);
		}
		catch (Exception ex)
		{
			Fail(ex);
		}
	}

	Gtk.MediaFile? CreateStream(Uri uri)
	{
		if (uri.IsAbsoluteUri && !uri.IsFile && !string.Equals(uri.Scheme, "pack", StringComparison.OrdinalIgnoreCase))
			return new Gtk.MediaFile(GLib.FileFactory.NewForUri(uri));
		var path = ImageSourceLocation.ResolveUri(uri);
		if (!File.Exists(path))
		{
			Fail(new FileNotFoundException("The media file was not found.", path));
			return null;
		}
		return new Gtk.MediaFile(path);
	}

	void OnError(Gtk.MediaFile stream)
	{
		if (!ReferenceEquals(stream, _stream) || stream.Error == IntPtr.Zero || _failed) return;
		Fail(new InvalidOperationException(ReadError(stream.Error)));
	}

	// MediaFailed is asynchronous, as in WPF, so handlers attached after Source is set still see it.
	void Fail(Exception error)
	{
		_failed = true;
		Dispatcher.BeginInvoke(new Action(() => RaiseFailed(error)));
	}

	// An exception must never escape a GTK callback.
	void Guard(Action action)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			try { RaiseFailed(ex); }
			catch { }
		}
	}

	// GError layout: guint domain, gint code, gchar* message.
	static string ReadError(IntPtr error)
	{
		var message = Marshal.ReadIntPtr(error, 8);
		if (message == IntPtr.Zero) return "The media could not be played.";
		var length = 0;
		while (Marshal.ReadByte(message, length) != 0) length++;
		var bytes = new byte[length];
		Marshal.Copy(message, bytes, 0, length);
		return System.Text.Encoding.UTF8.GetString(bytes);
	}
}
