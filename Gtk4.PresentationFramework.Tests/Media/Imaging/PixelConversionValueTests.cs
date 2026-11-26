using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

/// <summary>Exact BGRA32 output for the packed and exotic pixel formats.</summary>
public class PixelConversionValueTests
{
	static byte[] Bgra(PixelFormat f, byte[] px, int w = 1, int h = 1, int stride = 0,
		BitmapPalette? palette = null) => BitmapSource
		.Create(w, h, 96, 96, f, palette, px, stride == 0 ? (w * f.BitsPerPixel + 7) / 8 : stride).ToBgra32();

	[Fact]
	public void Bgr565_and_Bgr555_scale_each_channel()
	{
		Assert.Equal(new byte[] { 0, 0, 255, 255 }, Bgra(PixelFormats.Bgr565, new byte[] { 0x00, 0xF8 }));
		Assert.Equal(new byte[] { 0, 255, 0, 255 }, Bgra(PixelFormats.Bgr565, new byte[] { 0xE0, 0x07 }));
		Assert.Equal(new byte[] { 0, 0, 131, 255 }, Bgra(PixelFormats.Bgr565, new byte[] { 0x00, 0x80 }));
		Assert.Equal(new byte[] { 0, 0, 255, 255 }, Bgra(PixelFormats.Bgr555, new byte[] { 0x00, 0x7C }));
		Assert.Equal(new byte[] { 0, 255, 0, 255 }, Bgra(PixelFormats.Bgr555, new byte[] { 0xE0, 0x03 }));
		Assert.Equal(new byte[] { 8, 0, 0, 255 }, Bgra(PixelFormats.Bgr555, new byte[] { 0x01, 0x00 }));
	}

	[Fact]
	public void Rgb24_Gray16_and_Cmyk32_convert_exactly()
	{
		Assert.Equal(new byte[] { 0x30, 0x20, 0x10, 255 }, Bgra(PixelFormats.Rgb24, new byte[] { 0x10, 0x20, 0x30 }));
		Assert.Equal(new byte[] { 0x12, 0x12, 0x12, 255 }, Bgra(PixelFormats.Gray16, new byte[] { 0x34, 0x12 }));
		Assert.Equal(new byte[] { 255, 0, 255, 255 }, Bgra(PixelFormats.Cmyk32, new byte[] { 0, 255, 0, 0 }));
		Assert.Equal(new byte[] { 204, 204, 163, 255 }, Bgra(PixelFormats.Cmyk32, new byte[] { 51, 0, 0, 51 }));
	}

	[Fact]
	public void Indexed2_with_odd_width_and_padded_stride_unpacks_every_pixel()
	{
		var pal = new BitmapPalette(new[] { Colors.Red, Colors.Lime, Colors.Blue, Colors.White });
		var px = new byte[] { 0x1B, 0x40, 0xFF, 0xE4, 0x80, 0x00 };
		var bgra = Bgra(PixelFormats.Indexed2, px, 5, 2, 3, pal);
		int[] expected = { 0, 1, 2, 3, 1, 3, 2, 1, 0, 2 };
		for (int i = 0; i < expected.Length; i++)
		{
			var c = pal.Colors[expected[i]];
			Assert.Equal(new byte[] { c.B, c.G, c.R, c.A }, bgra[(i * 4)..(i * 4 + 4)]);
		}
	}

	[Fact]
	public void Indexed4_with_odd_width_and_padded_stride_unpacks_every_pixel()
	{
		var pal = new BitmapPalette(new[] { Colors.Red, Colors.Lime, Colors.Blue, Colors.White, Colors.Black });
		var px = new byte[] { 0x01, 0x23, 0x40, 0xFF };
		var bgra = Bgra(PixelFormats.Indexed4, px, 5, 1, 4, pal);
		for (int i = 0; i < 5; i++)
		{
			var c = pal.Colors[i];
			Assert.Equal(new byte[] { c.B, c.G, c.R, c.A }, bgra[(i * 4)..(i * 4 + 4)]);
		}
	}

	[Fact]
	public void Gray4_and_Gray2_with_odd_width_and_padded_stride_unpack_every_pixel()
	{
		var g4 = Bgra(PixelFormats.Gray4, new byte[] { 0x12, 0x34, 0x50, 0xFF }, 5, 1, 4);
		for (int i = 0; i < 5; i++)
			Assert.Equal((i + 1) * 17, g4[i * 4]);
		var g2 = Bgra(PixelFormats.Gray2, new byte[] { 0x1B, 0x40, 0xFF }, 5, 1, 3);
		int[] expected = { 0, 85, 170, 255, 85 };
		for (int i = 0; i < 5; i++)
			Assert.Equal(expected[i], g2[i * 4]);
	}
}
