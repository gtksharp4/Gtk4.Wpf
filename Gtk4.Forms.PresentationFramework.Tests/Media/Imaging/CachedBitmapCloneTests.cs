using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;
using static Gtk4.Forms.PresentationFramework.Tests.Media.Imaging.ImagingTestData;

namespace Gtk4.Forms.PresentationFramework.Tests.Media.Imaging;

public class CachedBitmapCloneTests
{
	static void Paint(RenderTargetBitmap rtb, Color color)
	{
		var dv = new DrawingVisual();
		using (var scope = dv.RenderOpen())
			scope.Context.DrawRectangle(new SolidColorBrush(color), null, new Rect(0, 0, 2, 2));
		rtb.Render(dv);
	}

	static CachedBitmap CacheRedThenRerender(out RenderTargetBitmap rtb)
	{
		rtb = new RenderTargetBitmap(2, 2, 96, 96, PixelFormats.Pbgra32);
		Paint(rtb, Colors.Red);
		var cached = new CachedBitmap(rtb, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
		Paint(rtb, Colors.Blue);
		return cached;
	}

	[Fact]
	public void Clone_keeps_the_cached_pixels_after_the_source_was_rendered_again()
	{
		var cached = CacheRedThenRerender(out _);
		var clone = cached.Clone();
		Assert.Equal(Pixels(cached), Pixels(clone));
		Assert.Equal(255, Pixel(clone, 0, 0)[2]);
		Assert.Equal(0, Pixel(clone, 0, 0)[0]);
	}

	[Fact]
	public void GetAsFrozen_and_CloneCurrentValue_keep_the_cached_pixels()
	{
		var cached = CacheRedThenRerender(out _);
		var frozen = (CachedBitmap)((Freezable)cached).GetAsFrozen();
		Assert.True(frozen.IsFrozen);
		Assert.Equal(Pixels(cached), Pixels(frozen));
		Assert.Equal(Pixels(cached), Pixels(cached.CloneCurrentValue()));
	}
}
