using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Media.Imaging;

public class ImageSourceFreezableTests
{
	static BitmapSource Small() =>
		BitmapSource.Create(2, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 1, 2, 3, 255, 4, 5, 6, 255 }, 8);

	static RenderTargetBitmap Painted()
	{
		var dv = new DrawingVisual();
		using (var scope = dv.RenderOpen())
			scope.Context.DrawRectangle(new SolidColorBrush(Colors.Red), null, new Rect(0, 0, 4, 4));
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		rtb.Render(dv);
		return rtb;
	}

	[Fact]
	public void ImageSource_is_an_Animatable_Freezable()
	{
		Assert.True(typeof(Animatable).IsAssignableFrom(typeof(ImageSource)));
		Assert.True(typeof(Freezable).IsAssignableFrom(typeof(BitmapSource)));
	}

	[Fact]
	public void RenderTargetBitmap_freezes_and_reports_state()
	{
		var rtb = Painted();
		Assert.False(rtb.IsFrozen);
		Assert.True(rtb.CanFreeze);
		rtb.Freeze();
		Assert.True(rtb.IsFrozen);
	}

	[Fact]
	public void Render_after_freeze_throws_InvalidOperationException()
	{
		var rtb = Painted();
		rtb.Freeze();
		Assert.Throws<InvalidOperationException>(() => rtb.Render(new DrawingVisual()));
	}

	[Fact]
	public void Frozen_bitmap_pixels_stay_readable()
	{
		var rtb = Painted();
		rtb.Freeze();
		var pixels = new byte[4 * 4 * 4];
		rtb.CopyPixels(pixels, 16, 0);
		Assert.Equal(255, pixels[2]);
		Assert.Equal(255, pixels[3]);
	}

	[Fact]
	public void Frozen_bitmap_can_be_an_ImageBrush_source()
	{
		var rtb = Painted();
		rtb.Freeze();
		var brush = new ImageBrush(rtb);
		Assert.Same(rtb, brush.ImageSource);
	}

	[Fact]
	public void Clone_is_independent_and_unfrozen()
	{
		var rtb = Painted();
		rtb.Freeze();
		var clone = rtb.Clone();
		Assert.IsType<RenderTargetBitmap>(clone);
		Assert.False(clone.IsFrozen);
		((RenderTargetBitmap)clone).Render(new DrawingVisual());
		var original = new byte[64];
		rtb.CopyPixels(original, 16, 0);
		Assert.Equal(255, original[3]);
	}

	[Fact]
	public void Memory_bitmap_clone_copies_pixels()
	{
		var source = Small();
		var clone = source.Clone();
		Assert.NotSame(source, clone);
		var a = new byte[8];
		var b = new byte[8];
		source.CopyPixels(a, 8, 0);
		clone.CopyPixels(b, 8, 0);
		Assert.Equal(a, b);
		Assert.Equal(2, clone.PixelWidth);
	}

	[Fact]
	public void CloneCurrentValue_and_GetAsFrozen_work_on_bitmaps()
	{
		var source = Small();
		Assert.False(source.CloneCurrentValue().IsFrozen);
		Assert.True(((Freezable)source).GetAsFrozen().IsFrozen);
	}

	[Fact]
	public void FromFile_image_source_clones_with_its_path()
	{
		var image = ImageSource.FromFile("a.png");
		var clone = (ImageSource)image.Clone();
		Assert.Equal("a.png", clone.FilePath);
		Assert.NotSame(image, clone);
		image.Freeze();
		Assert.True(image.IsFrozen);
	}

	[Fact]
	public void BitmapFrame_clone_keeps_the_pixels()
	{
		var frame = BitmapFrame.Create(Small());
		var clone = frame.Clone();
		Assert.IsAssignableFrom<BitmapFrame>(clone);
		Assert.Equal(2, clone.PixelWidth);
	}

	[Fact]
	public void BitmapImage_cannot_freeze_while_initializing()
	{
		var image = new BitmapImage();
		image.BeginInit();
		Assert.False(image.CanFreeze);
	}

	[Fact]
	public void DrawingImage_clone_and_frozen_setter()
	{
		var image = new DrawingImage(new GeometryDrawing());
		Assert.IsType<DrawingImage>(image.Clone());
		image.Freeze();
		Assert.Throws<InvalidOperationException>(() => image.Drawing = null);
	}
}
