using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

public partial class CodecRoundTripTests
{
	[Fact]
	public void Pixel_transforms_rotate_and_flip_the_pixels()
	{
		var px = new byte[] { 1, 1, 1, 255, 2, 2, 2, 255 };
		var (r, w, h) = PixelTransforms.Apply(px, 2, 1, Rotation.Rotate90, false, false);
		Assert.Equal((1, 2), (w, h));
		Assert.Equal(new byte[] { 1, 1, 1, 255, 2, 2, 2, 255 }, r);
		var (f, _, _) = PixelTransforms.Apply(px, 2, 1, Rotation.Rotate0, true, false);
		Assert.Equal(new byte[] { 2, 2, 2, 255, 1, 1, 1, 255 }, f);
		var (t, _, _) = PixelTransforms.Apply(px, 2, 1, Rotation.Rotate180, false, false);
		Assert.Equal(f, t);
	}

	[Fact]
	public void Decoder_create_sniffs_the_container_and_rejects_bad_data() => Run(() =>
	{
		var png = Save(new PngBitmapEncoder(), Rgb());
		Assert.IsType<PngBitmapDecoder>(BitmapDecoder.Create(new MemoryStream(png), BitmapCreateOptions.None,
			BitmapCacheOption.Default));
		var jpeg = Save(new JpegBitmapEncoder(), Rgb());
		Assert.IsType<JpegBitmapDecoder>(BitmapDecoder.Create(new MemoryStream(jpeg), BitmapCreateOptions.None,
			BitmapCacheOption.Default));
		Assert.Throws<NotSupportedException>(() => BitmapDecoder.Create(new MemoryStream(new byte[16]),
			BitmapCreateOptions.None, BitmapCacheOption.Default));
		Assert.Throws<FileFormatException>(() => new PngBitmapDecoder(new MemoryStream(jpeg),
			BitmapCreateOptions.None, BitmapCacheOption.Default));
		Assert.Throws<FileFormatException>(() => new WmpBitmapDecoder(new MemoryStream(jpeg),
			BitmapCreateOptions.None, BitmapCacheOption.Default));
		Assert.Throws<FileNotFoundException>(() => BitmapDecoder.Create(new Uri("missing-lane-b.png", UriKind.Relative),
			BitmapCreateOptions.None, BitmapCacheOption.Default));
	});

	[Fact]
	public void Wdp_is_the_documented_gap() => Run(() =>
	{
		var wdp = new byte[] { 0x49, 0x49, 0xBC, 0x01, 0, 0, 0, 0 };
		Assert.Throws<NotSupportedException>(() => new WmpBitmapDecoder(new MemoryStream(wdp),
			BitmapCreateOptions.None, BitmapCacheOption.Default));
		var encoder = new WmpBitmapEncoder { Lossless = true };
		encoder.Frames.Add(BitmapFrame.Create(Rgb()));
		Assert.Throws<NotSupportedException>(() => encoder.Save(new MemoryStream()));
		Assert.Equal(new Guid("57a37caa-367a-4540-916b-f183c5093a4b"), encoder.CodecInfo.ContainerFormat);
	});

	[Fact]
	public void Encoder_save_state_rules_and_codec_info() => Run(() =>
	{
		var encoder = new PngBitmapEncoder();
		Assert.Throws<NotSupportedException>(() => encoder.Save(new MemoryStream()));
		encoder.Frames.Add(BitmapFrame.Create(Rgb()));
		encoder.Save(new MemoryStream());
		Assert.Throws<InvalidOperationException>(() => encoder.Save(new MemoryStream()));
		var info = encoder.CodecInfo;
		Assert.Equal("Microsoft", info.Author);
		Assert.Equal(".png", info.FileExtensions);
		Assert.Equal("image/png", info.MimeTypes);
		Assert.Equal("PNG Encoder", info.FriendlyName);
		Assert.Equal(new Guid("1b7cfaf4-713f-473c-bbcd-6137425faeaf"), info.ContainerFormat);
		Assert.True(info.SupportsLossless);
		Assert.False(new JpegBitmapEncoder().CodecInfo.SupportsLossless);
		Assert.True(new GifBitmapEncoder().CodecInfo.SupportsMultipleFrames);
	});
}
