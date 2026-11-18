namespace System.Windows.Media;

/// <summary>
/// Mutable build state for <see cref="PathMarkupParser"/>: the current point and the
/// figure under construction. Tracks the current point so relative commands accumulate.
/// </summary>
internal sealed class PathParseContext
{
	PathFigure? _figure;

	/// <summary>Current X of the pen (absolute).</summary>
	public double CurX { get; private set; }

	/// <summary>Current Y of the pen (absolute).</summary>
	public double CurY { get; private set; }

	/// <summary>Begins a new figure at the given absolute start point.</summary>
	public void StartFigure(double x, double y)
	{
		_figure = new PathFigure { StartPoint = new Point(x, y) };
		CurX = x; CurY = y;
	}

	/// <summary>Adds a line segment to the current point.</summary>
	public void AddLine(double x, double y)
	{
		EnsureFigure(x, y);
		_figure!.Segments.Add(new LineSegment(new Point(x, y)));
		CurX = x; CurY = y;
	}

	/// <summary>Adds a cubic Bézier segment.</summary>
	public void AddCubic(Point p1, Point p2, Point p3)
	{
		EnsureFigure(p3.X, p3.Y);
		_figure!.Segments.Add(new BezierSegment(p1, p2, p3));
		CurX = p3.X; CurY = p3.Y;
	}

	/// <summary>Adds a quadratic Bézier segment.</summary>
	public void AddQuadratic(Point p1, Point p2)
	{
		EnsureFigure(p2.X, p2.Y);
		_figure!.Segments.Add(new QuadraticBezierSegment(p1, p2));
		CurX = p2.X; CurY = p2.Y;
	}

	/// <summary>Adds an elliptical-arc segment to <paramref name="endAbsolute"/>.</summary>
	public void AddArc(Size radii, double rotation, bool isLargeArc,
		SweepDirection sweep, Point endAbsolute)
	{
		EnsureFigure(endAbsolute.X, endAbsolute.Y);
		_figure!.Segments.Add(new ArcSegment(endAbsolute, radii, rotation, isLargeArc, sweep));
		CurX = endAbsolute.X; CurY = endAbsolute.Y;
	}

	/// <summary>
	/// Closes the current figure and flushes it. Per the SVG/WPF contract the current
	/// point moves back to the closed figure's start point, so a relative command that
	/// follows <c>Z</c> with no intervening <c>M</c> accumulates from there.
	/// </summary>
	public void CloseFigure(PathGeometry pg)
	{
		if (_figure == null) return;
		_figure.IsClosed = true;
		double sx = _figure.StartPoint.X, sy = _figure.StartPoint.Y;
		FlushFigure(pg);
		CurX = sx; CurY = sy;
	}

	/// <summary>Appends the current figure (if any) to <paramref name="pg"/>.</summary>
	public void FlushFigure(PathGeometry pg)
	{
		if (_figure != null) { pg.Figures.Add(_figure); _figure = null; }
	}

	void EnsureFigure(double x, double y)
	{
		if (_figure == null) StartFigure(x, y);
	}
}
