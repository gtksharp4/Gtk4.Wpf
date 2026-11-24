using System;
using System.Collections.Generic;
using System.Windows.Shapes;

namespace System.Windows.Media;

// ── PathGeometry ─────────────────────────────────────────────────────────

/// <summary>
/// A geometry built from one or more <see cref="PathFigure"/> instances.
/// Mirrors <c>System.Windows.Media.PathGeometry</c>.
/// </summary>
public partial class PathGeometry : Geometry
{
	/// <summary>Creates an empty path geometry.</summary>
	public PathGeometry() => Hook(_figures);

	/// <summary>Creates a path geometry pre-populated with <paramref name="figures"/>.</summary>
	public PathGeometry(IEnumerable<PathFigure> figures)
	{
		Hook(_figures);
		foreach (var f in figures)
			Figures.Add(f);
	}

	PathFigureCollection _figures = new PathFigureCollection();

	/// <summary>The collection of path figures; adding, removing or editing a figure raises <c>Changed</c>.</summary>
	public PathFigureCollection Figures
	{
		get => _figures;
		set
		{
			ThrowIfFrozen();
			if (value == null) throw new ArgumentNullException(nameof(value));
			_figures.Changed = null;
			_figures = value;
			Hook(value);
			OnChanged();
		}
	}

	void Hook(PathFigureCollection figures) => figures.Changed = () => OnChanged();

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