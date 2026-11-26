using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Media.Imaging.ImagingTestData;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

public class ImagingConvertTests
{
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
