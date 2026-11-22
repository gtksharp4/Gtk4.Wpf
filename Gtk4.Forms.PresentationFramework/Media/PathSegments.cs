using System;
using System.Collections.Generic;
using System.Windows.Shapes;

namespace System.Windows.Media;

// ── Path segments ─────────────────────────────────────────────────────────

/// <summary>
/// Base class for a segment within a <see cref="PathFigure"/>.
/// Mirrors <c>System.Windows.Media.PathSegment</c>.
/// </summary>
public abstract class PathSegment
{
	/// <summary>Appends this segment to the current open path in <paramref name="cr"/>.</summary>
	internal abstract void AddToPath(Cairo.Context cr);
}

/// <summary>
/// A straight-line segment to a point.
/// Mirrors <c>System.Windows.Media.LineSegment</c>.
/// </summary>
public class LineSegment : PathSegment
{
	/// <summary>Creates a default line segment.</summary>
	public LineSegment() { }

	/// <summary>Creates a line segment to <paramref name="point"/>.</summary>
	public LineSegment(System.Windows.Point point, bool isStroked = true)
	{
		Point = point; IsStroked = isStroked;
	}

	/// <summary>The end point of the line.</summary>
	public System.Windows.Point Point { get; set; }

	/// <summary>Whether the segment contributes to the stroke.</summary>
	public bool IsStroked { get; set; } = true;

	/// <inheritdoc/>
	internal override void AddToPath(Cairo.Context cr) =>
		cr.LineTo(Point.X, Point.Y);
}

/// <summary>
/// A cubic Bézier segment.
/// Mirrors <c>System.Windows.Media.BezierSegment</c>.
/// </summary>
public class BezierSegment : PathSegment
{
	/// <summary>Creates an empty Bézier segment.</summary>
	public BezierSegment() { }

	/// <summary>Creates a Bézier segment.</summary>
	public BezierSegment(System.Windows.Point point1, System.Windows.Point point2,
		System.Windows.Point point3, bool isStroked = true)
	{
		Point1 = point1; Point2 = point2; Point3 = point3; IsStroked = isStroked;
	}

	/// <summary>First control point.</summary>
	public System.Windows.Point Point1 { get; set; }
	/// <summary>Second control point.</summary>
	public System.Windows.Point Point2 { get; set; }
	/// <summary>End point.</summary>
	public System.Windows.Point Point3 { get; set; }
	/// <summary>Whether the segment contributes to the stroke.</summary>
	public bool IsStroked { get; set; } = true;

	/// <inheritdoc/>
	internal override void AddToPath(Cairo.Context cr) =>
		cr.CurveTo(Point1.X, Point1.Y, Point2.X, Point2.Y, Point3.X, Point3.Y);
}

/// <summary>
/// A quadratic Bézier segment (one control point + end point).
/// Mirrors <c>System.Windows.Media.QuadraticBezierSegment</c>.
/// Internally converted to a cubic Bézier for Cairo rendering.
/// </summary>
public class QuadraticBezierSegment : PathSegment
{
	/// <summary>Creates an empty quadratic Bézier segment.</summary>
	public QuadraticBezierSegment() { }

	/// <summary>Creates a quadratic Bézier segment.</summary>
	public QuadraticBezierSegment(System.Windows.Point point1, System.Windows.Point point2,
		bool isStroked = true)
	{
		Point1 = point1; Point2 = point2; IsStroked = isStroked;
	}

	/// <summary>The control point.</summary>
	public System.Windows.Point Point1 { get; set; }
	/// <summary>The end point.</summary>
	public System.Windows.Point Point2 { get; set; }
	/// <summary>Whether the segment contributes to the stroke.</summary>
	public bool IsStroked { get; set; } = true;

	/// <inheritdoc/>
	internal override void AddToPath(Cairo.Context cr)
	{
		// Elevate quadratic P0,P1,P2 to cubic: cp1 = P0 + 2/3*(P1-P0), cp2 = P2 + 2/3*(P1-P2)
		double p0x = cr.CurrentPoint.X, p0y = cr.CurrentPoint.Y;
		double cp1x = p0x + 2.0 / 3.0 * (Point1.X - p0x);
		double cp1y = p0y + 2.0 / 3.0 * (Point1.Y - p0y);
		double cp2x = Point2.X + 2.0 / 3.0 * (Point1.X - Point2.X);
		double cp2y = Point2.Y + 2.0 / 3.0 * (Point1.Y - Point2.Y);
		cr.CurveTo(cp1x, cp1y, cp2x, cp2y, Point2.X, Point2.Y);
	}
}
