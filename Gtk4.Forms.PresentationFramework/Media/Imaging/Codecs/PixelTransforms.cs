namespace System.Windows.Media.Imaging;

/// <summary>Rotation, flipping and resizing of straight BGRA32 buffers.</summary>
static class PixelTransforms
{
	internal static (byte[] Pixels, int Width, int Height) Apply(byte[] src, int w, int h, Rotation rotation,
		bool flipH, bool flipV)
	{
		var pixels = src;
		int width = w, height = h;
		if (flipH || flipV)
		{
			pixels = new byte[src.Length];
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
					Copy(src, (y * w + x) * 4, pixels, ((flipV ? h - 1 - y : y) * w + (flipH ? w - 1 - x : x)) * 4);
			}
		}
		if (rotation == Rotation.Rotate0)
			return (pixels, width, height);

		bool swap = rotation != Rotation.Rotate180;
		int nw = swap ? h : w, nh = swap ? w : h;
		var rotated = new byte[pixels.Length];
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int nx, ny;
				switch (rotation)
				{
					case Rotation.Rotate90: nx = height - 1 - y; ny = x; break;
					case Rotation.Rotate180: nx = width - 1 - x; ny = height - 1 - y; break;
					default: nx = y; ny = width - 1 - x; break;
				}
				Copy(pixels, (y * width + x) * 4, rotated, (ny * nw + nx) * 4);
			}
		}
		return (rotated, nw, nh);
	}

	internal static byte[] Resize(byte[] src, int w, int h, int nw, int nh)
	{
		var dst = new byte[nw * nh * 4];
		for (int y = 0; y < nh; y++)
		{
			int sy = Math.Min(h - 1, (int)((y + 0.5) * h / nh));
			for (int x = 0; x < nw; x++)
				Copy(src, (sy * w + Math.Min(w - 1, (int)((x + 0.5) * w / nw))) * 4, dst, (y * nw + x) * 4);
		}
		return dst;
	}

	static void Copy(byte[] s, int si, byte[] d, int di)
	{
		d[di] = s[si];
		d[di + 1] = s[si + 1];
		d[di + 2] = s[si + 2];
		d[di + 3] = s[si + 3];
	}
}
