using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;
/// <summary>BitmapImage and in-place metadata over the codec path.</summary>
public partial class CodecRoundTripTests
{
	[Fact]
	public void BitmapImage_decodes_from_a_stream_with_scaling() => Run(() =>
	{
		var png = Save(new PngBitmapEncoder(), Rgb(8, 4));
		var image = new BitmapImage();
		image.BeginInit();
		image.StreamSource = new MemoryStream(png);
		image.DecodePixelWidth = 4;
		image.EndInit();
		Assert.Equal(4, image.PixelWidth);
		Assert.Equal(2, image.PixelHeight);
		Assert.Equal(PixelFormats.Pbgra32, image.Format);

		var plain = new BitmapImage
		{
			StreamSource = new MemoryStream(png),
			CreateOptions = BitmapCreateOptions.PreservePixelFormat,
		};
		Assert.Equal(8, plain.PixelWidth);
		Assert.Equal(PixelFormats.Bgr24, plain.Format);
		Assert.Equal(96, plain.DpiX, 0);
	});

	[Fact]
	public void BitmapImage_decodes_a_file_uri_and_reports_decode_failures() => Run(() =>
	{
		string path = Path.Combine(Path.GetTempPath(), "lane-b-" + Guid.NewGuid().ToString("N") + ".png");
		File.WriteAllBytes(path, Save(new PngBitmapEncoder(), Rgb(3, 2)));
		try
		{
			var image = new BitmapImage(new Uri(path));
			Assert.Equal(3, image.PixelWidth);
			Assert.Equal(2, image.PixelHeight);
		}
		finally { File.Delete(path); }

		var bad = new BitmapImage { StreamSource = new MemoryStream(new byte[8]) };
		Exception? error = null;
		bad.DecodeFailed += (_, e) => error = e.ErrorException;
		Assert.Equal(0, bad.PixelHeight);
		Assert.IsType<NotSupportedException>(error);
	});

	[Fact]
	public void In_place_writer_saves_only_when_nothing_changed() => Run(() =>
	{
		var meta = new BitmapMetadata("png");
		meta.SetQuery("/Text/A", "b");
		var encoder = new PngBitmapEncoder();
		encoder.Frames.Add(BitmapFrame.Create(Rgb(), null, meta, null));
		using var ms = new MemoryStream();
		encoder.Save(ms);
		ms.Position = 0;
		var frame = BitmapFrame.Create(ms);
		var writer = frame.CreateInPlaceBitmapMetadataWriter();
		Assert.True(writer.TrySave());
		writer.SetQuery("/Text/Description", "Have a nice day.");
		Assert.Equal("Have a nice day.", writer.GetQuery("/Text/Description"));
		Assert.False(writer.TrySave());
	});
}
