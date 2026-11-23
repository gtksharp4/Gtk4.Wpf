using System.Runtime.InteropServices;

namespace System.Windows.Media.Imaging;

public abstract partial class BitmapSource
{
	/// <summary>Creates a bitmap from a pixel buffer.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="pixels"/> is null.</exception>
	/// <exception cref="ArgumentException">The stride or buffer is too small.</exception>
	/// <exception cref="InvalidOperationException">An indexed <paramref name="pixelFormat"/> has no
	/// palette.</exception>
	public static BitmapSource Create(int pixelWidth, int pixelHeight, double dpiX, double dpiY,
		PixelFormat pixelFormat, BitmapPalette? palette, Array pixels, int stride)
	{
		if (pixels == null)
			throw new ArgumentNullException(nameof(pixels));
		var bytes = new byte[(long)pixels.Length * ElementSize(pixels)];
		Buffer.BlockCopy(pixels, 0, bytes, 0, bytes.Length);
		return CreateFromBytes(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette, bytes, stride,
			nameof(pixels));
	}

	/// <summary>Creates a bitmap from unmanaged pixel memory.</summary>
	public static BitmapSource Create(int pixelWidth, int pixelHeight, double dpiX, double dpiY,
		PixelFormat pixelFormat, BitmapPalette? palette, IntPtr buffer, int bufferSize, int stride)
	{
		if (buffer == IntPtr.Zero)
			throw new ArgumentNullException(nameof(buffer));
		if (bufferSize < 0)
			throw new ArgumentOutOfRangeException(nameof(bufferSize));
		var bytes = new byte[bufferSize];
		Marshal.Copy(buffer, bytes, 0, bufferSize);
		return CreateFromBytes(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette, bytes, stride,
			nameof(buffer));
	}

	static BitmapSource CreateFromBytes(int width, int height, double dpiX, double dpiY, PixelFormat format,
		BitmapPalette? palette, byte[] bytes, int stride, string bufferName)
	{
		if (width <= 0)
			throw new ArgumentOutOfRangeException(nameof(width), "Must be > 0.");
		if (height <= 0)
			throw new ArgumentOutOfRangeException(nameof(height), "Must be > 0.");
		if (format.BitsPerPixel == 0)
			throw new ArgumentException("The pixel format is not supported.", nameof(format));
		int rowBytes = PixelConverter.MinStride(width, format);
		if (stride < rowBytes)
			throw new ArgumentException($"stride ({stride}) must be >= {rowBytes}.", nameof(stride));
		if ((long)stride * (height - 1) + rowBytes > bytes.Length)
			throw new ArgumentException("The buffer is too small.", bufferName);
		if (format.Name.StartsWith("Indexed", StringComparison.Ordinal) && palette == null)
			throw new InvalidOperationException("An indexed pixel format requires a palette.");

		var data = new byte[rowBytes * height];
		for (int y = 0; y < height; y++)
			Buffer.BlockCopy(bytes, y * stride, data, y * rowBytes, rowBytes);
		return new MemoryBitmapSource(width, height, dpiX, dpiY, format, palette, data);
	}
}

/// <summary>A bitmap whose pixels are held in managed memory.</summary>
sealed class MemoryBitmapSource : BitmapSource
{
	readonly byte[] _data;

	internal MemoryBitmapSource(int width, int height, double dpiX, double dpiY, PixelFormat format,
		BitmapPalette? palette, byte[] data)
	{
		PixelWidth = width;
		PixelHeight = height;
		DpiX = dpiX;
		DpiY = dpiY;
		Format = format;
		Palette = format.Name.StartsWith("Indexed", StringComparison.Ordinal) ? palette : null;
		_data = data;
	}

	internal override byte[] GetPixelData() => _data;

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() =>
		new MemoryBitmapSource(PixelWidth, PixelHeight, DpiX, DpiY, Format, Palette, (byte[])_data.Clone());
}
