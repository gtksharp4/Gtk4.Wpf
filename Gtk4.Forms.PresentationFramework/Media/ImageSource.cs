using System.Windows.Media.Animation;

namespace System.Windows.Media;

/// <summary>
/// WPF <c>ImageSource</c>: <c>ImageSource : Animatable : Freezable</c>. The base class itself is
/// the layer's file-path stand-in (<see cref="FromFile"/>); the bitmap family derives from
/// <see cref="Imaging.BitmapSource"/>.
/// </summary>
public class ImageSource : Animatable
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

	/// <summary>The image metadata; <see langword="null"/> for a source without any.</summary>
	public virtual ImageMetadata? Metadata => null;

	/// <summary>Creates an <see cref="ImageSource"/> from a file path.</summary>
	public static ImageSource FromFile(string path) => new ImageSource(path);

	/// <summary>Creates a new instance of the same concrete type; derived sources override it.</summary>
	protected override Freezable CreateInstanceCore() => new ImageSource(FilePath);

	/// <inheritdoc/>
	public override string ToString() => FilePath;
}
