using System.Windows.Controls.Primitives;

namespace System.Windows.Annotations;

public sealed partial class AnnotationService
{
	/// <summary>Creates a service for a document viewer.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="viewer"/> is null.</exception>
	public AnnotationService(DocumentViewerBase viewer) : this(HostOf(viewer))
	{
	}

	static IAnnotationHost HostOf(DocumentViewerBase viewer)
	{
		if (viewer == null) throw new ArgumentNullException(nameof(viewer));
		return new DocumentViewerBaseHost(viewer);
	}

	/// <summary>The enabled service on a document viewer, or null.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="viewer"/> is null.</exception>
	public static AnnotationService? GetService(DocumentViewerBase viewer)
	{
		if (viewer == null) throw new ArgumentNullException(nameof(viewer));
		return Lookup(viewer);
	}
}
