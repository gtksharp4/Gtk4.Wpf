namespace System.Windows.Media;

/// <summary>The well-known <see cref="PixelFormat"/> values (WPF <c>PixelFormats</c>).</summary>
public static class PixelFormats
{
	static byte[] M(params byte[] bytes) => bytes;

	static byte[][] Bytes(int bytesPerPixel, int channels, int channelBytes)
	{
		var masks = new byte[channels][];
		for (int c = 0; c < channels; c++)
		{
			masks[c] = new byte[bytesPerPixel];
			for (int i = 0; i < channelBytes; i++)
				masks[c][c * channelBytes + i] = 0xFF;
		}
		return masks;
	}

	/// <summary>The default format, pre-multiplied BGRA.</summary>
	public static readonly PixelFormat Default = new PixelFormat("Default", 0);

	/// <summary>1 bit per pixel palette index.</summary>
	public static readonly PixelFormat Indexed1 = new PixelFormat("Indexed1", 1, M(0x01));

	/// <summary>2 bits per pixel palette index.</summary>
	public static readonly PixelFormat Indexed2 = new PixelFormat("Indexed2", 2, M(0x03));

	/// <summary>4 bits per pixel palette index.</summary>
	public static readonly PixelFormat Indexed4 = new PixelFormat("Indexed4", 4, M(0x0F));

	/// <summary>8 bits per pixel palette index.</summary>
	public static readonly PixelFormat Indexed8 = new PixelFormat("Indexed8", 8, M(0xFF));

	/// <summary>1 bit black and white.</summary>
	public static readonly PixelFormat BlackWhite = new PixelFormat("BlackWhite", 1, M(0x01));

	/// <summary>2 bit gray.</summary>
	public static readonly PixelFormat Gray2 = new PixelFormat("Gray2", 2, M(0x03));

	/// <summary>4 bit gray.</summary>
	public static readonly PixelFormat Gray4 = new PixelFormat("Gray4", 4, M(0x0F));

	/// <summary>8 bit gray.</summary>
	public static readonly PixelFormat Gray8 = new PixelFormat("Gray8", 8, M(0xFF));

	/// <summary>16 bit gray.</summary>
	public static readonly PixelFormat Gray16 = new PixelFormat("Gray16", 16, M(0xFF, 0xFF));

	/// <summary>32 bit floating point gray.</summary>
	public static readonly PixelFormat Gray32Float = new PixelFormat("Gray32Float", 32, Bytes(4, 1, 4));

	/// <summary>16 bit 5-5-5 BGR.</summary>
	public static readonly PixelFormat Bgr555 =
		new PixelFormat("Bgr555", 16, M(0x1F, 0x00), M(0xE0, 0x03), M(0x00, 0x7C));

	/// <summary>16 bit 5-6-5 BGR.</summary>
	public static readonly PixelFormat Bgr565 =
		new PixelFormat("Bgr565", 16, M(0x1F, 0x00), M(0xE0, 0x07), M(0x00, 0xF8));

	/// <summary>24 bit BGR.</summary>
	public static readonly PixelFormat Bgr24 = new PixelFormat("Bgr24", 24, Bytes(3, 3, 1));

	/// <summary>24 bit RGB.</summary>
	public static readonly PixelFormat Rgb24 = new PixelFormat("Rgb24", 24, Bytes(3, 3, 1));

	/// <summary>32 bit 10-10-10 BGR.</summary>
	public static readonly PixelFormat Bgr101010 = new PixelFormat(
		"Bgr101010", 32, M(0xFF, 0x03, 0x00, 0x00), M(0x00, 0xFC, 0x0F, 0x00), M(0x00, 0x00, 0xF0, 0x3F));

	/// <summary>32 bit BGR (the fourth byte is unused).</summary>
	public static readonly PixelFormat Bgr32 = new PixelFormat("Bgr32", 32, Bytes(4, 3, 1));

	/// <summary>32 bit BGRA, straight alpha.</summary>
	public static readonly PixelFormat Bgra32 = new PixelFormat("Bgra32", 32, Bytes(4, 4, 1));

	/// <summary>32 bit BGRA, pre-multiplied alpha.</summary>
	public static readonly PixelFormat Pbgra32 = new PixelFormat("Pbgra32", 32, Bytes(4, 4, 1));

	/// <summary>48 bit RGB, 16 bits per channel.</summary>
	public static readonly PixelFormat Rgb48 = new PixelFormat("Rgb48", 48, Bytes(6, 3, 2));

	/// <summary>64 bit RGBA, straight alpha.</summary>
	public static readonly PixelFormat Rgba64 = new PixelFormat("Rgba64", 64, Bytes(8, 4, 2));

	/// <summary>64 bit RGBA, pre-multiplied alpha.</summary>
	public static readonly PixelFormat Prgba64 = new PixelFormat("Prgba64", 64, Bytes(8, 4, 2));

	/// <summary>128 bit floating point RGBA, straight alpha.</summary>
	public static readonly PixelFormat Rgba128Float = new PixelFormat("Rgba128Float", 128, Bytes(16, 4, 4));

	/// <summary>128 bit floating point RGBA, pre-multiplied alpha.</summary>
	public static readonly PixelFormat Prgba128Float = new PixelFormat("Prgba128Float", 128, Bytes(16, 4, 4));

	/// <summary>128 bit floating point RGB (the fourth float is unused).</summary>
	public static readonly PixelFormat Rgb128Float = new PixelFormat("Rgb128Float", 128, Bytes(16, 3, 4));

	/// <summary>32 bit CMYK.</summary>
	public static readonly PixelFormat Cmyk32 = new PixelFormat("Cmyk32", 32, Bytes(4, 4, 1));
}
