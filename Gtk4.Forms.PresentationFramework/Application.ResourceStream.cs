using System.IO;
using System.Windows.Resources;

namespace System.Windows;

public partial class Application
{
	/// <summary>
	/// Returns the stream and content type for a relative or <c>pack://application/</c> resource URI
	/// (System.Uri cannot parse the <c>:,,,</c> form; the layer normalises it). See <c>ResourceStreamLocator</c>
	/// for where it looks: this layer has no <c>.g.resources</c> container.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="uriResource"/> is null.</exception>
	/// <exception cref="ArgumentException">The URI is absolute and not a pack://application URI.</exception>
	/// <exception cref="IOException">No file or embedded resource matches.</exception>
	public static StreamResourceInfo? GetResourceStream(Uri uriResource)
	{
		if (uriResource == null) throw new ArgumentNullException(nameof(uriResource));
		if (uriResource.IsAbsoluteUri && !ResourceStreamLocator.IsApplicationPackUri(uriResource))
			throw new ArgumentException("An absolute URI must be a pack://application URI.", nameof(uriResource));
		return ResourceStreamLocator.Find(uriResource)
			?? throw new IOException(
				$"Cannot locate resource '{ResourceStreamLocator.LowerRelative(uriResource)}'.");
	}
}
