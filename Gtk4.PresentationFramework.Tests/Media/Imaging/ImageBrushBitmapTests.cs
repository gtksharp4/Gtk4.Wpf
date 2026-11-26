using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

/// <summary>ImageBrush paints any decoded bitmap, not only PNG files.</summary>
public class ImageBrushBitmapTests : WpfGtkTestBase
{
	static (byte B, byte G, byte R, byte A) Paint(ImageBrush brush)
	{
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 4, 4);
		using (var cr = new Cairo.Context(surface))
		{
			brush.ApplyToCairo(cr, 4, 4);
			cr.Paint();
		}
		surface.Flush();
		var d = surface.Data;
		int o = 1 * surface.Stride + 1 * 4;
		return (d[o], d[o + 1], d[o + 2], d[o + 3]);
	}

	[Fact]
	public void Solid_red_BitmapSource_paints_red() => Run(() =>
	{
		var px = new byte[2 * 2 * 4];
		for (int i = 0; i < px.Length; i += 4)
		{
			px[i + 2] = 255;
			px[i + 3] = 255;
		}
		var src = BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgra32, null, px, 8);
		var (b, g, r, a) = Paint(new ImageBrush(src) { Stretch = Stretch.Fill });
		Assert.Equal((0, 0, 255, 255), (b, g, r, a));
	});

	[Theory]
	[InlineData("bmp")]
	[InlineData("jpg")]
	public void BitmapImage_of_a_non_PNG_file_paints(string kind) => Run(() =>
	{
		var px = new byte[8 * 8 * 3];
		for (int i = 0; i < px.Length; i += 3)
			px[i + 2] = 255;
		BitmapEncoder encoder = kind == "bmp" ? new BmpBitmapEncoder() : new JpegBitmapEncoder();
		encoder.Frames.Add(BitmapFrame.Create(
			BitmapSource.Create(8, 8, 96, 96, PixelFormats.Bgr24, null, px, 24)));
		string path = Path.Combine(Path.GetTempPath(), "lane-b-" + Guid.NewGuid() + "." + kind);
		using (var fs = File.Create(path))
			encoder.Save(fs);
		try
		{
			var (b, g, r, a) = Paint(new ImageBrush(new BitmapImage(new Uri(path))) { Stretch = Stretch.Fill });
			Assert.True(r > 200 && g < 60 && b < 60 && a == 255, $"painted {r},{g},{b},{a}");
		}
		finally
		{
			File.Delete(path);
		}
	});
}
