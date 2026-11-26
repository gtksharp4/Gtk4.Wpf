namespace System.Windows.Media.Imaging;

/// <summary>Writes top-down straight-alpha BGRA32 pixels in a destination <see cref="PixelFormat"/>.</summary>
static class FormatWriter
{
	internal static byte[] Write(byte[] bgra, int width, int height, PixelFormat format, Color[]? palette,
		double alphaThreshold)
	{
		string name = format.Name;
		int bpp = format.BitsPerPixel;
		int stride = PixelConverter.MinStride(width, format);
		var dst = new byte[stride * height];
		bool indexed = name.StartsWith("Indexed", StringComparison.Ordinal);
		if (!indexed && !IsSupported(name))
			throw new NotSupportedException($"Converting to {name} is not supported.");
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int s = (y * width + x) * 4;
				byte b = bgra[s], g = bgra[s + 1], r = bgra[s + 2], a = bgra[s + 3];
				int d = y * stride + x * bpp / 8;
				if (indexed)
					SetPacked(dst, y * stride, x, bpp, Nearest(palette!, r, g, b, a, alphaThreshold));
				else if (bpp < 8)
					SetPacked(dst, y * stride, x, bpp, Gray(r, g, b) * ((1 << bpp) - 1) / 255);
				else
					WritePixel(dst, d, name, r, g, b, a);
			}
		}
		return dst;
	}

	static bool IsSupported(string name) => name == "BlackWhite" || name == "Gray2" || name == "Gray4"
		|| name == "Gray8" || name == "Gray16" || name == "Bgr24" || name == "Rgb24" || name == "Bgr32"
		|| name == "Bgra32" || name == "Pbgra32";

	static void WritePixel(byte[] d, int o, string name, byte r, byte g, byte b, byte a)
	{
		switch (name)
		{
			case "Gray8":
				d[o] = Gray(r, g, b);
				break;
			case "Gray16":
				d[o] = d[o + 1] = Gray(r, g, b);
				break;
			case "Bgr24":
				d[o] = b; d[o + 1] = g; d[o + 2] = r;
				break;
			case "Rgb24":
				d[o] = r; d[o + 1] = g; d[o + 2] = b;
				break;
			case "Bgr32":
				d[o] = b; d[o + 1] = g; d[o + 2] = r; d[o + 3] = 0;
				break;
			case "Bgra32":
				d[o] = b; d[o + 1] = g; d[o + 2] = r; d[o + 3] = a;
				break;
			default:
				d[o] = Mul(b, a); d[o + 1] = Mul(g, a); d[o + 2] = Mul(r, a); d[o + 3] = a;
				break;
		}
	}

	static byte Mul(byte c, byte a) => (byte)((c * a + 127) / 255);

	static byte Gray(byte r, byte g, byte b) => (byte)((r * 2126 + g * 7152 + b * 722 + 5000) / 10000);

	static void SetPacked(byte[] d, int row, int x, int bpp, int value)
	{
		if (bpp == 8)
		{
			d[row + x] = (byte)value;
			return;
		}
		int bit = x * bpp;
		d[row + bit / 8] |= (byte)(value << 8 - bpp - bit % 8);
	}

	static int Nearest(Color[] palette, byte r, byte g, byte b, byte a, double alphaThreshold)
	{
		bool transparent = a * 100.0 / 255 < alphaThreshold;
		int best = 0;
		long bestDistance = long.MaxValue;
		for (int i = 0; i < palette.Length; i++)
		{
			var c = palette[i];
			if (transparent)
			{
				if (c.A == 0)
					return i;
				continue;
			}
			long dr = c.R - r, dg = c.G - g, db = c.B - b, da = c.A - a;
			long distance = dr * dr + dg * dg + db * db + da * da;
			if (distance < bestDistance)
			{
				bestDistance = distance;
				best = i;
			}
		}
		return best;
	}
}
