using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

/// <summary>Encoder output decodes back to the same pixels, through the real gdk-pixbuf runtime.</summary>
public partial class CodecRoundTripTests : WpfGtkTestBase
{
	static BitmapSource Rgb(int w = 4, int h = 3)
	{
		var px = new byte[w * h * 3];
		for (int i = 0; i < px.Length; i++)
			px[i] = (byte)(i * 7 + 3);
		return BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgr24, null, px, w * 3);
	}

	static BitmapSource Argb()
	{
		var px = new byte[4 * 2 * 4];
		for (int i = 0; i < px.Length; i += 4)
		{
			px[i] = (byte)(i * 5);
			px[i + 1] = (byte)(i * 3 + 1);
			px[i + 2] = (byte)(255 - i);
			px[i + 3] = (byte)(i % 8 == 0 ? 255 : 128);
		}
		return BitmapSource.Create(4, 2, 96, 96, PixelFormats.Bgra32, null, px, 16);
	}

	static byte[] Save(BitmapEncoder encoder, params BitmapSource[] sources)
	{
		foreach (var s in sources)
			encoder.Frames.Add(BitmapFrame.Create(s));
		using var ms = new MemoryStream();
		encoder.Save(ms);
		return ms.ToArray();
	}

	[Fact]
	public void Png_round_trips_alpha_pixels_text_and_dpi() => Run(() =>
	{
		var src = Argb();
		var meta = new BitmapMetadata("png");
		meta.SetQuery("/Text/Description", "Have a nice day.");
		var encoder = new PngBitmapEncoder();
		encoder.Frames.Add(BitmapFrame.Create(src, null, meta, null));
		using var ms = new MemoryStream();
		encoder.Save(ms);
		ms.Position = 0;

		var frame = new PngBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default)
			.Frames[0];
		Assert.Equal(src.ToBgra32(), frame.ToBgra32());
		Assert.Equal(PixelFormats.Bgra32, frame.Format);
		Assert.Equal("Have a nice day.", ((BitmapMetadata)frame.Metadata!).GetQuery("/Text/Description"));
		Assert.True(frame.Metadata!.IsFrozen);
		Assert.Equal(96, frame.DpiX, 0);
		Assert.Throws<InvalidOperationException>(() => ((BitmapMetadata)frame.Metadata).SetQuery("/Text/X", "y"));
	});

	[Fact]
	public void Png_without_alpha_decodes_as_Bgr24() => Run(() =>
	{
		var src = Rgb();
		var frame = BitmapDecoder.Create(new MemoryStream(Save(new PngBitmapEncoder(), src)),
			BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
		Assert.Equal(PixelFormats.Bgr24, frame.Format);
		Assert.Equal(src.ToBgra32(), frame.ToBgra32());
	});

	[Theory]
	[InlineData(TiffCompressOption.Default)]
	[InlineData(TiffCompressOption.None)]
	[InlineData(TiffCompressOption.Lzw)]
	[InlineData(TiffCompressOption.Zip)]
	[InlineData(TiffCompressOption.Ccitt4)]
	public void Tiff_round_trips_pixels(TiffCompressOption compression) => Run(() =>
	{
		var src = Rgb();
		var encoder = new TiffBitmapEncoder { Compression = compression };
		var decoder = new TiffBitmapDecoder(new MemoryStream(Save(encoder, src)), BitmapCreateOptions.None,
			BitmapCacheOption.Default);
		Assert.Equal(src.ToBgra32(), decoder.Frames[0].ToBgra32());
		Assert.Equal("TIFF Decoder", decoder.CodecInfo.FriendlyName);
	});

	[Fact]
	public void Bmp_round_trips_true_colour_and_indexed() => Run(() =>
	{
		var src = Rgb(5, 3);
		var frame = new BmpBitmapDecoder(new MemoryStream(Save(new BmpBitmapEncoder(), src)),
			BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
		Assert.Equal(PixelFormats.Bgr24, frame.Format);
		Assert.Equal(src.ToBgra32(), frame.ToBgra32());

		var pal = new BitmapPalette(new[] { Colors.Red, Colors.Lime, Colors.Blue, Colors.White });
		var data = new byte[] { 0x01, 0x23, 0x10, 0x32, 0x00, 0x00 };
		var idx = BitmapSource.Create(5, 2, 96, 96, PixelFormats.Indexed4, pal, data, 3);
		var back = new BmpBitmapDecoder(new MemoryStream(Save(new BmpBitmapEncoder(), idx)),
			BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
		Assert.Equal(PixelFormats.Indexed4, back.Format);
		Assert.Equal(idx.ToBgra32(), back.ToBgra32());
	});

	[Fact]
	public void Gif_round_trips_indexed_frames() => Run(() =>
	{
		var pal = new BitmapPalette(new[] { Colors.Red, Colors.Lime, Colors.Blue });
		var px = new byte[64 * 64];
		for (int i = 0; i < px.Length; i++)
			px[i] = (byte)(i * 31 / 7 % 3);
		var one = BitmapSource.Create(64, 64, 96, 96, PixelFormats.Indexed8, pal, px, 64);
		var two = BitmapSource.Create(2, 2, 96, 96, PixelFormats.Indexed8, pal, new byte[] { 2, 1, 0, 2 }, 2);
		var bytes = Save(new GifBitmapEncoder(), one, two);
		var decoder = new GifBitmapDecoder(new MemoryStream(bytes), BitmapCreateOptions.None,
			BitmapCacheOption.Default);
		Assert.Equal(2, decoder.Frames.Count);
		Assert.Equal(PixelFormats.Indexed8, decoder.Frames[0].Format);
		Assert.Equal(one.ToBgra32(), decoder.Frames[0].ToBgra32());
		Assert.Equal(two.ToBgra32(), decoder.Frames[1].ToBgra32());
		Assert.True(decoder.CodecInfo.SupportsAnimation);
	});

	[Fact]
	public void Gif_keeps_a_transparent_palette_entry_and_quantises_true_colour() => Run(() =>
	{
		var pal = new BitmapPalette(new[] { Color.FromArgb(0, 1, 2, 3), Colors.Red });
		var idx = BitmapSource.Create(2, 1, 96, 96, PixelFormats.Indexed1, pal, new byte[] { 0x40 }, 1);
		var back = BitmapDecoder.Create(new MemoryStream(Save(new GifBitmapEncoder(), idx)), BitmapCreateOptions.None,
			BitmapCacheOption.Default).Frames[0];
		Assert.Equal(0, back.ToBgra32()[3]);
		Assert.Equal(255, back.ToBgra32()[7]);

		var rgb = Rgb();
		var q = BitmapDecoder.Create(new MemoryStream(Save(new GifBitmapEncoder(), rgb)), BitmapCreateOptions.None,
			BitmapCacheOption.Default).Frames[0];
		Assert.Equal(rgb.ToBgra32(), q.ToBgra32());
	});

	[Fact]
	public void Jpeg_encodes_with_quality_rotation_and_flip() => Run(() =>
	{
		var src = Rgb(8, 4);
		var encoder = new JpegBitmapEncoder { QualityLevel = 90, Rotation = Rotation.Rotate90 };
		var frame = new JpegBitmapDecoder(new MemoryStream(Save(encoder, src)), BitmapCreateOptions.None,
			BitmapCacheOption.Default).Frames[0];
		Assert.Equal(4, frame.PixelWidth);
		Assert.Equal(8, frame.PixelHeight);
		Assert.Equal(75, new JpegBitmapEncoder().QualityLevel);
		Assert.Throws<ArgumentOutOfRangeException>(() => new JpegBitmapEncoder().QualityLevel = 0);
		Assert.Throws<ArgumentOutOfRangeException>(() => new JpegBitmapEncoder().QualityLevel = 101);

		var flipped = new JpegBitmapEncoder { FlipHorizontal = true };
		var f2 = BitmapDecoder.Create(new MemoryStream(Save(flipped, src)), BitmapCreateOptions.None,
			BitmapCacheOption.Default).Frames[0];
		Assert.Equal(8, f2.PixelWidth);
	});
}
