using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

public class ImagingApiShapeTests : WpfGtkTestBase
{
	sealed class MyFrame : BitmapFrame { }

	sealed class MyDecoder : BitmapDecoder { }

	sealed class MyEncoder : BitmapEncoder { }

	sealed class MyInfo : BitmapCodecInfo
	{
		public override string Author => "a";
		public override Guid ContainerFormat => Guid.Empty;
		public override string DeviceManufacturer => "";
		public override string DeviceModels => "";
		public override string FileExtensions => ".x";
		public override string FriendlyName => "x";
		public override string MimeTypes => "x/x";
		public override Version SpecificationVersion => new Version(1, 0);
		public override bool SupportsAnimation => false;
		public override bool SupportsLossless => false;
		public override bool SupportsMultipleFrames => false;
		public override Version Version => new Version(1, 0);
	}

	[Fact]
	public void Codec_base_classes_can_be_derived_outside_the_assembly()
	{
		var frame = new MyFrame { BaseUri = new Uri("http://x/y") };
		Assert.Equal("http://x/y", frame.BaseUri!.OriginalString);
		Assert.Empty(new MyDecoder().Frames);
		Assert.Throws<NotSupportedException>(() => new MyDecoder().CodecInfo);
		Assert.Empty(new MyEncoder().Frames);
		Assert.Equal("x", new MyInfo().FriendlyName);
	}

	[Fact]
	public void Frame_metadata_is_typed_ImageMetadata() => Run(() =>
	{
		var meta = new BitmapMetadata("png");
		var frame = BitmapFrame.Create(
			BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[4], 4), null, meta, null);
		ImageMetadata m = frame.Metadata!;
		Assert.Same(meta, m);
	});

	[Fact]
	public void Create_indexed_without_palette_throws_InvalidOperation_and_names_the_buffer_parameter()
	{
		Assert.Throws<InvalidOperationException>(() =>
			BitmapSource.Create(2, 2, 96, 96, PixelFormats.Indexed8, null, new byte[4], 2));
		var ex = Assert.Throws<ArgumentException>(() =>
			BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgr24, null, new byte[8], 6));
		Assert.Equal("pixels", ex.ParamName);
	}

	[Fact]
	public void Width_and_Height_fall_back_to_96_dpi_for_non_positive_dpi()
	{
		var b = BitmapSource.Create(10, 20, 0, 0, PixelFormats.Bgra32, null, new byte[800], 40);
		Assert.Equal(10, b.Width);
		Assert.Equal(20, b.Height);
	}

	[Theory]
	[InlineData("Gray256")]
	[InlineData("Gray4Transparent")]
	[InlineData("Halftone64")]
	[InlineData("WebPaletteTransparent")]
	public void Fixed_palettes_are_cached(string name)
	{
		var p = typeof(BitmapPalettes).GetProperty(name)!;
		Assert.Same(p.GetValue(null), p.GetValue(null));
	}

	[Theory]
	[InlineData("BlackAndWhiteTransparent")]
	[InlineData("Gray4Transparent")]
	[InlineData("Gray16Transparent")]
	[InlineData("Gray256Transparent")]
	public void Transparent_gray_palettes_stay_opaque_like_WIC(string name)
	{
		var palette = (BitmapPalette)typeof(BitmapPalettes).GetProperty(name)!.GetValue(null)!;
		Assert.All(palette.Colors, c => Assert.Equal(255, c.A));
	}
}
