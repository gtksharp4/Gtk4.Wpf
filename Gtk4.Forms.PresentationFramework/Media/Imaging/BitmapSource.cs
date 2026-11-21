namespace System.Windows.Media.Imaging;

/// <summary>
/// WPF <c>BitmapSource</c>: a bitmap with a pixel format, optional palette and DPI. Pixels live in
/// managed memory (<see cref="GetPixelData"/>); GTK widgets display them through a
/// <c>Gdk.MemoryTexture</c> built from <see cref="ToBgra32"/>.
/// </summary>
public abstract partial class BitmapSource : ImageSource
{
	PixelFormat _format = PixelFormats.Pbgra32;

	/// <summary>Creates an empty bitmap source.</summary>
	protected BitmapSource() { }

	/// <summary>Pixel width of the bitmap.</summary>
	public virtual int PixelWidth { get; protected set; }

	/// <summary>Pixel height of the bitmap.</summary>
	public virtual int PixelHeight { get; protected set; }

	/// <summary>Horizontal DPI of the bitmap (WPF default 96).</summary>
	public virtual double DpiX { get; protected set; } = 96d;

	/// <summary>Vertical DPI of the bitmap (WPF default 96).</summary>
	public virtual double DpiY { get; protected set; } = 96d;

	/// <summary>The pixel format of the bitmap.</summary>
	public virtual PixelFormat Format
	{
		get => _format;
		protected set => _format = value;
	}

	/// <summary>The metadata of the bitmap; <see langword="null"/> when it has none.</summary>
	public override ImageMetadata? Metadata => null;

	/// <summary>The palette of an indexed bitmap, otherwise <see langword="null"/>.</summary>
	public virtual BitmapPalette? Palette { get; protected set; }

	/// <summary>Width in device-independent units (<c>PixelWidth * 96 / DpiX</c>; 96 when DpiX <= 0).</summary>
	public virtual double Width => PixelWidth * 96d / (DpiX > 0 ? DpiX : 96d);

	/// <summary>Height in device-independent units (<c>PixelHeight * 96 / DpiY</c>; 96 when DpiY <= 0).</summary>
	public virtual double Height => PixelHeight * 96d / (DpiY > 0 ? DpiY : 96d);

	/// <summary>The pixels in <see cref="Format"/>, top-down, rows of exactly the minimal stride.</summary>
	internal virtual byte[] GetPixelData() => new byte[PixelConverter.MinStride(PixelWidth, Format) * PixelHeight];

	/// <summary>The pixels converted to top-down straight-alpha BGRA32.</summary>
	internal byte[] ToBgra32() => PixelConverter.ToBgra32(
		GetPixelData(), PixelConverter.MinStride(PixelWidth, Format), PixelWidth, PixelHeight, Format,
		Palette?.ColorArray);
}
