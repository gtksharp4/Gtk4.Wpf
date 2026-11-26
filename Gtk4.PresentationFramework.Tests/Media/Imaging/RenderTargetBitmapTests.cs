using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

/// <summary>
/// Pure (no GTK) tests for <see cref="RenderTargetBitmap"/>.
/// Cairo.ImageSurface is an in-memory surface; these tests run headless.
/// </summary>
public class RenderTargetBitmapTests
{
	// ── Constructor / dimensions ──────────────────────────────────────────────

	[Fact]
	public void Ctor_sets_PixelWidth_and_PixelHeight()
	{
		var rtb = new RenderTargetBitmap(10, 20, 96, 96, PixelFormats.Pbgra32);
		Assert.Equal(10, rtb.PixelWidth);
		Assert.Equal(20, rtb.PixelHeight);
	}

	[Fact]
	public void Ctor_stores_dpi()
	{
		var rtb = new RenderTargetBitmap(8, 8, 120, 72, PixelFormats.Pbgra32);
		Assert.Equal(120, rtb.DpiX);
		Assert.Equal(72, rtb.DpiY);
	}

	[Fact]
	public void Ctor_zero_width_throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			new RenderTargetBitmap(0, 10, 96, 96, PixelFormats.Pbgra32));
	}

	[Fact]
	public void Ctor_zero_height_throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			new RenderTargetBitmap(10, 0, 96, 96, PixelFormats.Pbgra32));
	}

	[Fact]
	public void Is_subtype_of_BitmapSource()
	{
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		Assert.IsAssignableFrom<BitmapSource>(rtb);
		Assert.IsAssignableFrom<ImageSource>(rtb);
	}

	// ── Render: null / non-DrawingVisual ─────────────────────────────────────

	[Fact]
	public void Render_null_does_not_throw()
	{
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		var ex = Record.Exception(() => rtb.Render(null));
		Assert.Null(ex);
	}

	[Fact]
	public void Render_null_produces_transparent_pixels()
	{
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		rtb.Render(null);
		var (b, g, r, a) = rtb.GetPixel(2, 2);
		Assert.Equal(0, b);
		Assert.Equal(0, g);
		Assert.Equal(0, r);
		Assert.Equal(0, a);
	}

	[Fact]
	public void Render_general_Visual_does_not_throw()
	{
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		var button = new System.Windows.Controls.Button();
		var ex = Record.Exception(() => rtb.Render(button));
		Assert.Null(ex);
	}

	[Fact]
	public void Render_general_Visual_produces_transparent_pixels()
	{
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		rtb.Render(new System.Windows.Controls.Border());
		var (b, g, r, a) = rtb.GetPixel(2, 2);
		Assert.Equal(0, b + g + r + a);
	}

	[Fact]
	public void Render_DrawingVisual_no_committed_drawing_produces_transparent()
	{
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		var dv = new DrawingVisual();   // no RenderOpen/commit
		rtb.Render(dv);
		var (b, g, r, a) = rtb.GetPixel(2, 2);
		Assert.Equal(0, b + g + r + a);
	}

	// ── CopyPixels: contract / argument validation ────────────────────────────

	[Fact]
	public void CopyPixels_null_buffer_throws_ArgumentNullException()
	{
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		rtb.Render(null);
		Assert.Throws<ArgumentNullException>(() => rtb.CopyPixels(null!, 16, 0));
	}

	[Fact]
	public void CopyPixels_stride_too_narrow_throws_ArgumentException()
	{
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		rtb.Render(null);
		// stride 15 < 4 * 4 = 16
		Assert.Throws<ArgumentException>(() => rtb.CopyPixels(new byte[64], 15, 0));
	}

	[Fact]
	public void CopyPixels_buffer_too_small_throws_ArgumentException()
	{
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		rtb.Render(null);
		// stride=16, height=4 → need 64 bytes; supply 63
		Assert.Throws<ArgumentException>(() => rtb.CopyPixels(new byte[63], 16, 0));
	}

	[Fact]
	public void CopyPixels_valid_call_fills_buffer_without_throwing()
	{
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		rtb.Render(null);
		var buf = new byte[64];
		var ex = Record.Exception(() => rtb.CopyPixels(buf, 16, 0));
		Assert.Null(ex);
	}

	[Fact]
	public void CopyPixels_offset_respected()
	{
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		rtb.Render(null);
		int offset = 8;
		var buf = new byte[64 + offset];
		var ex = Record.Exception(() => rtb.CopyPixels(buf, 16, offset));
		Assert.Null(ex);
	}

	// ── GetPixel before Render ────────────────────────────────────────────────

	[Fact]
	public void GetPixel_before_render_returns_zero()
	{
		var rtb = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
		var (b, g, r, a) = rtb.GetPixel(0, 0);
		Assert.Equal(0, b + g + r + a);
	}

	// ── PixelFormat stub ──────────────────────────────────────────────────────

	[Fact]
	public void Pbgra32_singleton_equals_itself()
	{
		Assert.Equal(PixelFormats.Pbgra32, PixelFormats.Pbgra32);
	}

	[Fact]
	public void PixelFormat_ToString_returns_name()
	{
		Assert.Equal("Pbgra32", PixelFormats.Pbgra32.ToString());
	}
}
