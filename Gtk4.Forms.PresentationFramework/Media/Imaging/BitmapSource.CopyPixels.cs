namespace System.Windows.Media.Imaging;

public abstract partial class BitmapSource
{
	/// <summary>Copies all pixels into <paramref name="pixels"/> (in <see cref="Format"/>).</summary>
	public void CopyPixels(Array pixels, int stride, int offset) =>
		CopyPixels(new Int32Rect(0, 0, PixelWidth, PixelHeight), pixels, stride, offset);

	/// <summary>Copies the pixels inside <paramref name="sourceRect"/> into <paramref name="pixels"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="pixels"/> is null.</exception>
	/// <exception cref="ArgumentException">The stride or the buffer is too small, or the rectangle is
	/// outside the bitmap.</exception>
	public virtual void CopyPixels(Int32Rect sourceRect, Array pixels, int stride, int offset)
	{
		if (pixels == null)
			throw new ArgumentNullException(nameof(pixels));
		if (sourceRect.IsEmpty)
			sourceRect = new Int32Rect(0, 0, PixelWidth, PixelHeight);
		if (sourceRect.X < 0 || sourceRect.Y < 0 || sourceRect.Width < 0 || sourceRect.Height < 0
			|| sourceRect.X + sourceRect.Width > PixelWidth || sourceRect.Y + sourceRect.Height > PixelHeight)
			throw new ArgumentException("The source rectangle is outside the bitmap.", nameof(sourceRect));
		if (offset < 0)
			throw new ArgumentOutOfRangeException(nameof(offset));
		int bpp = Format.BitsPerPixel;
		int rowBytes = (sourceRect.Width * bpp + 7) / 8;
		if (stride < rowBytes)
			throw new ArgumentException($"stride ({stride}) must be >= {rowBytes}.", nameof(stride));
		long bufferBytes = (long)pixels.Length * ElementSize(pixels);
		long required = sourceRect.Height == 0 ? 0 : (long)stride * (sourceRect.Height - 1) + rowBytes;
		if (bufferBytes - offset < required)
			throw new ArgumentException("The buffer is too small.", nameof(pixels));

		byte[] data = GetPixelData();
		int srcStride = PixelConverter.MinStride(PixelWidth, Format);
		var buffer = new byte[bufferBytes];
		Buffer.BlockCopy(pixels, 0, buffer, 0, buffer.Length);
		if (bpp >= 8)
		{
			int bytesPerPixel = bpp / 8;
			for (int y = 0; y < sourceRect.Height; y++)
				Buffer.BlockCopy(data, (sourceRect.Y + y) * srcStride + sourceRect.X * bytesPerPixel,
					buffer, offset + y * stride, rowBytes);
		}
		else
		{
			for (int y = 0; y < sourceRect.Height; y++)
			{
				Array.Clear(buffer, offset + y * stride, rowBytes);
				CopyBitRow(data, (sourceRect.Y + y) * srcStride, sourceRect.X, buffer, offset + y * stride,
					sourceRect.Width, bpp);
			}
		}
		Buffer.BlockCopy(buffer, 0, pixels, 0, buffer.Length);
	}

	static int ElementSize(Array array)
	{
		var type = array.GetType().GetElementType()!;
		if (!type.IsPrimitive)
			throw new ArgumentException("The buffer must be an array of primitive elements.", nameof(array));
		return System.Runtime.InteropServices.Marshal.SizeOf(type);
	}

	static void CopyBitRow(byte[] src, int srcRow, int x0, byte[] dst, int dstRow, int width, int bpp)
	{
		int mask = (1 << bpp) - 1;
		for (int x = 0; x < width; x++)
		{
			int sbit = (x0 + x) * bpp;
			int value = src[srcRow + sbit / 8] >> 8 - bpp - sbit % 8 & mask;
			int dbit = x * bpp;
			dst[dstRow + dbit / 8] |= (byte)(value << 8 - bpp - dbit % 8);
		}
	}
}
