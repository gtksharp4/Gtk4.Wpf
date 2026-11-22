using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Media.Imaging;

public class ImagingTransformTests
{
	// 3x2 Bgra32; pixel (x, y) has B = x, G = y, R = 10 * (y * 3 + x), A = 255.
	static BitmapSource Source()
	{
		var px = new byte[3 * 2 * 4];
		for (int y = 0; y < 2; y++)
		{
			for (int x = 0; x < 3; x++)
			{
				int o = (y * 3 + x) * 4;
				px[o] = (byte)x;
				px[o + 1] = (byte)y;
				px[o + 2] = (byte)(10 * (y * 3 + x));
				px[o + 3] = 255;
			}
		}
		return BitmapSource.Create(3, 2, 96, 96, PixelFormats.Bgra32, null, px, 12);
	}

	static byte[] Pixels(BitmapSource s)
	{
		int stride = (s.PixelWidth * s.Format.BitsPerPixel + 7) / 8;
		var buffer = new byte[stride * s.PixelHeight];
		s.CopyPixels(buffer, stride, 0);
		return buffer;
	}

	static byte[] Pixel(BitmapSource s, int x, int y) =>
		Pixels(s).Skip((y * s.PixelWidth + x) * 4).Take(4).ToArray();

	[Fact]
	public void Crop_returns_the_exact_region()
	{
		var crop = new CroppedBitmap(Source(), new Int32Rect(1, 0, 2, 2));
		Assert.Equal(2, crop.PixelWidth);
		Assert.Equal(2, crop.PixelHeight);
		Assert.Equal(new byte[] { 1, 0, 10, 255 }, Pixel(crop, 0, 0));
		Assert.Equal(new byte[] { 2, 1, 50, 255 }, Pixel(crop, 1, 1));
	}

	[Fact]
	public void Crop_with_empty_rect_is_the_whole_source()
	{
		var crop = new CroppedBitmap(Source(), Int32Rect.Empty);
		Assert.Equal(3, crop.PixelWidth);
		Assert.Equal(2, crop.PixelHeight);
		Assert.Equal(Pixels(Source()), Pixels(crop));
	}

	[Fact]
	public void Crop_outside_the_source_throws_ArgumentException()
	{
		Assert.Throws<ArgumentException>(() => new CroppedBitmap(Source(), new Int32Rect(2, 0, 2, 1)));
		var late = new CroppedBitmap { Source = Source(), SourceRect = new Int32Rect(0, 0, 9, 9) };
		Assert.Throws<ArgumentException>(() => late.PixelWidth);
	}

	[Fact]
	public void Crop_supports_BeginInit_and_rejects_double_BeginInit()
	{
		var crop = new CroppedBitmap();
		crop.BeginInit();
		Assert.Throws<InvalidOperationException>(() => crop.BeginInit());
		crop.Source = Source();
		crop.SourceRect = new Int32Rect(0, 1, 3, 1);
		crop.EndInit();
		Assert.Equal(1, crop.PixelHeight);
		Assert.Throws<InvalidOperationException>(() => crop.EndInit());
	}

	[Fact]
	public void Crop_property_change_is_reflected()
	{
		var crop = new CroppedBitmap(Source(), new Int32Rect(0, 0, 1, 1));
		Assert.Equal(1, crop.PixelWidth);
		crop.SourceRect = new Int32Rect(0, 0, 3, 1);
		Assert.Equal(3, crop.PixelWidth);
	}

	[Theory]
	[InlineData(90, 2, 3)]
	[InlineData(180, 3, 2)]
	[InlineData(270, 2, 3)]
	public void Rotation_swaps_size_for_quarter_turns(int angle, int width, int height)
	{
		var t = new TransformedBitmap(Source(), new RotateTransform(angle));
		Assert.Equal(width, t.PixelWidth);
		Assert.Equal(height, t.PixelHeight);
	}

	[Fact]
	public void Rotate90_moves_pixels_clockwise()
	{
		var t = new TransformedBitmap(Source(), new RotateTransform(90));
		// Source row 0 (R 0,10,20) becomes the last column, top to bottom.
		Assert.Equal(0, Pixel(t, 1, 0)[2]);
		Assert.Equal(10, Pixel(t, 1, 1)[2]);
		Assert.Equal(20, Pixel(t, 1, 2)[2]);
		Assert.Equal(30, Pixel(t, 0, 0)[2]);
	}

	[Fact]
	public void Rotate180_reverses_the_pixels()
	{
		var t = new TransformedBitmap(Source(), new RotateTransform(180));
		Assert.Equal(50, Pixel(t, 0, 0)[2]);
		Assert.Equal(0, Pixel(t, 2, 1)[2]);
	}

	[Fact]
	public void Rotate270_moves_pixels_counter_clockwise()
	{
		var t = new TransformedBitmap(Source(), new RotateTransform(270));
		Assert.Equal(20, Pixel(t, 0, 0)[2]);
		Assert.Equal(10, Pixel(t, 0, 1)[2]);
		Assert.Equal(0, Pixel(t, 0, 2)[2]);
		Assert.Equal(50, Pixel(t, 1, 0)[2]);
	}

