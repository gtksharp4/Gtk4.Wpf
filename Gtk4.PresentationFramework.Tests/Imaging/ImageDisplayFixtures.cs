using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Imaging;

/// <summary>A non-uniform test image, the codecs and the colour assertions of the display tests.</summary>
static class ImageDisplayFixtures
{
	public const int Size = 64;

	// TL red, TR blue, BL green, BR white.
	public static readonly (int X, int Y, byte R, byte G, byte B)[] Quadrants =
	{
		(16, 16, 255, 0, 0), (48, 16, 0, 0, 255), (16, 48, 0, 255, 0), (48, 48, 255, 255, 255),
	};

	public static BitmapSource Quadrant()
	{
		const int stride = Size * 3;
		var px = new byte[stride * Size];
		for (int y = 0; y < Size; y++)
			for (int x = 0; x < Size; x++)
			{
				bool right = x >= Size / 2, bottom = y >= Size / 2;
				byte r = (byte)(!right && !bottom || right && bottom ? 255 : 0);
				byte g = (byte)(!right && bottom || right && bottom ? 255 : 0);
				byte b = (byte)(right ? 255 : 0);
				int o = y * stride + x * 3;
				px[o] = b;
				px[o + 1] = g;
				px[o + 2] = r;
			}
		return BitmapSource.Create(Size, Size, 96, 96, PixelFormats.Bgr24, null, px, stride);
	}

	public static BitmapEncoder Encoder(string codec) => codec switch
	{
		"BMP" => new BmpBitmapEncoder(),
		"GIF" => new GifBitmapEncoder(),
		"JPEG" => new JpegBitmapEncoder(),
		"PNG" => new PngBitmapEncoder(),
		"TIFF" => new TiffBitmapEncoder(),
		_ => throw new ArgumentException(codec),
	};

	public static string Extension(string codec) =>
		codec == "JPEG" ? "jpg" : codec == "TIFF" ? "tif" : codec.ToLowerInvariant();

	public static byte[] Encode(string codec, BitmapSource source)
	{
		var encoder = Encoder(codec);
		encoder.Frames.Add(BitmapFrame.Create(source));
		using var ms = new MemoryStream();
		encoder.Save(ms);
		return ms.ToArray();
	}

	public static BitmapDecoder Decode(string codec, Stream stream) => codec switch
	{
		"BMP" => new BmpBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default),
		"GIF" => new GifBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default),
		"JPEG" => new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default),
		"PNG" => new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default),
		_ => new TiffBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default),
	};

	public static BitmapDecoder Decode(string codec, Uri uri) => codec switch
	{
		"BMP" => new BmpBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default),
		"GIF" => new GifBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default),
		"JPEG" => new JpegBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default),
		"PNG" => new PngBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default),
		_ => new TiffBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default),
	};

	public static void AssertQuadrants(WidgetPixels pixels, int tolerance, int xOffset = 0, int yOffset = 0)
	{
		foreach (var (x, y, r, g, b) in Quadrants)
		{
			var (pb, pg, pr, pa) = pixels.At(x + xOffset, y + yOffset);
			bool ok = pa == 255 && Math.Abs(pr - r) <= tolerance && Math.Abs(pg - g) <= tolerance
				&& Math.Abs(pb - b) <= tolerance;
			Assert.True(ok, $"pixel ({x},{y}) was rgba({pr},{pg},{pb},{pa}), expected rgb({r},{g},{b})");
		}
	}
}
