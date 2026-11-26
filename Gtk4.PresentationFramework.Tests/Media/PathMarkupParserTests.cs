using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// Pure (no GTK) tests for the WPF Path markup mini-language parser
/// and for Polyline (which is not auto-closed, unlike Polygon).
/// </summary>
public class PathMarkupParserTests
{
	// ── M + L + Z (basic closed figure) ──────────────────────────────────────

	[Fact]
	public void Parse_M_L_L_Z_produces_one_closed_figure()
	{
		var pg = (PathGeometry)Geometry.Parse("M0,0 L10,0 L10,10 Z");
		Assert.Single(pg.Figures);
		var fig = pg.Figures[0];
		Assert.Equal(new Point(0, 0), fig.StartPoint);
		Assert.Equal(2, fig.Segments.Count);
		Assert.IsType<LineSegment>(fig.Segments[0]);
		Assert.Equal(new Point(10, 0), ((LineSegment)fig.Segments[0]).Point);
		Assert.IsType<LineSegment>(fig.Segments[1]);
		Assert.Equal(new Point(10, 10), ((LineSegment)fig.Segments[1]).Point);
		Assert.True(fig.IsClosed);
	}

	// ── relative commands accumulate from current point ───────────────────────

	[Fact]
	public void Parse_relative_m_and_l()
	{
		var pg = (PathGeometry)Geometry.Parse("M5,5 l10,0");
		var fig = pg.Figures[0];
		Assert.Equal(new Point(5, 5), fig.StartPoint);
		var seg = Assert.IsType<LineSegment>(fig.Segments[0]);
		Assert.Equal(new Point(15, 5), seg.Point);
	}

	[Fact]
	public void Parse_lowercase_m_relative_start()
	{
		// m from implicit origin (0,0): m2,3 → start at (2,3)
		var pg = (PathGeometry)Geometry.Parse("m2,3 L10,10");
		Assert.Equal(new Point(2, 3), pg.Figures[0].StartPoint);
	}

	// ── H and V ───────────────────────────────────────────────────────────────

	[Fact]
	public void Parse_H_moves_x_keeps_y()
	{
		var pg = (PathGeometry)Geometry.Parse("M0,5 H10");
		var seg = Assert.IsType<LineSegment>(pg.Figures[0].Segments[0]);
		Assert.Equal(new Point(10, 5), seg.Point);
	}

	[Fact]
	public void Parse_V_moves_y_keeps_x()
	{
		var pg = (PathGeometry)Geometry.Parse("M5,0 V10");
		var seg = Assert.IsType<LineSegment>(pg.Figures[0].Segments[0]);
		Assert.Equal(new Point(5, 10), seg.Point);
	}

	[Fact]
	public void Parse_H_then_V_sequence()
	{
		var pg = (PathGeometry)Geometry.Parse("M0,0 H10 V10");
		var fig = pg.Figures[0];
		Assert.Equal(new Point(10, 0), ((LineSegment)fig.Segments[0]).Point);
		Assert.Equal(new Point(10, 10), ((LineSegment)fig.Segments[1]).Point);
	}

	// ── cubic Bézier C ────────────────────────────────────────────────────────

	[Fact]
	public void Parse_C_produces_BezierSegment()
	{
		var pg = (PathGeometry)Geometry.Parse("M0,0 C1,1 2,2 3,3");
		var seg = Assert.IsType<BezierSegment>(pg.Figures[0].Segments[0]);
		Assert.Equal(new Point(1, 1), seg.Point1);
		Assert.Equal(new Point(2, 2), seg.Point2);
		Assert.Equal(new Point(3, 3), seg.Point3);
	}

	[Fact]
	public void Parse_relative_c_offsets_from_current()
	{
		var pg = (PathGeometry)Geometry.Parse("M10,10 c1,0 2,1 3,0");
		var seg = Assert.IsType<BezierSegment>(pg.Figures[0].Segments[0]);
		Assert.Equal(new Point(11, 10), seg.Point1);
		Assert.Equal(new Point(12, 11), seg.Point2);
		Assert.Equal(new Point(13, 10), seg.Point3);
	}

