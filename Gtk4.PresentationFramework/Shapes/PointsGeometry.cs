using System.Windows.Media;

namespace System.Windows.Shapes;

/// <summary>Builds the one-figure geometry that defines a Polygon or Polyline.</summary>
static class PointsGeometry
{
	/// <summary>A closed (Polygon) or open (Polyline) figure through <paramref name="points"/>; null when empty.</summary>
	internal static PathGeometry? Create(PointCollection? points, FillRule fillRule, bool closed)
	{
		if (points == null || points.Count == 0) return null;
		var figure = new PathFigure { StartPoint = points[0], IsClosed = closed };
		for (int i = 1; i < points.Count; i++)
			figure.Segments.Add(new LineSegment { Point = points[i] });
		var geometry = new PathGeometry { FillRule = fillRule };
		geometry.Figures.Add(figure);
		return geometry;
	}
}
