namespace System.Windows.Media.Imaging;

public sealed partial class BitmapImage
{
	/// <summary>Not supported on a BitmapImage, as in WPF; use <see cref="BitmapFrame.Metadata"/>.</summary>
	/// <exception cref="NotSupportedException">Always.</exception>
	public override ImageMetadata? Metadata =>
		throw new NotSupportedException("Metadata is not supported for this image type.");
}
