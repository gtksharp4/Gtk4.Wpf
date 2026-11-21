namespace System.Windows.Media.Imaging;

/// <summary>Converts raw pixel buffers of any <see cref="PixelFormats"/> member to straight BGRA32.</summary>
static class PixelConverter
{
	internal static int MinStride(int width, PixelFormat format) => (width * format.BitsPerPixel + 7) / 8;

	/// <summary>Returns top-down straight-alpha BGRA32 (stride = width * 4) for the source buffer.</summary>
	internal static byte[] ToBgra32(byte[] src, int stride, int width, int height, PixelFormat format,
		Color[]? palette)
	{
		var dst = new byte[width * height * 4];
		for (int y = 0; y < height; y++)
		{
			int row = y * stride;
			for (int x = 0; x < width; x++)
			{
				var (r, g, b, a) = ReadPixel(src, row, x, format, palette);
				int o = (y * width + x) * 4;
				dst[o] = b;
				dst[o + 1] = g;
				dst[o + 2] = r;
				dst[o + 3] = a;
			}
		}
		return dst;
	}

	static (byte R, byte G, byte B, byte A) ReadPixel(byte[] s, int row, int x, PixelFormat f, Color[]? palette)
	{
		int bpp = f.BitsPerPixel;
		string n = f.Name;
		if (n.StartsWith("Indexed", StringComparison.Ordinal))
		{
			int i = Packed(s, row, x, bpp);
			var c = palette != null && i < palette.Length ? palette[i] : default;
			return (c.R, c.G, c.B, c.A);
		}
		if (n == "BlackWhite" || n == "Gray2" || n == "Gray4" || n == "Gray8")
		{
			byte v = (byte)(Packed(s, row, x, bpp) * 255 / ((1 << bpp) - 1));
			return (v, v, v, 255);
		}
		int p = row + x * bpp / 8;
		switch (n)
		{
			case "Gray16": return Gray((byte)(U16(s, p) >> 8));
			case "Gray32Float": return Gray(Float(BitConverter.ToSingle(s, p)));
			case "Bgr555": return Scaled(U16(s, p) >> 10 & 31, U16(s, p) >> 5 & 31, U16(s, p) & 31, 31, 31, 31);
			case "Bgr565": return Scaled(U16(s, p) >> 11 & 31, U16(s, p) >> 5 & 63, U16(s, p) & 31, 31, 63, 31);
			case "Bgr24": return (s[p + 2], s[p + 1], s[p], 255);
			case "Rgb24": return (s[p], s[p + 1], s[p + 2], 255);
			case "Bgr101010":
				uint v = BitConverter.ToUInt32(s, p);
				return Scaled((int)(v >> 20 & 0x3FF), (int)(v >> 10 & 0x3FF), (int)(v & 0x3FF), 1023, 1023, 1023);
			case "Bgr32": return (s[p + 2], s[p + 1], s[p], 255);
			case "Bgra32": return (s[p + 2], s[p + 1], s[p], s[p + 3]);
			case "Pbgra32": return Straight(s[p + 2], s[p + 1], s[p], s[p + 3]);
			case "Rgb48": return ((byte)(U16(s, p) >> 8), (byte)(U16(s, p + 2) >> 8), (byte)(U16(s, p + 4) >> 8), 255);
			case "Rgba64": return Rgba64(s, p, false);
			case "Prgba64": return Rgba64(s, p, true);
			case "Rgba128Float": return Rgba128(s, p, false);
			case "Prgba128Float": return Rgba128(s, p, true);
			case "Rgb128Float": return (Float(F(s, p)), Float(F(s, p + 4)), Float(F(s, p + 8)), 255);
			case "Cmyk32": return Cmyk(s[p], s[p + 1], s[p + 2], s[p + 3]);
			default: return (0, 0, 0, 0);
		}
	}

	static int Packed(byte[] s, int row, int x, int bpp)
	{
		int bit = x * bpp;
		int shift = 8 - bpp - bit % 8;
		return s[row + bit / 8] >> shift & (1 << bpp) - 1;
	}

	static int U16(byte[] s, int p) => s[p] | s[p + 1] << 8;

	static float F(byte[] s, int p) => BitConverter.ToSingle(s, p);

	static byte Float(float v) => (byte)Math.Round(Math.Max(0f, Math.Min(1f, v)) * 255f);

	static (byte, byte, byte, byte) Gray(byte v) => (v, v, v, 255);

	static (byte, byte, byte, byte) Scaled(int r, int g, int b, int rm, int gm, int bm) =>
		((byte)(r * 255 / rm), (byte)(g * 255 / gm), (byte)(b * 255 / bm), 255);

	static byte Un(int c, int a) => a == 0 ? (byte)0 : (byte)Math.Min(255, (c * 255 + a / 2) / a);

	static (byte, byte, byte, byte) Straight(byte r, byte g, byte b, byte a) =>
		a == 255 ? (r, g, b, a) : (Un(r, a), Un(g, a), Un(b, a), a);

	static (byte, byte, byte, byte) Rgba64(byte[] s, int p, bool premultiplied)
	{
		byte r = (byte)(U16(s, p) >> 8), g = (byte)(U16(s, p + 2) >> 8);
		byte b = (byte)(U16(s, p + 4) >> 8), a = (byte)(U16(s, p + 6) >> 8);
		return premultiplied ? Straight(r, g, b, a) : (r, g, b, a);
	}

	static (byte, byte, byte, byte) Rgba128(byte[] s, int p, bool premultiplied)
	{
		byte r = Float(F(s, p)), g = Float(F(s, p + 4)), b = Float(F(s, p + 8)), a = Float(F(s, p + 12));
		return premultiplied ? Straight(r, g, b, a) : (r, g, b, a);
	}

	static (byte, byte, byte, byte) Cmyk(int c, int m, int y, int k) =>
		((byte)((255 - c) * (255 - k) / 255), (byte)((255 - m) * (255 - k) / 255),
			(byte)((255 - y) * (255 - k) / 255), 255);
}
