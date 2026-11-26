using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Media.Imaging.ImagingTestData;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

public class ImagingTransformTests
{
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
}
