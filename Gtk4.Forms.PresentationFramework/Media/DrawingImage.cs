namespace System.Windows.Media;

/// <summary>
/// An <see cref="ImageSource"/> that wraps a <see cref="Media.Drawing"/>.
/// Mirrors <c>System.Windows.Media.DrawingImage</c>.
/// </summary>
/// <remarks>
/// <b>Object-model only:</b> the <c>Image</c> control does not paint a <see cref="DrawingImage"/> yet.
/// </remarks>
public class DrawingImage : ImageSource
{
	/// <summary>Creates an empty drawing image.</summary>
	public DrawingImage() { }

	/// <summary>Creates a drawing image over <paramref name="drawing"/>.</summary>
	public DrawingImage(Drawing? drawing) => Drawing = drawing;

	/// <summary>The drawing this image shows.</summary>
	public Drawing? Drawing { get; set; }
}
