using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

/// <summary>EndInit reads the source bytes immediately, so the stream or file can go away afterwards.</summary>
public class BitmapImageLoadTests : WpfGtkTestBase
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
	public void Stream_source_is_read_at_EndInit_and_survives_disposal() => Run(() =>
	{
		var stream = new MemoryStream(Png(6, 4));
		var image = new BitmapImage();
		bool failed = false;
		image.DecodeFailed += (_, _) => failed = true;
		image.BeginInit();
		image.CacheOption = BitmapCacheOption.OnLoad;
		image.StreamSource = stream;
		image.EndInit();
		stream.Dispose();

		Assert.Equal(6, image.PixelWidth);
		Assert.Equal(4, image.PixelHeight);
		Assert.False(failed);
	});

	[Fact]
	public void OnLoad_file_is_read_at_EndInit_and_survives_deletion() => Run(() =>
	{
		string path = Path.Combine(Path.GetTempPath(), "lane-b-" + Guid.NewGuid() + ".png");
		File.WriteAllBytes(path, Png(5, 3));
		var image = new BitmapImage();
		image.BeginInit();
		image.CacheOption = BitmapCacheOption.OnLoad;
		image.UriSource = new Uri(path);
		image.EndInit();
		File.Delete(path);

		Assert.Equal(5, image.PixelWidth);
		Assert.Equal(3, image.PixelHeight);
	});

	[Fact]
	public void Missing_or_pack_relative_uri_does_not_throw_at_EndInit() => Run(() =>
	{
		foreach (var uri in new[] { new Uri("missing-lane-b.png", UriKind.Relative),
			new Uri("/Nope;component/x.png", UriKind.Relative) })
		{
			var image = new BitmapImage();
			image.BeginInit();
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.UriSource = uri;
			image.EndInit();
			Assert.Equal(0, image.PixelWidth);
		}
	});
}
