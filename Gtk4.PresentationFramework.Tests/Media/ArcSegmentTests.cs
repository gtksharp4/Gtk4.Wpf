using System;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// Pure (no GTK) tests for ArcSegment, ArcToBezier, and the A/a path command.
/// </summary>
public class ArcSegmentTests
{
	const double Tol = 1e-4;

	// ── ArcToBezier — quarter circle ─────────────────────────────────────────

	/// <summary>
	/// 90° arc from (1,0) to (0,1) on unit circle, CW.
	/// One cubic whose control points use the standard kappa ≈ 0.5523.
	/// </summary>
	[Fact]
	public void ArcToBezier_QuarterCircle_SingleCubic_KappaControlPoints()
	{
		// From (1,0) to (0,1), r=1, no rotation, not large-arc, clockwise
		var curves = ArcToBezier.Convert(
			1, 0, 1, 1, 0, false, SweepDirection.Clockwise, 0, 1);

		Assert.Single(curves);
		var (cp1x, cp1y, cp2x, cp2y, ex, ey) = curves[0];

		// End point must land on (0,1)
		Assert.Equal(0.0, ex, 3);
		Assert.Equal(1.0, ey, 3);

		// kappa = 4/3*(sqrt(2)-1) ≈ 0.5523
		double kappa = 4.0 / 3.0 * (Math.Sqrt(2) - 1);
		Assert.Equal(1.0,    cp1x, 3);
		Assert.Equal(kappa,  cp1y, 3);
		Assert.Equal(kappa,  cp2x, 3);
		Assert.Equal(1.0,    cp2y, 3);
	}

	// ── ArcToBezier — semicircle splits into ≥2 cubics ───────────────────────

	[Fact]
	public void ArcToBezier_Semicircle_IsLargeArc_TwoOrMoreCubics()
	{
		// 180° arc (large-arc) from (1,0) to (-1,0), r=1
		var curves = ArcToBezier.Convert(
			1, 0, 1, 1, 0, true, SweepDirection.Clockwise, -1, 0);

		Assert.True(curves.Count >= 2, $"Expected ≥2 cubics for semicircle, got {curves.Count}");

		// Last cubic must end at (-1,0)
		var last = curves[curves.Count - 1];
		Assert.Equal(-1.0, last.ex, 3);
		Assert.Equal(0.0, last.ey, 3);
	}

	// ── ArcToBezier — degenerate zero radius → empty list ────────────────────

	[Fact]
	public void ArcToBezier_ZeroRadius_ReturnsEmpty()
	{
		var curves = ArcToBezier.Convert(0, 0, 0, 0, 0, false, SweepDirection.Clockwise, 10, 0);
		Assert.Empty(curves);
	}

	// ── ArcToBezier — coincident endpoints → empty list ─────────────────────

	[Fact]
	public void ArcToBezier_CoincidentPoints_ReturnsEmpty()
	{
		var curves = ArcToBezier.Convert(5, 5, 10, 10, 0, false, SweepDirection.Clockwise, 5, 5);
		Assert.Empty(curves);
	}

	// ── ArcSegment DP defaults ────────────────────────────────────────────────

	[Fact]
	public void ArcSegment_DefaultProperties()
	{
		var seg = new ArcSegment();
		Assert.Equal(new Point(0, 0), seg.Point);
		Assert.Equal(new Size(0, 0), seg.Size);
		Assert.Equal(0.0, seg.RotationAngle);
		Assert.False(seg.IsLargeArc);
		Assert.Equal(SweepDirection.Counterclockwise, seg.SweepDirection);
		Assert.True(seg.IsStroked);
	}

	[Fact]
	public void ArcSegment_ConstructorRoundTrips()
	{
		var seg = new ArcSegment(
			new Point(10, 20), new Size(5, 7), 30, true, SweepDirection.Clockwise, false);
		Assert.Equal(new Point(10, 20), seg.Point);
		Assert.Equal(new Size(5, 7), seg.Size);
		Assert.Equal(30.0, seg.RotationAngle);
		Assert.True(seg.IsLargeArc);
		Assert.Equal(SweepDirection.Clockwise, seg.SweepDirection);
		Assert.False(seg.IsStroked);
	}

	// ── Geometry.Parse A command ──────────────────────────────────────────────

	[Fact]
	public void Parse_A_AbsoluteArc_ProducesArcSegment()
	{
		var pg = (PathGeometry)Geometry.Parse("M0,0 A5,5 0 0 1 10,0");
		Assert.Single(pg.Figures);
		var fig = pg.Figures[0];
		Assert.Single(fig.Segments);
		var arc = Assert.IsType<ArcSegment>(fig.Segments[0]);
		Assert.Equal(new Point(10, 0), arc.Point);
		Assert.Equal(new Size(5, 5), arc.Size);
		Assert.Equal(0.0, arc.RotationAngle);
		Assert.False(arc.IsLargeArc);
		Assert.Equal(SweepDirection.Clockwise, arc.SweepDirection);
	}

	[Fact]
	public void Parse_A_LargeArcFlag_IsTrue()
	{
		var pg = (PathGeometry)Geometry.Parse("M0,0 A5,5 0 1 1 10,0");
		var arc = Assert.IsType<ArcSegment>(pg.Figures[0].Segments[0]);
		Assert.True(arc.IsLargeArc);
	}

	[Fact]
	public void Parse_A_CounterclockwiseSweep()
	{
		// sweep-flag=0 → Counterclockwise
		var pg = (PathGeometry)Geometry.Parse("M0,0 A5,5 0 0 0 10,0");
		var arc = Assert.IsType<ArcSegment>(pg.Figures[0].Segments[0]);
		Assert.Equal(SweepDirection.Counterclockwise, arc.SweepDirection);
	}

	// ── relative a command accumulates from current point ─────────────────────

	[Fact]
	public void Parse_a_RelativeArc_AccumulatesFromCurrentPoint()
	{
		// Start at (10,10); relative arc endpoint (5,0) → absolute (15,10)
		var pg = (PathGeometry)Geometry.Parse("M10,10 a5,5 0 0 1 5,0");
		var arc = Assert.IsType<ArcSegment>(pg.Figures[0].Segments[0]);
		Assert.Equal(new Point(15, 10), arc.Point);
	}

	// ── implicit repeat after A ───────────────────────────────────────────────

	[Fact]
	public void Parse_A_ImplicitRepeat_TwoArcSegments()
	{
		var pg = (PathGeometry)Geometry.Parse("M0,0 A5,5 0 0 1 10,0 5,5 0 0 1 20,0");
		Assert.Equal(2, pg.Figures[0].Segments.Count);
		Assert.IsType<ArcSegment>(pg.Figures[0].Segments[0]);
		Assert.IsType<ArcSegment>(pg.Figures[0].Segments[1]);
		Assert.Equal(new Point(20, 0), ((ArcSegment)pg.Figures[0].Segments[1]).Point);
	}

	// ── degenerate arc does not throw ─────────────────────────────────────────

	[Fact]
	public void ArcToBezier_ZeroRadius_NoThrow()
	{
		// Building a path with a zero-radius ArcSegment must not throw.
		var ex = Record.Exception(() =>
			ArcToBezier.Convert(0, 0, 0, 0, 0, false, SweepDirection.Clockwise, 5, 5));
		Assert.Null(ex);
	}

	// ── SweepDirection enum values ────────────────────────────────────────────

	[Fact]
	public void SweepDirection_Values()
	{
		Assert.Equal(0, (int)SweepDirection.Counterclockwise);
		Assert.Equal(1, (int)SweepDirection.Clockwise);
	}
}
