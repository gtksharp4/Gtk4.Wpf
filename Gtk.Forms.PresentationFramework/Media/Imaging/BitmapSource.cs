namespace System.Windows.Media.Imaging
{
	/// <summary>
	/// Phase-1 stand-in for WPF's abstract <c>System.Windows.Media.Imaging.BitmapSource</c>.
	/// Sits between <see cref="ImageSource"/> and concrete bitmaps such as
	/// <see cref="BitmapImage"/>, so WPF code that types image fields as
	/// <c>BitmapSource</c> or <c>ImageSource</c> compiles unchanged. The decode/pixel
	/// pipeline is not yet implemented; the dimension properties default to zero.
	/// </summary>
	public abstract class BitmapSource : ImageSource
	{
		/// <summary>Pixel width of the bitmap (0 until a real decoder is wired in).</summary>
		public virtual int PixelWidth { get; protected set; }

		/// <summary>Pixel height of the bitmap (0 until a real decoder is wired in).</summary>
		public virtual int PixelHeight { get; protected set; }

		/// <summary>Horizontal DPI of the bitmap (WPF default 96).</summary>
		public virtual double DpiX { get; protected set; } = 96d;

		/// <summary>Vertical DPI of the bitmap (WPF default 96).</summary>
		public virtual double DpiY { get; protected set; } = 96d;
	}
}
