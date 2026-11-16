namespace System.Windows.Media
{
	/// <summary>
	/// Phase-1 stand-in for WPF's abstract <c>System.Windows.Media.ImageSource</c>.
	/// Wraps a file path; the Phase-4 media stack will replace this with the real
	/// abstract base class + codec pipeline.
	/// </summary>
	public class ImageSource
	{
		/// <summary>Absolute or relative path to the image file.</summary>
		public string FilePath { get; }

		ImageSource(string filePath) => FilePath = filePath;

		/// <summary>Creates an <see cref="ImageSource"/> from a file path.</summary>
		public static ImageSource FromFile(string path) => new ImageSource(path);

		/// <inheritdoc/>
		public override string ToString() => FilePath;
	}
}
