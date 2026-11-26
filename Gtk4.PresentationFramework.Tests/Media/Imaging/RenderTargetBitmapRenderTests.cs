using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

/// <summary>
/// Tests that verify pixel content produced by <see cref="RenderTargetBitmap.Render"/>.
/// Cairo.ImageSurface is in-memory — these tests are headless (no GTK display needed).
/// </summary>
/// <remarks>
/// Cairo ARGB32 stores pixels as premultiplied BGRA in native byte order:
/// byte[0]=B, byte[1]=G, byte[2]=R, byte[3]=A.
/// A fully opaque red fill should produce B=0, G=0, R≈255, A=255.
/// Minor rounding differences (±2) are tolerated.
/// </remarks>
public class RenderTargetBitmapRenderTests
{
	const int Size = 10;
	const int Tolerance = 2;

	static RenderTargetBitmap MakeRedBitmap()
	{
		var dv = new DrawingVisual();
		using (var scope = dv.RenderOpen())
		{
			scope.Context.DrawRectangle(
				new SolidColorBrush(Colors.Red),
				null,
				new Rect(0, 0, Size, Size));
		}
		var rtb = new RenderTargetBitmap(Size, Size, 96, 96, PixelFormats.Pbgra32);
		rtb.Render(dv);
		return rtb;
	}

	// ── Core pixel-readback test ──────────────────────────────────────────────

	[Fact]
	public void Render_red_fill_center_pixel_is_red()
	{
		var rtb = MakeRedBitmap();
		var (b, g, r, a) = rtb.GetPixel(Size / 2, Size / 2);
		// Premultiplied: R and A are both 255 for opaque red; B and G are 0.
		Assert.True(r >= 255 - Tolerance, $"R={r}");
		Assert.True(a >= 255 - Tolerance, $"A={a}");
		Assert.True(b <= Tolerance, $"B={b}");
		Assert.True(g <= Tolerance, $"G={g}");
	}

	[Fact]
	public void Render_red_fill_corner_pixel_is_red()
	{
		var rtb = MakeRedBitmap();
		var (b, g, r, a) = rtb.GetPixel(0, 0);
		Assert.True(r >= 255 - Tolerance, $"R={r}");
		Assert.True(a >= 255 - Tolerance, $"A={a}");
		Assert.True(b <= Tolerance, $"B={b}");
		Assert.True(g <= Tolerance, $"G={g}");
	}

	// ── CopyPixels round-trip ─────────────────────────────────────────────────

	[Fact]
	public void CopyPixels_roundtrip_matches_GetPixel()
	{
		var rtb = MakeRedBitmap();
		int stride = Size * 4;
		var buf = new byte[stride * Size];
		rtb.CopyPixels(buf, stride, 0);

		var (b, g, r, a) = rtb.GetPixel(Size / 2, Size / 2);
		int off = (Size / 2) * stride + (Size / 2) * 4;

		Assert.Equal(b, buf[off]);
		Assert.Equal(g, buf[off + 1]);
		Assert.Equal(r, buf[off + 2]);
		Assert.Equal(a, buf[off + 3]);
	}

	// ── Second Render replaces previous pixels ────────────────────────────────

	[Fact]
	public void Second_Render_replaces_pixels()
	{
		var rtb = MakeRedBitmap();

		// Overwrite with a transparent (empty) render.
		var dv2 = new DrawingVisual();
		using (var scope = dv2.RenderOpen())
		{
			scope.Context.DrawRectangle(
				new SolidColorBrush(Colors.Transparent),
				null,
				new Rect(0, 0, Size, Size));
		}
		rtb.Render(dv2);

		var (b, g, r, a) = rtb.GetPixel(Size / 2, Size / 2);
		// Transparent: all channels zero.
		Assert.Equal(0, r + g + b + a);
	}

	// ── DrawingVisual with blue fill ──────────────────────────────────────────

	[Fact]
	public void Render_blue_fill_center_pixel_is_blue()
	{
		var dv = new DrawingVisual();
		using (var scope = dv.RenderOpen())
		{
			scope.Context.DrawRectangle(
				new SolidColorBrush(Colors.Blue),
				null,
				new Rect(0, 0, Size, Size));
		}
		var rtb = new RenderTargetBitmap(Size, Size, 96, 96, PixelFormats.Pbgra32);
		rtb.Render(dv);

		var (b, g, r, a) = rtb.GetPixel(Size / 2, Size / 2);
		Assert.True(b >= 255 - Tolerance, $"B={b}");
		Assert.True(a >= 255 - Tolerance, $"A={a}");
		Assert.True(r <= Tolerance, $"R={r}");
		Assert.True(g <= Tolerance, $"G={g}");
	}
}
