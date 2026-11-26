using System;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// Pure tests for Geometry bounds and Transform matrix math.
/// </summary>
public class GeometryTests
{
	// ── RectangleGeometry ────────────────────────────────────────────────────

	[Fact]
	public void RectangleGeometry_Bounds_matches_rect()
	{
		var g = new RectangleGeometry(new Rect(10, 20, 100, 50));
		Assert.Equal(new Rect(10, 20, 100, 50), g.Bounds);
	}

	[Fact]
	public void RectangleGeometry_default_Bounds_is_zero()
	{
		var g = new RectangleGeometry();
		Assert.Equal(new Rect(0, 0, 0, 0), g.Bounds);
	}

	[Fact]
	public void RectangleGeometry_with_radii_stores_them()
	{
		var g = new RectangleGeometry(new Rect(0, 0, 100, 60), 10, 5);
		Assert.Equal(10, g.RadiusX);
		Assert.Equal(5, g.RadiusY);
	}

	// ── EllipseGeometry ──────────────────────────────────────────────────────

	[Fact]
	public void EllipseGeometry_Bounds_is_bounding_box()
	{
		var g = new EllipseGeometry(new Point(50, 40), 30, 20);
		Assert.Equal(new Rect(20, 20, 60, 40), g.Bounds);
	}

	[Fact]
	public void EllipseGeometry_circle_has_equal_radii()
	{
		var g = new EllipseGeometry(new Point(0, 0), 15, 15);
		Assert.Equal(g.Bounds.Width, g.Bounds.Height);
	}

	// ── LineGeometry ─────────────────────────────────────────────────────────

	[Fact]
	public void LineGeometry_Bounds_contains_both_points()
	{
		var g = new LineGeometry(new Point(5, 10), new Point(50, 80));
		Assert.Equal(5, g.Bounds.X);
		Assert.Equal(10, g.Bounds.Y);
		Assert.Equal(45, g.Bounds.Width);
		Assert.Equal(70, g.Bounds.Height);
	}

	[Fact]
	public void LineGeometry_reversed_points_gives_same_bounds()
	{
		var a = new LineGeometry(new Point(10, 20), new Point(50, 60));
		var b = new LineGeometry(new Point(50, 60), new Point(10, 20));
		Assert.Equal(a.Bounds, b.Bounds);
	}

	// ── PathGeometry bounds ──────────────────────────────────────────────────

	[Fact]
	public void PathGeometry_empty_Bounds_is_zero()
	{
		var g = new PathGeometry();
		Assert.Equal(new Rect(0, 0, 0, 0), g.Bounds);
	}

	[Fact]
	public void PathGeometry_Bounds_covers_all_figures()
	{
		var fig = new PathFigure
		{
			StartPoint = new Point(5, 5)
		};
		fig.Segments.Add(new LineSegment(new Point(100, 200)));
		var g = new PathGeometry(new[] { fig });
		Assert.Equal(5, g.Bounds.X);
		Assert.Equal(5, g.Bounds.Y);
		Assert.Equal(95, g.Bounds.Width);
		Assert.Equal(195, g.Bounds.Height);
	}
}