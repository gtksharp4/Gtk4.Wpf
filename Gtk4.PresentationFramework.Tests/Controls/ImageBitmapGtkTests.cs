using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using WpfImage = System.Windows.Controls.Image;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class ImageBitmapGtkTests : WpfGtkTestBase
{
	static BitmapSource Create(int w, int h) =>
		BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgr24, null, new byte[w * h * 3], w * 3);

	[Fact]
	public void In_memory_bitmap_becomes_a_texture_with_its_pixel_size() => Run(() =>
	{
		var img = new WpfImage { Source = Create(7, 5) };
		var pic = (Gtk.Picture)img.Widget;
		Assert.IsType<Gdk.MemoryTexture>(pic.Paintable);
		Assert.Equal(7, pic.Paintable.IntrinsicWidth);
		Assert.Equal(5, pic.Paintable.IntrinsicHeight);
	});

	[Fact]
	public void Source_change_replaces_the_texture_and_null_clears_it() => Run(() =>
	{
		var img = new WpfImage { Source = Create(2, 2) };
		var pic = (Gtk.Picture)img.Widget;
		img.Source = Create(9, 3);
		Assert.Equal(9, pic.Paintable.IntrinsicWidth);
		img.Source = null;
		Assert.Null(pic.Paintable);
	});

	[Fact]
	public void Stretch_None_shows_the_natural_size_by_disabling_shrink() => Run(() =>
	{
		var img = new WpfImage { Source = Create(6, 6), Stretch = Stretch.None };
		var pic = (Gtk.Picture)img.Widget;
		Assert.False(pic.CanShrink);
		img.Stretch = Stretch.Uniform;
		Assert.True(pic.CanShrink);
	});

	[Fact]
	public void A_BitmapImage_that_fails_to_decode_leaves_the_picture_empty() => Run(() =>
	{
		var src = new BitmapImage { StreamSource = new System.IO.MemoryStream(new byte[0]) };
		var img = new WpfImage { Source = src };
		var pic = (Gtk.Picture)img.Widget;
		Assert.Null(pic.Paintable);
	});

	[Fact]
	public void Plain_file_uri_keeps_the_native_file_path() => Run(() =>
	{
		string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "lane-b-" + System.Guid.NewGuid() + ".png");
		var encoder = new PngBitmapEncoder();
		encoder.Frames.Add(BitmapFrame.Create(Create(3, 2)));
		using (var fs = System.IO.File.Create(path))
			encoder.Save(fs);
		try
		{
			var img = new WpfImage { Source = new BitmapImage(new System.Uri(path)) };
			var pic = (Gtk.Picture)img.Widget;
			Assert.NotNull(pic.File);

		}
		finally
		{
			System.IO.File.Delete(path);
		}
	});
}