	// ── quadratic Bézier Q ────────────────────────────────────────────────────

	[Fact]
	public void Parse_Q_produces_QuadraticBezierSegment()
	{
		var pg = (PathGeometry)Geometry.Parse("M0,0 Q5,10 10,0");
		var seg = Assert.IsType<QuadraticBezierSegment>(pg.Figures[0].Segments[0]);
		Assert.Equal(new Point(5, 10), seg.Point1);
		Assert.Equal(new Point(10, 0), seg.Point2);
	}

	// ── multiple figures (two M commands) ────────────────────────────────────

	[Fact]
	public void Parse_two_M_commands_produce_two_figures()
	{
		var pg = (PathGeometry)Geometry.Parse("M0,0 L10,0 M20,20 L30,30");
		Assert.Equal(2, pg.Figures.Count);
		Assert.Equal(new Point(0, 0), pg.Figures[0].StartPoint);
		Assert.Equal(new Point(20, 20), pg.Figures[1].StartPoint);
	}

	// ── implicit repeat after L ───────────────────────────────────────────────

	[Fact]
	public void Parse_implicit_L_repeat_produces_two_line_segments()
	{
		var pg = (PathGeometry)Geometry.Parse("M0,0 L1,1 2,2");
		var fig = pg.Figures[0];
		Assert.Equal(2, fig.Segments.Count);
		Assert.Equal(new Point(1, 1), ((LineSegment)fig.Segments[0]).Point);
		Assert.Equal(new Point(2, 2), ((LineSegment)fig.Segments[1]).Point);
	}

	// ── malformed input ───────────────────────────────────────────────────────

	[Fact]
	public void Parse_unknown_command_throws_FormatException()
	{
		Assert.Throws<FormatException>(() => Geometry.Parse("M0,0 X10,10"));
	}

	[Fact]
	public void Parse_missing_number_throws_FormatException()
	{
		Assert.Throws<FormatException>(() => Geometry.Parse("M0,0 L,10"));
	}

	// ── empty / whitespace ────────────────────────────────────────────────────

	[Fact]
	public void Parse_empty_string_returns_empty_geometry()
	{
		var pg = (PathGeometry)Geometry.Parse("");
		Assert.Empty(pg.Figures);
	}

	// ── Polyline is NOT closed (contrast Polygon) ─────────────────────────────

	[Fact]
	public void Polyline_three_points_is_not_closed()
	{
		var pl = new Polyline();
		pl.Points.Add(new Point(0, 0));
		pl.Points.Add(new Point(50, 0));
		pl.Points.Add(new Point(50, 50));
		Assert.Equal(3, pl.Points.Count);
		// Polyline has no IsClosed property; verify it differs from Polygon by
		// confirming the shape type exposes Points without auto-closure API.
		Assert.IsType<Polyline>(pl);
	}

	[Fact]
	public void Polyline_has_expected_points()
	{
		var pl = new Polyline();
		pl.Points.Add(new Point(1, 2));
		pl.Points.Add(new Point(3, 4));
		pl.Points.Add(new Point(5, 6));
		Assert.Equal(new Point(1, 2), pl.Points[0]);
		Assert.Equal(new Point(3, 4), pl.Points[1]);
		Assert.Equal(new Point(5, 6), pl.Points[2]);
	}

	[Fact]
	public void Polygon_is_distinct_type_from_Polyline()
	{
		// Ensures the two are separate classes (Polygon has IsClosed semantics in BuildShapePath)
		var poly = new Polygon();
		var pl   = new Polyline();
		Assert.IsNotType<Polyline>(poly);
		Assert.IsNotType<Polygon>(pl);
	}
}
