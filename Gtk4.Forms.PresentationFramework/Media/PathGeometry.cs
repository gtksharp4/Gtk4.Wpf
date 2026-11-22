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

// ── PathFigure ────────────────────────────────────────────────────────────

/// <summary>
/// A sub-path within a <see cref="PathGeometry"/>: a start point and a list of segments.
/// Mirrors <c>System.Windows.Media.PathFigure</c>.
/// </summary>
public class PathFigure
{
	/// <summary>Start point of this figure.</summary>
	public System.Windows.Point StartPoint { get; set; }

	/// <summary>Whether the figure is closed (last point joined to first).</summary>
	public bool IsClosed { get; set; }

	/// <summary>Whether the figure contributes to the fill area.</summary>
	public bool IsFilled { get; set; } = true;

	/// <summary>The segments that make up this figure.</summary>
	public IList<PathSegment> Segments { get; } = new List<PathSegment>();

	/// <summary>Adds this figure's path to <paramref name="cr"/>.</summary>
	internal void BuildPath(Cairo.Context cr)
	{
		cr.MoveTo(StartPoint.X, StartPoint.Y);
		foreach (var seg in Segments)
			seg.AddToPath(cr);
		if (IsClosed)
			cr.ClosePath();
	}
}

// ── PathGeometry ─────────────────────────────────────────────────────────

/// <summary>
/// A geometry built from one or more <see cref="PathFigure"/> instances.
/// Mirrors <c>System.Windows.Media.PathGeometry</c>.
/// </summary>
public partial class PathGeometry : Geometry
{
	/// <summary>Creates an empty path geometry.</summary>
	public PathGeometry() { }

	/// <summary>Creates a path geometry pre-populated with <paramref name="figures"/>.</summary>
	public PathGeometry(IEnumerable<PathFigure> figures)
	{
		foreach (var f in figures)
			Figures.Add(f);
	}

	/// <summary>The collection of path figures.</summary>
	public PathFigureCollection Figures { get; set; } = new PathFigureCollection();

	/// <inheritdoc/>
	public override System.Windows.Rect Bounds
	{
		get
		{
			double minX = double.MaxValue, minY = double.MaxValue;
			double maxX = double.MinValue, maxY = double.MinValue;
			bool any = false;
			foreach (var fig in Figures)
			{
				void Expand(double x, double y)
				{
					if (x < minX) minX = x;
					if (y < minY) minY = y;
					if (x > maxX) maxX = x;
					if (y > maxY) maxY = y;
					any = true;
				}
				Expand(fig.StartPoint.X, fig.StartPoint.Y);
				foreach (var seg in fig.Segments)
				{
					if (seg is LineSegment ls) Expand(ls.Point.X, ls.Point.Y);
					else if (seg is BezierSegment bs)
					{
						Expand(bs.Point1.X, bs.Point1.Y);
						Expand(bs.Point2.X, bs.Point2.Y);
						Expand(bs.Point3.X, bs.Point3.Y);
					}
					else if (seg is QuadraticBezierSegment qs)
					{
						Expand(qs.Point1.X, qs.Point1.Y);
						Expand(qs.Point2.X, qs.Point2.Y);
					}
						else if (seg is ArcSegment arc)
						{
							// Approximate: expand over endpoint only.
							// May under-estimate for arcs bulging beyond start/end.
							Expand(arc.Point.X, arc.Point.Y);
						}
				}
			}
			return any
				? new System.Windows.Rect(minX, minY, maxX - minX, maxY - minY)
				: new System.Windows.Rect(0, 0, 0, 0);
		}
	}

	/// <inheritdoc/>
	internal override void BuildPath(Cairo.Context cr)
	{
		cr.FillRule = FillRule == FillRule.Nonzero ? Cairo.FillRule.Winding : Cairo.FillRule.EvenOdd;
		foreach (var fig in Figures)
			fig.BuildPath(cr);
	}
}