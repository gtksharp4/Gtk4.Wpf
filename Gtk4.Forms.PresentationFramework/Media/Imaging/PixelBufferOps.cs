namespace System.Windows.Media.Imaging;

/// <summary>Format-agnostic pixel copies on raw top-down buffers (any bits per pixel, minimal stride).</summary>
static class PixelBufferOps
{
	/// <summary>Copies one pixel of <paramref name="bpp"/> bits from (sx, sy) to (dx, dy).</summary>
	internal static void CopyPixel(byte[] src, int srcStride, int sx, int sy, byte[] dst, int dstStride, int dx,
		int dy, int bpp)
	{
		if (bpp >= 8)
		{
			int n = bpp / 8;
			Buffer.BlockCopy(src, sy * srcStride + sx * n, dst, dy * dstStride + dx * n, n);
			return;
		}
		int sbit = sx * bpp;
		int value = src[sy * srcStride + sbit / 8] >> 8 - bpp - sbit % 8 & (1 << bpp) - 1;
		int dbit = dx * bpp;
		int shift = 8 - bpp - dbit % 8;
		int index = dy * dstStride + dbit / 8;
		dst[index] = (byte)(dst[index] & ~(((1 << bpp) - 1) << shift) | value << shift);
	}

	/// <summary>Returns the pixels inside <paramref name="r"/>.</summary>
	internal static byte[] Crop(byte[] src, int width, int bpp, Int32Rect r)
	{
		int srcStride = (width * bpp + 7) / 8;
		int dstStride = (r.Width * bpp + 7) / 8;
		var dst = new byte[dstStride * r.Height];
		for (int y = 0; y < r.Height; y++)
		{
			if (bpp >= 8)
				Buffer.BlockCopy(src, (r.Y + y) * srcStride + r.X * (bpp / 8), dst, y * dstStride, dstStride);
			else
				for (int x = 0; x < r.Width; x++)
					CopyPixel(src, srcStride, r.X + x, r.Y + y, dst, dstStride, x, y, bpp);
		}
		return dst;
	}

	/// <summary>Builds a <paramref name="dw"/> x <paramref name="dh"/> buffer; <paramref name="map"/> gives the
	/// source pixel of each destination pixel.</summary>
	internal static byte[] Remap(byte[] src, int sw, int bpp, int dw, int dh, Func<int, int, (int X, int Y)> map)
	{
		int srcStride = (sw * bpp + 7) / 8;
		int dstStride = (dw * bpp + 7) / 8;
		var dst = new byte[dstStride * dh];
		for (int y = 0; y < dh; y++)
		{
			for (int x = 0; x < dw; x++)
			{
				var (sx, sy) = map(x, y);
				CopyPixel(src, srcStride, sx, sy, dst, dstStride, x, y, bpp);
			}
		}
		return dst;
	}
}
