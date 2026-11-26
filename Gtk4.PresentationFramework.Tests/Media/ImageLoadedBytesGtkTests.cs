using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using WpfImage = System.Windows.Controls.Image;

namespace Gtk4.PresentationFramework.Tests.Media;

public class ImageLoadedBytesGtkTests : WpfGtkTestBase
{
	static byte[] Png(int w, int h)
	{
		var encoder = new PngBitmapEncoder();
		encoder.Frames.Add(BitmapFrame.Create(
			BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgr24, null, new byte[w * h * 3], w * 3)));
		using var ms = new MemoryStream();
		encoder.Save(ms);
		return ms.ToArray();
	}

	[Fact]
	public void Clipboard_image_bytes_come_from_an_initialised_stream_source() => Run(() =>
	{
		var png = Png(4, 3);
		var image = new BitmapImage();
		image.BeginInit();
		image.StreamSource = new MemoryStream(png);
		image.EndInit();

		Assert.Equal(png, GtkClipboardBackend.ImageBytes(image));
	});

	[Fact]
	public void OnLoad_uri_image_survives_deleting_the_file() => Run(() =>
	{
		string path = Path.Combine(Path.GetTempPath(), "lane-b-" + Guid.NewGuid() + ".png");
		File.WriteAllBytes(path, Png(5, 2));
		var image = new BitmapImage();
		image.BeginInit();
		image.UriSource = new Uri(path);
		image.CacheOption = BitmapCacheOption.OnLoad;
		image.EndInit();
		File.Delete(path);

		var pic = (Gtk.Picture)new WpfImage { Source = image }.Widget;
		Assert.IsType<Gdk.MemoryTexture>(pic.Paintable);
		Assert.Equal(5, pic.Paintable.IntrinsicWidth);
		Assert.Equal(2, pic.Paintable.IntrinsicHeight);
	});
}
