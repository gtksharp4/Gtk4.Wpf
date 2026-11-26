using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using WpfImage = System.Windows.Controls.Image;

namespace Gtk4.PresentationFramework.Tests.Imaging;

/// <summary>Sources the samples build directly, relative uris, and Source changes before and during display.</summary>
public class ImageDisplaySourceTests : WpfGtkTestBase
{
	static (byte B, byte G, byte R, byte A) Centre(ImageDisplayProbe probe)
	{
		var pixels = probe.ImagePixels();
		return pixels.At(pixels.Width / 2, pixels.Height / 2);
	}

	[Fact]
	public void An_all_zero_Indexed1_buffer_paints_solid_red_at_128x128() => Run(() =>
	{
		var palette = new BitmapPalette(new[] { Colors.Red, Colors.Blue, Colors.Green });
		var source = BitmapSource.Create(128, 128, 96, 96, PixelFormats.Indexed1, palette, new byte[16 * 128], 16);
		using var probe = new ImageDisplayProbe(source);
		Assert.Equal(128, probe.Image.ActualWidth);
		Assert.Equal(128, probe.Image.ActualHeight);
		var pixels = probe.ImagePixels();
		Assert.Equal(128 * 128, pixels.NonTransparent());
		Assert.Equal(((byte)0, (byte)0, (byte)255, (byte)255), pixels.At(64, 64));
	});

	[Fact]
	public void BitmapFrame_Create_of_a_bitmap_is_drawn() => Run(() =>
	{
		using var probe = new ImageDisplayProbe(BitmapFrame.Create(ImageDisplayFixtures.Quadrant()));
		Assert.Equal(ImageDisplayFixtures.Size, probe.Image.ActualWidth);
		ImageDisplayFixtures.AssertQuadrants(probe.ImagePixels(), 0);
	});

	[Fact]
	public void A_BitmapImage_from_a_relative_uri_next_to_the_app_is_drawn() => Run(() =>
	{
		string name = "lane-e-" + Guid.NewGuid().ToString("N") + ".png";
		string path = Path.Combine(AppContext.BaseDirectory, name);
		File.WriteAllBytes(path, ImageDisplayFixtures.Encode("PNG", ImageDisplayFixtures.Quadrant()));
		try
		{
			using var probe = new ImageDisplayProbe(new BitmapImage(new Uri(name, UriKind.Relative)));
			GtkPumpFor(100);
			Assert.Equal(ImageDisplayFixtures.Size, probe.Image.ActualWidth);
			ImageDisplayFixtures.AssertQuadrants(probe.ImagePixels(), 0);
		}
		finally { File.Delete(path); }
	});

	[Fact]
	public void A_BitmapImage_from_an_absolute_file_uri_is_drawn() => Run(() =>
	{
		string path = Path.Combine(Path.GetTempPath(), "lane-e-" + Guid.NewGuid().ToString("N") + ".png");
		File.WriteAllBytes(path, ImageDisplayFixtures.Encode("PNG", ImageDisplayFixtures.Quadrant()));
		try
		{
			using var probe = new ImageDisplayProbe(new BitmapImage(new Uri(path)));
			ImageDisplayFixtures.AssertQuadrants(probe.ImagePixels(), 0);
		}
		finally { File.Delete(path); }
	});

	[Fact]
	public void A_source_set_after_the_window_is_shown_is_drawn_and_replacing_it_redraws() => Run(() =>
	{
		using var probe = new ImageDisplayProbe(null);
		probe.Image.Source = ImageDisplayFixtures.Quadrant();
		GtkPumpFor(200);
		Assert.Equal(ImageDisplayFixtures.Size, probe.Image.ActualWidth);
		ImageDisplayFixtures.AssertQuadrants(probe.ImagePixels(), 0);

		var red = BitmapSource.Create(32, 32, 96, 96, PixelFormats.Bgr32, null, RedPixels(32), 128);
		probe.Image.Source = red;
		GtkPumpFor(200);
		Assert.Equal(32, probe.Image.ActualWidth);
		Assert.Equal(((byte)0, (byte)0, (byte)255, (byte)255), Centre(probe));
	});

	static byte[] RedPixels(int size)
	{
		var px = new byte[size * size * 4];
		for (int i = 0; i < px.Length; i += 4)
			px[i + 2] = 255;
		return px;
	}

	static void GtkPumpFor(int ms) => Lifecycle.GtkPump.For(ms);

	[Theory]
	[InlineData(Stretch.Uniform)]
	[InlineData(Stretch.Fill)]
	[InlineData(Stretch.UniformToFill)]
	public void Stretching_modes_still_fill_the_slot_and_draw(Stretch stretch) => Run(() =>
	{
		using var probe = new ImageDisplayProbe(ImageDisplayFixtures.Quadrant(), stretch);
		var pic = (Gtk.Picture)probe.Image.Widget;
		Assert.True(pic.CanShrink);
		Assert.Equal(Gtk.Align.Fill, pic.Halign);
		Assert.True(probe.Image.ActualWidth > ImageDisplayFixtures.Size);
		Assert.True(probe.ImagePixels().NonTransparent() > 0);
	});
}
