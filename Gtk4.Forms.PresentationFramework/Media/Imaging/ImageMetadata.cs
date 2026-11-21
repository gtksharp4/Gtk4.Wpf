namespace System.Windows.Media;

/// <summary>Base class of image metadata (WPF <c>ImageMetadata</c>); <c>BitmapMetadata</c> derives from it.</summary>
public abstract class ImageMetadata : Freezable
{
	/// <summary>Creates the base metadata object.</summary>
	protected ImageMetadata() { }
}
