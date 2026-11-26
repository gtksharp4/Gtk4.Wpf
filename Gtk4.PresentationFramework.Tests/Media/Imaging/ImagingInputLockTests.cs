using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Media.Imaging.ImagingTestData;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

/// <summary>WPF coerces the inputs of derived bitmaps back to their initialised values after EndInit.</summary>
public class ImagingInputLockTests
{
	static BitmapSource Other() =>
		BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 9, 9, 9, 255 }, 4);

	[Fact]
	public void Crop_inputs_changed_after_EndInit_are_ignored()
	{
		var source = Source();
		var crop = new CroppedBitmap(source, new Int32Rect(1, 0, 2, 2));
		var before = Pixels(crop);
		crop.SourceRect = new Int32Rect(0, 0, 3, 1);
		crop.Source = Other();
		Assert.Equal(new Int32Rect(1, 0, 2, 2), crop.SourceRect);
		Assert.Same(source, crop.Source);
		Assert.Equal(2, crop.PixelWidth);
		Assert.Equal(before, Pixels(crop));
	}

	[Fact]
	public void Crop_inputs_are_writable_again_between_BeginInit_and_EndInit()
	{
		var crop = new CroppedBitmap(Source(), new Int32Rect(0, 0, 1, 1));
		crop.BeginInit();
		crop.SourceRect = new Int32Rect(0, 0, 3, 1);
		crop.EndInit();
		Assert.Equal(3, crop.PixelWidth);
	}

	[Fact]
	public void FormatConverted_inputs_changed_after_EndInit_are_ignored()
	{
		var source = Source();
		var palette = new BitmapPalette(new[] { Colors.Black, Colors.White });
		var gray = new FormatConvertedBitmap(source, PixelFormats.Gray8, palette, 5);
		var before = Pixels(gray);
		gray.Source = Other();
		gray.DestinationFormat = PixelFormats.Bgra32;
		gray.DestinationPalette = null;
		gray.AlphaThreshold = 50;
		Assert.Same(source, gray.Source);
		Assert.Equal(PixelFormats.Gray8, gray.DestinationFormat);
		Assert.Same(palette, gray.DestinationPalette);
		Assert.Equal(5, gray.AlphaThreshold);
		Assert.Equal(PixelFormats.Gray8, gray.Format);
		Assert.Equal(before, Pixels(gray));
	}

	[Fact]
	public void Transformed_inputs_changed_after_EndInit_are_ignored()
	{
		var source = Source();
		var transform = new RotateTransform(90);
		var rotated = new TransformedBitmap(source, transform);
		var before = Pixels(rotated);
		rotated.Source = Other();
		rotated.Transform = new RotateTransform(180);
		Assert.Same(source, rotated.Source);
		Assert.Same(transform, rotated.Transform);
		Assert.Equal(2, rotated.PixelWidth);
		Assert.Equal(before, Pixels(rotated));
	}

	[Fact]
	public void A_clone_is_initialised_too()
	{
		var crop = new CroppedBitmap(Source(), new Int32Rect(1, 0, 2, 2));
		var clone = (CroppedBitmap)crop.Clone();
		clone.SourceRect = Int32Rect.Empty;
		Assert.Equal(new Int32Rect(1, 0, 2, 2), clone.SourceRect);
		Assert.Equal(2, clone.PixelWidth);
	}

	[Fact]
	public void Freeze_freezes_the_Freezable_inputs()
	{
		var source = Source();
		var rotated = new TransformedBitmap(source, new RotateTransform(90));
		var transform = rotated.Transform;
		Assert.True(rotated.CanFreeze);
		rotated.Freeze();
		Assert.True(source.IsFrozen);
		Assert.True(transform.IsFrozen);
		var crop = new CroppedBitmap(Source(), Int32Rect.Empty);
		crop.Freeze();
		Assert.True(crop.Source!.IsFrozen);
		var gray = new FormatConvertedBitmap(Source(), PixelFormats.Gray8, null, 0);
		gray.Freeze();
		Assert.True(gray.Source!.IsFrozen);
	}

	[Fact]
	public void CanFreeze_is_false_while_an_input_cannot_freeze()
	{
		var inner = new CroppedBitmap();
		inner.BeginInit();
		var holder = new TransformedBitmap { Source = inner };
		Assert.False(holder.CanFreeze);
		Assert.Throws<System.InvalidOperationException>(() => holder.Freeze());
	}
}
