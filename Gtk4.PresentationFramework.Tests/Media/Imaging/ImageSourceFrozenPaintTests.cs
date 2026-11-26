using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using WpfImage = System.Windows.Controls.Image;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

/// <summary>A frozen bitmap still paints through the real GTK texture path.</summary>
public class ImageSourceFrozenPaintTests : WpfGtkTestBase
{
	static RenderTargetBitmap FrozenRed()
	{
		var dv = new DrawingVisual();
		using (var scope = dv.RenderOpen())
			scope.Context.DrawRectangle(new SolidColorBrush(Colors.Red), null, new Rect(0, 0, 4, 4));
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		rtb.Render(dv);
		rtb.Freeze();
		return rtb;
	}

	static (byte B, byte G, byte R, byte A) PaintBrush(ImageBrush brush)
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
	public void A_frozen_RenderTargetBitmap_paints_red_through_ImageBrush() => Run(() =>
	{
		var rtb = FrozenRed();
		Assert.True(rtb.IsFrozen);
		Assert.Equal((0, 0, 255, 255), PaintBrush(new ImageBrush(rtb) { Stretch = Stretch.Fill }));
	});

	[Fact]
	public void A_frozen_RenderTargetBitmap_becomes_a_red_texture_through_Image_Source() => Run(() =>
	{
		var rtb = FrozenRed();
		var image = new WpfImage { Source = rtb };
		var texture = Assert.IsAssignableFrom<Gdk.Texture>(((Gtk.Picture)image.Widget).Paintable);
		Assert.Equal(4, texture.Width);
		using var png = new MemoryStream(texture.SaveToPngBytes().Data);
		var frame = BitmapFrame.Create(png, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
		var pixel = new byte[4];
		frame.CopyPixels(new Int32Rect(1, 1, 1, 1), pixel, 4, 0);
		Assert.Equal(new byte[] { 0, 0, 255, 255 }, pixel);
	});
}
