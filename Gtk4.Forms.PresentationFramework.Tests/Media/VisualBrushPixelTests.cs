using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Media;

/// <summary>VisualBrush and tile mapping pixels, read back from a Rectangle painted through its draw path.</summary>
public class VisualBrushPixelTests : WpfGtkTestBase
{
	const int Size = 100;

	static (byte R, byte G, byte B, byte A) PixelAt(Cairo.ImageSurface s, int x, int y)
	{
		s.Flush();
		byte[] d = s.Data;
		int i = y * s.Stride + x * 4;
		return (d[i + 2], d[i + 1], d[i], d[i + 3]);
	}

	static Cairo.ImageSurface Paint(Rectangle r)
	{
		var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, Size, Size);
		using var cr = new Cairo.Context(surface);
		r.PaintTo(cr, Size, Size);
		return surface;
	}

	static DrawingBrush TwoColour() => new DrawingBrush(new DrawingGroup
	{
		Children =
		{
			new GeometryDrawing
			{
				Brush = new SolidColorBrush(Colors.Red), Geometry = new RectangleGeometry(new Rect(0, 0, 10, 20)),
			},
			new GeometryDrawing
			{
				Brush = new SolidColorBrush(Colors.Blue), Geometry = new RectangleGeometry(new Rect(10, 0, 10, 20)),
			},
		},
	});

	static void AssertRed((byte R, byte G, byte B, byte A) p) => Assert.True(p.R > 200 && p.B < 50 && p.A > 200, p.ToString());

	static void AssertBlue((byte R, byte G, byte B, byte A) p) => Assert.True(p.B > 200 && p.R < 50 && p.A > 200, p.ToString());

	static void AssertClear((byte R, byte G, byte B, byte A) p) => Assert.Equal(0, p.A);

	[Fact]
	public void A_DrawingBrush_fill_shows_both_halves() => Run(() =>
	{
		using var s = Paint(new Rectangle { Fill = TwoColour() });
		AssertRed(PixelAt(s, 20, 50));
		AssertBlue(PixelAt(s, 80, 50));
	});

	[Fact]
	public void Absolute_viewport_with_Tile_repeats_at_the_viewport_period() => Run(() =>
	{
		var b = TwoColour();
		b.Viewport = new Rect(0, 0, 20, 20);
		b.ViewportUnits = BrushMappingMode.Absolute;
		b.TileMode = TileMode.Tile;
		using var s = Paint(new Rectangle { Fill = b });
		AssertRed(PixelAt(s, 5, 5));
		AssertBlue(PixelAt(s, 15, 5));
		AssertRed(PixelAt(s, 25, 5));
		AssertBlue(PixelAt(s, 95, 85));
	});

	[Fact]
	public void FlipX_mirrors_the_second_column() => Run(() =>
	{
		var b = TwoColour();
		b.Viewport = new Rect(0, 0, 20, 20);
		b.ViewportUnits = BrushMappingMode.Absolute;
		b.TileMode = TileMode.FlipX;
		using var s = Paint(new Rectangle { Fill = b });
		AssertRed(PixelAt(s, 5, 5));
		AssertBlue(PixelAt(s, 15, 5));
		AssertBlue(PixelAt(s, 25, 5));
		AssertRed(PixelAt(s, 35, 5));
	});

	[Fact]
	public void TileMode_None_leaves_the_outside_transparent() => Run(() =>
	{
		var b = TwoColour();
		b.Viewport = new Rect(0, 0, 20, 20);
		b.ViewportUnits = BrushMappingMode.Absolute;
		using var s = Paint(new Rectangle { Fill = b });
		AssertRed(PixelAt(s, 5, 5));
		AssertClear(PixelAt(s, 50, 50));
	});

	[Fact]
	public void Viewbox_selects_the_blue_half() => Run(() =>
	{
		var b = TwoColour();
		b.Viewbox = new Rect(0.5, 0, 0.5, 1);
		using var s = Paint(new Rectangle { Fill = b });
		AssertBlue(PixelAt(s, 5, 50));
		AssertBlue(PixelAt(s, 95, 50));
	});

	[Fact]
	public void Opacity_is_baked_into_the_tile() => Run(() =>
	{
		var b = TwoColour();
		b.Opacity = 0.5;
		using var s = Paint(new Rectangle { Fill = b });
		var p = PixelAt(s, 20, 50);
		Assert.InRange(p.A, 110, 145);
	});

	[Fact]
	public void A_degenerate_viewbox_paints_transparent_not_black() => Run(() =>
	{
		var b = TwoColour();
		b.Viewbox = new Rect(0, 0, 0, 0);
		using var s = Paint(new Rectangle { Fill = b });
		AssertClear(PixelAt(s, 50, 50));
	});

	[Fact]
	public void A_VisualBrush_of_a_non_drawing_visual_without_size_is_not_black() => Run(() =>
	{
		var border = new Border { Background = Brushes.Red };
		using var s = Paint(new Rectangle { Fill = new VisualBrush(border) });
		AssertClear(PixelAt(s, 50, 50));
	});

	[Fact]
	public void Setting_Viewbox_in_code_raises_Changed_on_the_fill_owner() => Run(() =>
	{
		var b = new VisualBrush();
		int n = 0;
		b.Changed += (_, _) => n++;
		b.Viewbox = new Rect(0, 0, 0.5, 0.5);
		Assert.Equal(1, n);
	});
}
