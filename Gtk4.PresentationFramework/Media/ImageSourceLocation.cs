using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace System.Windows.Media;

/// <summary>
/// Maps an image source to a file on disk. Relative, root-relative and pack://application URIs
/// resolve under the app directory.
/// </summary>
static class ImageSourceLocation
{
	internal static string ResolveFilePath(ImageSource source)
	{
		if (source is BitmapImage { UriSource: { } uri })
			return ResolveUri(uri);
		return source.FilePath;
	}

	internal static string ResolveUri(Uri uri)
	{
		if (!uri.IsAbsoluteUri)
			return FromBase(uri.OriginalString);
		if (uri.IsFile)
			return uri.LocalPath;
		if (string.Equals(uri.Scheme, "pack", StringComparison.OrdinalIgnoreCase))
			return FromBase(PackRelativePath(uri));
		return uri.OriginalString;
	}

	static string PackRelativePath(Uri uri)
	{
		string path = Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/');
		int component = path.IndexOf(";component/", StringComparison.OrdinalIgnoreCase);
		return component >= 0 ? path.Substring(component + ";component/".Length) : path;
	}

	static string FromBase(string relative)
	{
		string clean = Uri.UnescapeDataString(relative).TrimStart('/', '\\');
		return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, clean));
	}
}
