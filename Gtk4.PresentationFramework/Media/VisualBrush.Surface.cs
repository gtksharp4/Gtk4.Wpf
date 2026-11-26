namespace System.Windows.Media;

public partial class VisualBrush
{
	/// <summary>
	/// The source's current pixels and their bounds, for consumers outside the Cairo fill path
	/// (a 3D material texture). Raises <see cref="Freezable.Changed"/> when the pixels go stale.
	/// </summary>
	internal bool TryGetLiveSurface(out Cairo.ImageSurface surface, out Rect contentBounds)
	{
		surface = null!;
		contentBounds = Rect.Empty;
		if (Visual is not UIElement element) return false;
		Watch(element);
		var live = RenderSource(element);
		if (live == null) return false;
		surface = live;
		contentBounds = new Rect(0, 0, live.Width, live.Height);
		return true;
	}
}
