using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

public class BitmapSourceTests
{
	static BitmapSource Bgr24(int w, int h) =>
		BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgr24, null, new byte[w * 3 * h], w * 3);

	[Fact]
	public void Create_reports_size_format_and_dpi()
	{
		var b = BitmapSource.Create(4, 2, 192, 48, PixelFormats.Bgr24, null, new byte[24], 12);
		Assert.Equal(4, b.PixelWidth);
		Assert.Equal(PixelFormats.Bgr24, b.Format);
		Assert.Equal(2d, b.Width);
		Assert.Equal(4d, b.Height);
		Assert.Null(b.Palette);
	}

	[Fact]
	public void Create_validates_stride_buffer_and_palette()
	{
		Assert.Throws<ArgumentNullException>(() =>
			BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgr24, null, (Array)null!, 6));
		Assert.Throws<ArgumentException>(() =>
			BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgr24, null, new byte[12], 5));
		Assert.Throws<ArgumentException>(() =>
			BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgr24, null, new byte[8], 6));
		Assert.Throws<InvalidOperationException>(() =>
			BitmapSource.Create(2, 2, 96, 96, PixelFormats.Indexed8, null, new byte[4], 2));
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			BitmapSource.Create(0, 2, 96, 96, PixelFormats.Bgr24, null, new byte[12], 6));
	}

	[Fact]
	public void CopyPixels_round_trips_and_honours_offset_and_rect()
	{
		var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
		var b = BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgr24, null, data, 6);
		var all = new byte[14];
		b.CopyPixels(all, 6, 2);
		Assert.Equal(data, all[2..]);
		var part = new byte[3];
		b.CopyPixels(new Int32Rect(1, 1, 1, 1), part, 3, 0);
		Assert.Equal(new byte[] { 10, 11, 12 }, part);
		Assert.Throws<ArgumentException>(() => b.CopyPixels(new byte[11], 6, 0));
		Assert.Throws<ArgumentException>(() => b.CopyPixels(new byte[12], 5, 0));
	}

	[Fact]
	public void CopyPixels_into_an_int_array_and_packed_formats()
	{
		var b = BitmapSource.Create(2, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 1, 0, 0, 0, 2, 0, 0, 0 }, 8);
		var ints = new int[2];
		b.CopyPixels(ints, 8, 0);
		Assert.Equal(new[] { 1, 2 }, ints);

		var bits = BitmapSource.Create(10, 1, 96, 96, PixelFormats.Indexed1, BitmapPalettes.BlackAndWhite,
			new byte[] { 0b1010_0000, 0b0100_0000 }, 2);
		var row = new byte[2];
		bits.CopyPixels(new Int32Rect(1, 0, 9, 1), row, 2, 0);
		Assert.Equal(new byte[] { 0b0100_0000, 0b1000_0000 }, row);
	}

	[Theory]
	[InlineData("Indexed1")]
	[InlineData("Gray8")]
	[InlineData("Bgr565")]
	[InlineData("Bgr555")]
	[InlineData("Rgb24")]
	[InlineData("Bgr101010")]
	[InlineData("Rgba64")]
	[InlineData("Rgba128Float")]
	[InlineData("Cmyk32")]
	public void Every_format_converts_to_bgra32(string name)
	{
		var format = (PixelFormat)typeof(PixelFormats).GetField(name)!.GetValue(null)!;
		int stride = (format.BitsPerPixel + 7) / 8;
		var pal = format == PixelFormats.Indexed1 ? BitmapPalettes.BlackAndWhite : null;
		var b = BitmapSource.Create(1, 1, 96, 96, format, pal, new byte[stride], stride);
		Assert.Equal(4, b.ToBgra32().Length);
	}

	[Fact]
	public void Indexed_pixels_resolve_through_the_palette()
	{
		var pal = new BitmapPalette(new[] { Colors.Red, Colors.Lime });
		var b = BitmapSource.Create(2, 1, 96, 96, PixelFormats.Indexed1, pal, new byte[] { 0b0100_0000 }, 1);
		Assert.Equal(new byte[] { 0, 0, 255, 255, 0, 255, 0, 255 }, b.ToBgra32());
	}

	[Fact]
	public void Pbgra32_is_unpremultiplied_for_display()
	{
		var b = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Pbgra32, null, new byte[] { 0, 0, 128, 128 }, 4);
		Assert.Equal(new byte[] { 0, 0, 255, 128 }, b.ToBgra32());
	}

	[Fact]
	public void RenderTargetBitmap_reports_Pbgra32_through_the_new_path()
	{
		var rtb = new RenderTargetBitmap(2, 2, 96, 96, PixelFormats.Pbgra32);
		Assert.Equal(PixelFormats.Pbgra32, rtb.Format);
		Assert.Equal(16, rtb.ToBgra32().Length);
	}

	[Fact]
	public void Enums_have_the_wpf_values()
	{
		Assert.Equal(0, (int)BitmapCacheOption.OnDemand);
		Assert.Equal(BitmapCacheOption.Default, BitmapCacheOption.OnDemand);
		Assert.Equal(1, (int)BitmapCacheOption.OnLoad);
		Assert.Equal(2, (int)BitmapCacheOption.None);
		Assert.Equal(15, (int)(BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.DelayCreation
			| BitmapCreateOptions.IgnoreColorProfile | BitmapCreateOptions.IgnoreImageCache));
		Assert.Equal(3, (int)Rotation.Rotate270);
		Assert.Equal(2, (int)PngInterlaceOption.Off);
		Assert.Equal(6, (int)TiffCompressOption.Zip);
	}

	[Fact]
	public void Frames_wrap_sources_and_carry_extras()
	{
		var src = Bgr24(2, 2);
		var thumb = Bgr24(1, 1);
		var meta = new BitmapMetadata("png");
		var frame = BitmapFrame.Create(src, thumb, meta, null);
		Assert.Same(thumb, frame.Thumbnail);
		Assert.Same(meta, frame.Metadata);
		Assert.Null(frame.Decoder);
		Assert.Equal(2, frame.PixelWidth);
		Assert.Equal(PixelFormats.Bgr24, frame.Format);
		Assert.Throws<ArgumentNullException>(() => BitmapFrame.Create((BitmapSource)null!));
		Assert.Throws<InvalidOperationException>(() => frame.CreateInPlaceBitmapMetadataWriter());
	}

	[Fact]
	public void Metadata_queries_policy_and_freezing()
	{
		var m = new BitmapMetadata("png");
		Assert.Throws<ArgumentException>(() => new BitmapMetadata("zzz"));
		m.SetQuery("/Text/Title", "t");
		Assert.True(m.ContainsQuery("/Text/Title"));
		Assert.Equal("t", m.GetQuery("/Text/Title"));
		Assert.Null(m.GetQuery("/Text/None"));
		m.Title = "hello";
		m.Rating = 5;
		var clone = m.Clone();
		Assert.Equal("hello", clone.Title);
		Assert.Equal("t", clone.GetQuery("/Text/Title"));
		m.RemoveQuery("/Text/Title");
		Assert.False(m.ContainsQuery("/Text/Title"));
		m.Freeze();
		Assert.True(m.IsReadOnly);
		Assert.Throws<InvalidOperationException>(() => m.SetQuery("/Text/A", "b"));
		Assert.Equal("png", m.Format);
		Assert.Equal("/", m.Location);
	}

	[Fact]
	public void BitmapImage_validates_initialization()
	{
		var image = new BitmapImage();
		Assert.Throws<InvalidOperationException>(() => image.EndInit());
		image.BeginInit();
		Assert.Throws<InvalidOperationException>(() => image.BeginInit());
		Assert.Throws<InvalidOperationException>(() => image.EndInit());
		image.UriSource = new Uri("missing-lane-b.png", UriKind.Relative);
		image.EndInit();
		Assert.Throws<InvalidOperationException>(() => image.UriSource = new Uri("x.png", UriKind.Relative));
		Assert.Throws<InvalidOperationException>(() => image.DecodePixelWidth = 4);
	}

	[Fact]
	public void BitmapImage_with_a_missing_file_raises_DecodeFailed_instead_of_throwing()
	{
		var image = new BitmapImage(new Uri("missing-lane-b.png", UriKind.Relative));
		Exception? error = null;
		image.DecodeFailed += (_, e) => error = e.ErrorException;
		Assert.Equal(0, image.PixelWidth);
		Assert.IsAssignableFrom<System.IO.IOException>(error);
	}
}
