using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Media;

/// <summary>RelativeTransform, Transform order and the Flip modes, read back from a painted Rectangle.</summary>
public class TileBrushTransformPixelTests : WpfGtkTestBase
{
	const int Size = 100;

	static (byte R, byte G, byte B, byte A) PixelAt(Cairo.ImageSurface s, int x, int y)
	{
		s.Flush();
		byte[] d = s.Data;
		int i = y * s.Stride + x * 4;
		return (d[i + 2], d[i + 1], d[i], d[i + 3]);
	}

	static Cairo.ImageSurface Paint(Brush brush)
	{
		var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, Size, Size);
		using var cr = new Cairo.Context(surface);
		new Rectangle { Fill = brush }.PaintTo(cr, Size, Size);
		return surface;
	}

	static GeometryDrawing Fill(Color c, Rect r) =>
		new GeometryDrawing { Brush = new SolidColorBrush(c), Geometry = new RectangleGeometry(r) };

	// Red left half, blue right half.
	static DrawingBrush Horizontal() => new DrawingBrush(new DrawingGroup
	{
		Children = { Fill(Colors.Red, new Rect(0, 0, 10, 20)), Fill(Colors.Blue, new Rect(10, 0, 10, 20)) },
	});

	// Red top half, blue bottom half.
	static DrawingBrush Vertical() => new DrawingBrush(new DrawingGroup
	{
		Children = { Fill(Colors.Red, new Rect(0, 0, 20, 10)), Fill(Colors.Blue, new Rect(0, 10, 20, 10)) },
	});

	static void AssertRed((byte R, byte G, byte B, byte A) p) => Assert.True(p.R > 200 && p.B < 50 && p.A > 200, p.ToString());

	static void AssertBlue((byte R, byte G, byte B, byte A) p) => Assert.True(p.B > 200 && p.R < 50 && p.A > 200, p.ToString());

	static void AssertClear((byte R, byte G, byte B, byte A) p) => Assert.Equal(0, p.A);

	[Fact]
	public void A_RelativeTransform_flip_mirrors_the_content_vertically() => Run(() =>
	{
		var b = Vertical();
		b.RelativeTransform = new TransformGroup
		{
			Children = { new ScaleTransform(1, -1), new TranslateTransform(0, 1) },
		};
		using var s = Paint(b);
		AssertBlue(PixelAt(s, 50, 20));
		AssertRed(PixelAt(s, 50, 80));
	});

	[Fact]
	public void An_unflipped_brush_keeps_the_red_on_top() => Run(() =>
	{
		using var s = Paint(Vertical());
		AssertRed(PixelAt(s, 50, 20));
		AssertBlue(PixelAt(s, 50, 80));
	});

	[Fact]
	public void A_RelativeTransform_rotation_turns_left_into_top() => Run(() =>
	{
		var b = Horizontal();
		b.RelativeTransform = new RotateTransform(90, 0.5, 0.5);
		using var s = Paint(b);
		AssertRed(PixelAt(s, 50, 20));
		AssertBlue(PixelAt(s, 50, 80));
	});

	[Fact]
	public void Transform_is_applied_after_RelativeTransform() => Run(() =>
	{
		var b = Horizontal();
		b.RelativeTransform = new TranslateTransform(0.25, 0);
		b.Transform = new ScaleTransform(2, 1);
		using var s = Paint(b);
		// x' = (x + 25) * 2: red covers 50..150, so 30 is empty. The reverse order would paint it red.
		AssertClear(PixelAt(s, 30, 50));
		AssertRed(PixelAt(s, 60, 50));
	});

	[Fact]
	public void Mutating_Transform_repaints_through_Changed() => Run(() =>
	{
		var b = Horizontal();
		b.TileMode = TileMode.None;
		int n = 0;
		b.Changed += (_, _) => n++;
		b.Transform = new TranslateTransform(50, 0);
		b.RelativeTransform = new TranslateTransform(0.1, 0);
		Assert.True(n >= 2, n.ToString());
		using var s = Paint(b);
		AssertClear(PixelAt(s, 20, 50));
	});

	[Fact]
	public void FlipY_mirrors_the_second_row() => Run(() =>
	{
		var b = Vertical();
		b.Viewport = new Rect(0, 0, 50, 50);
		b.ViewportUnits = BrushMappingMode.Absolute;
		b.TileMode = TileMode.FlipY;
		using var s = Paint(b);
		AssertRed(PixelAt(s, 10, 10));
		AssertBlue(PixelAt(s, 10, 40));
		AssertBlue(PixelAt(s, 10, 60));
		AssertRed(PixelAt(s, 10, 90));
	});

	[Fact]
	public void FlipXY_mirrors_both_axes() => Run(() =>
	{
		var b = Horizontal();
		b.Viewport = new Rect(0, 0, 50, 50);
		b.ViewportUnits = BrushMappingMode.Absolute;
		b.TileMode = TileMode.FlipXY;
		using var s = Paint(b);
		AssertRed(PixelAt(s, 10, 10));
		AssertBlue(PixelAt(s, 40, 10));
		AssertBlue(PixelAt(s, 60, 10));
		AssertRed(PixelAt(s, 90, 10));
		AssertRed(PixelAt(s, 10, 60));
		AssertBlue(PixelAt(s, 60, 60));
		AssertRed(PixelAt(s, 90, 90));
	});

	[Fact]
	public void The_sample_grid_brush_draws_a_line_every_10_pixels() => Run(() =>
	{
		var b = new DrawingBrush(new DrawingGroup
		{
			Children =
			{
				Fill(Colors.White, new Rect(0, 0, 1, 1)),
				new GeometryDrawing
				{
					Brush = new SolidColorBrush(Color.FromRgb(0xE6, 0xE8, 0xF6)),
					Geometry = Geometry.Parse("M 0,0 L 0,1 0.1,1 0.1,0.1 1,0.1 1,0 Z"),
				},
			},
		})
		{
			Viewport = new Rect(0, 0, 10, 10), ViewportUnits = BrushMappingMode.Absolute, TileMode = TileMode.Tile,
		};
		using var s = Paint(b);
		foreach (var (x, y) in new[] { (0, 5), (10, 5), (20, 5), (5, 0), (5, 10), (5, 20) })
		{
			var p = PixelAt(s, x, y);
			Assert.True(p.R is > 220 and < 240 && p.B > 240, $"({x},{y}) {p}");
		}
		foreach (var (x, y) in new[] { (5, 5), (15, 15), (25, 5) })
		{
			var p = PixelAt(s, x, y);
			Assert.True(p.R > 250 && p.G > 250 && p.B > 250, $"({x},{y}) {p}");
		}
	});
}
