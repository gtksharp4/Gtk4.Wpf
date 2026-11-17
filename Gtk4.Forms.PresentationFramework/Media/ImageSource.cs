namespace System.Windows.Media;

/// <summary>
/// Phase-1 stand-in for WPF's abstract <c>System.Windows.Media.ImageSource</c>.
/// Wraps a file path; the Phase-4 media stack will replace this with the real
/// abstract base class + codec pipeline.
/// </summary>
public class ImageSource
{
	/// <summary>Absolute or relative path to the image file (empty for derived sources).</summary>
	public string FilePath { get; }

	ImageSource(string filePath) => FilePath = filePath;

	/// <summary>
	/// Base constructor for derived image sources (e.g. the
	/// <see cref="System.Windows.Media.Imaging.BitmapSource"/> family), which identify
	/// their content by URI or stream rather than a file path.
	/// </summary>
	protected ImageSource() => FilePath = string.Empty;

	/// <summary>Creates an <see cref="ImageSource"/> from a file path.</summary>
	public static ImageSource FromFile(string path) => new ImageSource(path);

	/// <inheritdoc/>
	public override string ToString() => FilePath;
}