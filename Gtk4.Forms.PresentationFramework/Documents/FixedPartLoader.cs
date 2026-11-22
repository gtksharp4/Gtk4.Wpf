using System.IO;
using System.Windows.Markup;
using System.Windows.Xps.Packaging;

namespace System.Windows.Documents;

/// <summary>Loads what <see cref="PageContent"/> and <see cref="DocumentReference"/> point to.</summary>
static class FixedPartLoader
{
	/// <summary>Resolves <paramref name="source"/> against <paramref name="baseUri"/>.</summary>
	/// <exception cref="InvalidOperationException">A relative source has no base URI.</exception>
	internal static Uri Resolve(Uri? baseUri, Uri source)
	{
		if (source.IsAbsoluteUri)
			return source;
		if (baseUri == null || !baseUri.IsAbsoluteUri)
			throw new InvalidOperationException("The relative source '" + source + "' has no base URI.");
		return new Uri(baseUri, source);
	}

	internal static FixedPage LoadPage(Uri uri)
	{
		if (XpsPackage.TryResolve(uri, out var package, out string part))
			return XpsPartReader.ReadPage(package!, part);
		return (FixedPage)Load(uri, "FixedPage");
	}

	internal static FixedDocument LoadDocument(Uri uri)
	{
		if (XpsPackage.TryResolve(uri, out var package, out string part))
			return XpsPartReader.ReadDocument(package!, part);
		var document = (FixedDocument)Load(uri, "FixedDocument");
		((IUriContext)document).BaseUri = uri;
		return document;
	}

	// Outside a package the part is a XAML file read through the XAML loader.
	static object Load(Uri uri, string kind)
	{
		if (!uri.IsAbsoluteUri || !uri.IsFile)
			throw new InvalidOperationException("The " + kind + " source '" + uri + "' is not in an open XPS package.");
		using (var stream = File.OpenRead(uri.LocalPath))
			return XamlReader.Load(stream);
	}
}
