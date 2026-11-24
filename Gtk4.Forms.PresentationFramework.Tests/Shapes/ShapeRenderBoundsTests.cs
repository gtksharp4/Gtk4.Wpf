using System;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Shapes;

/// <summary>Geometry.GetRenderBounds: pen-inflated bounds, expected values worked out by hand.</summary>
public class ShapeRenderBoundsTests : WpfGtkTestBase
{
	const double Eps = 0.02;

	static void AssertRect(Rect r, double x, double y, double w, double h)
	{
		Assert.InRange(r.X, x - Eps, x + Eps);
		Assert.InRange(r.Y, y - Eps, y + Eps);
		Assert.InRange(r.Width, w - Eps, w + Eps);
		Assert.InRange(r.Height, h - Eps, h + Eps);
	}

	static Pen PenOf(double thickness, PenLineCap cap = PenLineCap.Flat, PenLineJoin join = PenLineJoin.Miter,
		double miter = 10) =>
		new Pen(Brushes.Black, thickness) { StartLineCap = cap, EndLineCap = cap, LineJoin = join, MiterLimit = miter };

	[Fact]
	public void Null_pen_gives_the_fill_bounds() => Run(() =>
		AssertRect(new EllipseGeometry(new Point(50, 50), 25, 25).GetRenderBounds(null), 25, 25, 50, 50));

	[Fact]
	public void Flat_capped_horizontal_line_grows_only_in_y() => Run(() =>
		AssertRect(new LineGeometry(new Point(10, 10), new Point(30, 10)).GetRenderBounds(PenOf(4)), 10, 8, 20, 4));

	[Fact]
	public void Square_caps_extend_the_line_by_half_the_thickness() => Run(() =>
		AssertRect(new LineGeometry(new Point(10, 10), new Point(30, 10))
			.GetRenderBounds(PenOf(4, PenLineCap.Square)), 8, 8, 24, 4));

	[Fact]
	public void Round_caps_extend_the_line_by_half_the_thickness() => Run(() =>
		AssertRect(new LineGeometry(new Point(10, 10), new Point(30, 10))
			.GetRenderBounds(PenOf(4, PenLineCap.Round)), 8, 8, 24, 4));

	[Fact]
	public void Diagonal_flat_line_grows_by_half_thickness_times_sin45() => Run(() =>
	{
		double d = Math.Sqrt(2) / 2;
		AssertRect(new LineGeometry(new Point(1, 1), new Point(50, 50)).GetRenderBounds(PenOf(2)),
			1 - d, 1 - d, 49 + 2 * d, 49 + 2 * d);
	});

	[Fact]
	public void Ellipse_grows_by_half_the_thickness_on_every_side() => Run(() =>
		AssertRect(new EllipseGeometry(new Point(50, 50), 25, 25).GetRenderBounds(PenOf(4)), 23, 23, 54, 54));

	[Fact]
	public void Rectangle_with_miter_corners_grows_by_half_the_thickness() => Run(() =>
		AssertRect(new RectangleGeometry(new Rect(10, 20, 30, 40)).GetRenderBounds(PenOf(6)), 7, 17, 36, 46));

	[Fact]
	public void Triangle_miter_tip_extends_past_the_bevel_corner() => Run(() =>
	{
		var (miterRight, _) = ShapeExpectations.PolygonFarCorner(new[] { (1.0, 50.0), (10.0, 80.0), (50.0, 50.0) }, 2, 10);
		var miter = Polygon(PenOf(2, join: PenLineJoin.Miter));
		var bevel = Polygon(PenOf(2, join: PenLineJoin.Bevel));
		Assert.InRange(miter.Right, miterRight - Eps, miterRight + Eps);
		Assert.True(miter.Right > bevel.Right + 1, $"miter {miter.Right} vs bevel {bevel.Right}");
	});

	[Fact]
	public void Miter_limit_below_the_ratio_falls_back_to_a_bevel() => Run(() =>
	{
		var limited = Polygon(PenOf(2, miter: 2));
		var bevel = Polygon(PenOf(2, join: PenLineJoin.Bevel));
		Assert.InRange(limited.Right, bevel.Right - Eps, bevel.Right + Eps);
	});

	static Rect Polygon(Pen pen)
	{
		var figure = new PathFigure { StartPoint = new Point(1, 50), IsClosed = true };
		figure.Segments.Add(new LineSegment(new Point(10, 80)));
		figure.Segments.Add(new LineSegment(new Point(50, 50)));
		var g = new PathGeometry();
		g.Figures.Add(figure);
		return g.GetRenderBounds(pen);
	}

	[Fact]
	public void Geometry_transform_is_applied_but_the_pen_is_not_scaled() => Run(() =>
	{
		var g = new LineGeometry(new Point(0, 0), new Point(10, 0)) { Transform = new ScaleTransform(2, 2) };
		AssertRect(g.GetRenderBounds(PenOf(4)), 0, -2, 20, 4);
	});

	[Fact]
	public void Pen_defaults_match_wpf() => Run(() =>
	{
		var pen = new Pen();
		Assert.Equal(PenLineCap.Flat, pen.StartLineCap);
		Assert.Equal(PenLineCap.Flat, pen.EndLineCap);
		Assert.Equal(PenDashCap.Flat, pen.DashCap);
		Assert.Equal(PenLineJoin.Miter, pen.LineJoin);
		Assert.Equal(10.0, pen.MiterLimit);
	});
}
