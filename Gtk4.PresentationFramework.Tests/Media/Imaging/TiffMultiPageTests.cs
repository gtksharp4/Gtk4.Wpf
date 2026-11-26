using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

public class TiffMultiPageTests : WpfGtkTestBase
{
	static BitmapSource Solid(int w, int h, byte b, byte g, byte r, byte a = 255)
	{
		var px = new byte[w * h * 4];
		for (int i = 0; i < px.Length; i += 4)
		{
			px[i] = b;
			px[i + 1] = g;
			px[i + 2] = r;
			px[i + 3] = a;
		}
		return BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, px, w * 4);
	}

	static byte[] Save(TiffBitmapEncoder encoder, params BitmapSource[] pages)
	{
		foreach (var p in pages)
			encoder.Frames.Add(BitmapFrame.Create(p));
		using var ms = new MemoryStream();
		encoder.Save(ms);
		return ms.ToArray();
	}

	[Theory]
	[InlineData(TiffCompressOption.None)]
	[InlineData(TiffCompressOption.Lzw)]
	[InlineData(TiffCompressOption.Zip)]
	public void Three_pages_of_different_size_and_colour_round_trip(TiffCompressOption compression) => Run(() =>
	{
		var bytes = Save(new TiffBitmapEncoder { Compression = compression },
			Solid(4, 3, 0, 0, 255), Solid(7, 2, 0, 255, 0), Solid(2, 5, 255, 0, 0));
		var decoder = new TiffBitmapDecoder(new MemoryStream(bytes), BitmapCreateOptions.None,
			BitmapCacheOption.Default);

		Assert.Equal(3, decoder.Frames.Count);
		Assert.Equal((4, 3), (decoder.Frames[0].PixelWidth, decoder.Frames[0].PixelHeight));
		Assert.Equal((7, 2), (decoder.Frames[1].PixelWidth, decoder.Frames[1].PixelHeight));
		Assert.Equal((2, 5), (decoder.Frames[2].PixelWidth, decoder.Frames[2].PixelHeight));
		Assert.Equal(new byte[] { 0, 0, 255, 255 }, decoder.Frames[0].ToBgra32()[..4]);
		Assert.Equal(new byte[] { 0, 255, 0, 255 }, decoder.Frames[1].ToBgra32()[..4]);
		Assert.Equal(new byte[] { 255, 0, 0, 255 }, decoder.Frames[2].ToBgra32()[..4]);
	});

	[Fact]
	public void Alpha_survives_and_large_noisy_data_exercises_LZW_table_resets() => Run(() =>
	{
		var rnd = new System.Random(7);
		var px = new byte[200 * 100 * 4];
		rnd.NextBytes(px);
		for (int i = 3; i < px.Length; i += 4)
			px[i] = 255;
		var noisy = BitmapSource.Create(200, 100, 96, 96, PixelFormats.Bgra32, null, px, 800);
		var bytes = Save(new TiffBitmapEncoder { Compression = TiffCompressOption.Lzw }, noisy,
			Solid(2, 2, 1, 2, 3, 128));
		var decoder = new TiffBitmapDecoder(new MemoryStream(bytes), BitmapCreateOptions.None,
			BitmapCacheOption.Default);

		Assert.Equal(2, decoder.Frames.Count);
		Assert.Equal(noisy.ToBgra32(), decoder.Frames[0].ToBgra32());
		Assert.Equal(128, decoder.Frames[1].ToBgra32()[3]);
		Assert.True(decoder.CodecInfo.SupportsMultipleFrames);
	});
}
