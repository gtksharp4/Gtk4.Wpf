using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace System.Windows.Resources;

/// <summary>
/// Locates the stream for a relative or pack://application URI. Files under the app base directory win,
/// then manifest resources of the resource assembly (the pack assembly or the entry assembly).
/// </summary>
/// <remarks>
/// A <c>&lt;Resource&gt;</c> item is not copied to the output of a converted app and no <c>.g.resources</c>
/// is produced, so such a resource is found only if the project also copies it (CopyToOutputDirectory)
/// or embeds it (<c>&lt;EmbeddedResource&gt;</c>, matched by its dotted name suffix).
/// </remarks>
static class ResourceStreamLocator
{
	/// <summary>Replaces the entry assembly as the manifest-resource source (tests).</summary>
	internal static Assembly? ResourceAssemblyOverride { get; set; }

	internal static bool IsApplicationPackUri(Uri uri) =>
		string.Equals(uri.Scheme, "pack", StringComparison.OrdinalIgnoreCase)
		&& Uri.UnescapeDataString(uri.Host).StartsWith("application", StringComparison.OrdinalIgnoreCase);

	internal static StreamResourceInfo? Find(Uri uri)
	{
		string path = ImageSourceLocation.ResolveUri(uri);
		if (File.Exists(path))
			return new StreamResourceInfo(File.OpenRead(path), ContentType(path));

		string relative = Relative(uri);
		var assembly = ResourceAssemblyOverride ?? AssemblyFor(uri) ?? Assembly.GetEntryAssembly();
		return assembly == null ? null : FromManifest(assembly, relative);
	}

	static StreamResourceInfo? FromManifest(Assembly assembly, string relative)
	{
		string dotted = relative.Replace('/', '.');
		string? name = assembly.GetManifestResourceNames().FirstOrDefault(n =>
			string.Equals(n, dotted, StringComparison.OrdinalIgnoreCase)
			|| n.EndsWith("." + dotted, StringComparison.OrdinalIgnoreCase));
		var stream = name == null ? null : assembly.GetManifestResourceStream(name);
		return stream == null ? null : new StreamResourceInfo(stream, ContentType(relative));
	}

	internal static string LowerRelative(Uri uri) => Relative(uri).ToLowerInvariant();

	static string Relative(Uri uri)
	{
		string text = uri.IsAbsoluteUri
			? Uri.UnescapeDataString(uri.AbsolutePath)
			: Uri.UnescapeDataString(uri.OriginalString);
		text = text.Replace((char)92, '/').TrimStart('/');
		int component = text.IndexOf(";component/", StringComparison.OrdinalIgnoreCase);
		return component >= 0 ? text.Substring(component + ";component/".Length) : text;
	}

	static Assembly? AssemblyFor(Uri uri)
	{
		if (!uri.IsAbsoluteUri) return null;
		string path = Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/');
		int component = path.IndexOf(";component/", StringComparison.OrdinalIgnoreCase);
		if (component < 0) return null;
		string name = path.Substring(0, component);
		int version = name.IndexOf(";v", StringComparison.OrdinalIgnoreCase);
		if (version >= 0) name = name.Substring(0, version);
		return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a =>
			string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase));
	}

	static string ContentType(string path) =>
		Path.GetExtension(path).ToLowerInvariant() switch
		{
			".xaml" => "application/xaml+xml",
			".png" => "image/png",
			".jpg" or ".jpeg" => "image/jpeg",
			".gif" => "image/gif",
			".bmp" => "image/bmp",
			".ico" => "image/x-icon",
			".svg" => "image/svg+xml",
			".json" => "application/json",
			".xml" => "text/xml",
			".txt" => "text/plain",
			".htm" or ".html" => "text/html",
			".css" => "text/css",
			".wav" => "audio/wav",
			".mp3" => "audio/mpeg",
			".mp4" => "video/mp4",
			_ => "application/octet-stream",
		};
}
