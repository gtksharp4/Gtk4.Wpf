using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

/// <summary>Shared pixel fixtures for the imaging tests.</summary>
static class ImagingTestData
{
	// 3x2 Bgra32; pixel (x, y) has B = x, G = y, R = 10 * (y * 3 + x), A = 255.
	internal static BitmapSource Source()
	{
		var px = new byte[3 * 2 * 4];
		for (int y = 0; y < 2; y++)
		{
			for (int x = 0; x < 3; x++)
			{
				int o = (y * 3 + x) * 4;
				px[o] = (byte)x;
				px[o + 1] = (byte)y;
				px[o + 2] = (byte)(10 * (y * 3 + x));
				px[o + 3] = 255;
			}
		}
		return BitmapSource.Create(3, 2, 96, 96, PixelFormats.Bgra32, null, px, 12);
	}

	internal static byte[] Pixels(BitmapSource s)
	{
		int stride = (s.PixelWidth * s.Format.BitsPerPixel + 7) / 8;
		var buffer = new byte[stride * s.PixelHeight];
		s.CopyPixels(buffer, stride, 0);
		return buffer;
	}

	internal static byte[] Pixel(BitmapSource s, int x, int y) =>
		Pixels(s).Skip((y * s.PixelWidth + x) * 4).Take(4).ToArray();
}
