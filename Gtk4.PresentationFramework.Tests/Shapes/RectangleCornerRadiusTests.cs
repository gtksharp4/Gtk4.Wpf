using System;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;
using WpfRectangle = System.Windows.Shapes.Rectangle;

namespace Gtk4.PresentationFramework.Tests.Shapes;

/// <summary>
/// Pins the F5 fix: rounded-rectangle corner centres use rx horizontally and
/// ry vertically. The pixel render itself is not unit-tested; the pure helper
/// <see cref="WpfRectangle.CornerArcCentre"/> is asserted instead.
/// Coverage limit: EllipticArc/Cairo path output is not verified — that requires
/// a GTK drawing surface.
/// </summary>
public class RectangleCornerRadiusTests
{
	// ── CornerArcCentre: horizontal offset uses rx ────────────────────────────

	[Theory]
	[InlineData(0)] // top-right
	[InlineData(1)] // bottom-right
	public void Right_corners_cx_offset_uses_rx(int corner)
	{
		double x = 0, y = 0, w = 100, h = 60, rx = 15, ry = 8;
		var (cx, _) = WpfRectangle.CornerArcCentre(x, y, w, h, rx, ry, corner);
		// right corners: cx = x + w - rx
		Assert.Equal(x + w - rx, cx, precision: 10);
	}

	[Theory]
	[InlineData(2)] // bottom-left
	[InlineData(3)] // top-left
	public void Left_corners_cx_offset_uses_rx(int corner)
	{
		double x = 0, y = 0, w = 100, h = 60, rx = 15, ry = 8;
		var (cx, _) = WpfRectangle.CornerArcCentre(x, y, w, h, rx, ry, corner);
		// left corners: cx = x + rx
		Assert.Equal(x + rx, cx, precision: 10);
	}

	// ── CornerArcCentre: vertical offset uses ry ──────────────────────────────

	[Theory]
	[InlineData(0)] // top-right
	[InlineData(3)] // top-left
	public void Top_corners_cy_offset_uses_ry(int corner)
	{
		double x = 0, y = 0, w = 100, h = 60, rx = 15, ry = 8;
		var (_, cy) = WpfRectangle.CornerArcCentre(x, y, w, h, rx, ry, corner);
		// top corners: cy = y + ry
		Assert.Equal(y + ry, cy, precision: 10);
	}

	[Theory]
	[InlineData(1)] // bottom-right
	[InlineData(2)] // bottom-left
	public void Bottom_corners_cy_offset_uses_ry(int corner)
	{
		double x = 0, y = 0, w = 100, h = 60, rx = 15, ry = 8;
		var (_, cy) = WpfRectangle.CornerArcCentre(x, y, w, h, rx, ry, corner);
		// bottom corners: cy = y + h - ry
		Assert.Equal(y + h - ry, cy, precision: 10);
	}

	// ── cx/cy are independent of ry/rx respectively ───────────────────────────

	[Fact]
	public void Cx_does_not_depend_on_ry()
	{
		double x = 10, y = 5, w = 200, h = 100, rx = 20;
		var (cx1, _) = WpfRectangle.CornerArcCentre(x, y, w, h, rx, ry: 5,  corner: 0);
		var (cx2, _) = WpfRectangle.CornerArcCentre(x, y, w, h, rx, ry: 30, corner: 0);
		Assert.Equal(cx1, cx2, precision: 10);
	}

	[Fact]
	public void Cy_does_not_depend_on_rx()
	{
		double x = 10, y = 5, w = 200, h = 100, ry = 20;
		var (_, cy1) = WpfRectangle.CornerArcCentre(x, y, w, h, rx: 5,  ry, corner: 1);
		var (_, cy2) = WpfRectangle.CornerArcCentre(x, y, w, h, rx: 40, ry, corner: 1);
		Assert.Equal(cy1, cy2, precision: 10);
	}

	// ── RectangleGeometry uses same helper (Geometry.cs F5 fix) ──────────────

	[Fact]
	public void RectangleGeometry_rounded_stores_distinct_radii()
	{
		var g = new RectangleGeometry(new System.Windows.Rect(0, 0, 100, 60), radiusX: 15, radiusY: 8);
		Assert.Equal(15, g.RadiusX);
		Assert.Equal(8,  g.RadiusY);
		// Verify they are stored independently (a bug where ry was used for both
		// would make RadiusX == RadiusY when set to different values).
		Assert.NotEqual(g.RadiusX, g.RadiusY);
	}
}