	[Fact]
	public void Scale_resizes_nearest_neighbour_and_negative_scale_flips()
	{
		var up = new TransformedBitmap(Source(), new ScaleTransform(2, 1));
		Assert.Equal(6, up.PixelWidth);
		Assert.Equal(0, Pixel(up, 1, 0)[2]);
		Assert.Equal(10, Pixel(up, 2, 0)[2]);
		var flip = new TransformedBitmap(Source(), new ScaleTransform(-1, 1));
		Assert.Equal(3, flip.PixelWidth);
		Assert.Equal(20, Pixel(flip, 0, 0)[2]);
	}

	[Fact]
	public void Non_quarter_rotation_or_skew_throws_InvalidOperationException()
	{
		Assert.Throws<InvalidOperationException>(() => new TransformedBitmap(Source(), new RotateTransform(45)));
		Assert.Throws<InvalidOperationException>(() => new TransformedBitmap(Source(), new SkewTransform(10, 0)));
	}

	[Fact]
	public void Transform_default_is_identity()
	{
		var t = new TransformedBitmap { Source = Source() };
		Assert.Equal(Matrix.Identity, t.Transform.Value);
		Assert.Equal(3, t.PixelWidth);
	}

	[Fact]
	public void Gray8_conversion_uses_luma()
	{
		var gray = new FormatConvertedBitmap(Source(), PixelFormats.Gray8, null, 0);
		Assert.Equal(PixelFormats.Gray8, gray.Format);
		var p = Pixels(gray);
		Assert.Equal(6, p.Length);
		Assert.Equal((byte)((10 * 2126 + 0 * 7152 + 1 * 722 + 5000) / 10000), p[1]);
	}

	[Theory]
	[InlineData("Bgra32")]
	[InlineData("Pbgra32")]
	[InlineData("Bgr32")]
	public void Four_byte_conversions_keep_the_colour(string name)
	{
		var format = (PixelFormat)typeof(PixelFormats).GetField(name)!.GetValue(null)!;
		var c = new FormatConvertedBitmap(Source(), format, null, 0);
		var p = Pixel(c, 2, 1);
		Assert.Equal(new byte[] { 2, 1, 50 }, p.Take(3).ToArray());
	}

	[Fact]
	public void Indexed8_conversion_maps_to_the_nearest_palette_entry()
	{
		var palette = new BitmapPalette(new[] { Colors.Black, Color.FromRgb(50, 1, 2), Colors.White });
		var c = new FormatConvertedBitmap(Source(), PixelFormats.Indexed8, palette, 0);
		Assert.Equal(PixelFormats.Indexed8, c.Format);
		var p = Pixels(c);
		Assert.Equal(1, p[5]);
		Assert.Equal(0, p[0]);
		Assert.NotNull(c.Palette);
	}

	[Fact]
	public void FormatConverted_defaults_and_null_checks()
	{
		var c = new FormatConvertedBitmap();
		Assert.Equal(PixelFormats.Pbgra32, c.DestinationFormat);
		Assert.Equal(0.0, c.AlphaThreshold);
		Assert.Null(c.DestinationPalette);
		Assert.Throws<ArgumentNullException>(() => new FormatConvertedBitmap(null!, PixelFormats.Gray8, null, 0));
		Assert.Throws<NotSupportedException>(() =>
			new FormatConvertedBitmap(Source(), PixelFormats.Rgba128Float, null, 0));
	}

	[Fact]
	public void CachedBitmap_snapshots_its_source()
	{
		var rtb = new RenderTargetBitmap(2, 2, 96, 96, PixelFormats.Pbgra32);
		var dv = new DrawingVisual();
		using (var scope = dv.RenderOpen())
			scope.Context.DrawRectangle(new SolidColorBrush(Colors.Red), null, new Rect(0, 0, 2, 2));
		rtb.Render(dv);
		var cached = new CachedBitmap(rtb, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
		rtb.Render(new DrawingVisual());
		Assert.Equal(255, Pixels(cached)[3]);
		Assert.Throws<ArgumentNullException>(() => new CachedBitmap(null!, BitmapCreateOptions.None,
			BitmapCacheOption.Default));
	}

	[Fact]
	public void Derived_bitmaps_freeze_and_clone()
	{
		var crop = new CroppedBitmap(Source(), new Int32Rect(0, 0, 2, 2));
		crop.Freeze();
		Assert.True(crop.IsFrozen);
		Assert.Throws<InvalidOperationException>(() => crop.SourceRect = Int32Rect.Empty);
		var clone = crop.Clone();
		Assert.IsType<CroppedBitmap>(clone);
		Assert.Equal(2, clone.PixelWidth);
		Assert.Equal(Pixels(crop), Pixels(clone));
	}

	[Fact]
	public void BitmapFrame_Create_wraps_every_derived_bitmap()
	{
		BitmapSource[] all =
		{
			new CroppedBitmap(Source(), new Int32Rect(0, 0, 2, 1)),
			new FormatConvertedBitmap(Source(), PixelFormats.Gray8, null, 0),
			new TransformedBitmap(Source(), new RotateTransform(90)),
			new CachedBitmap(Source(), BitmapCreateOptions.None, BitmapCacheOption.Default),
		};
		foreach (var bitmap in all)
		{
			var frame = BitmapFrame.Create(bitmap);
			Assert.Equal(bitmap.PixelWidth, frame.PixelWidth);
			Assert.Equal(Pixels(bitmap), Pixels(frame));
		}
	}
}
