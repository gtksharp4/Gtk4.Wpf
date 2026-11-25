using System;
using System.Collections.Generic;

namespace System.Windows.Media;

/// <summary>
/// A sub-path within a <see cref="PathGeometry"/>: a start point and a list of segments.
/// Mirrors <c>System.Windows.Media.PathFigure</c>.
/// </summary>
public class PathFigure
{
	Point _startPoint;
	bool _isClosed;
	bool _isFilled = true;
	PathSegmentCollection _segments;

	internal Action? Changed;

	/// <summary>Creates an empty figure.</summary>
	public PathFigure() => _segments = Hook(new PathSegmentCollection());

	/// <summary>Start point of this figure.</summary>
	public Point StartPoint
	{
		get => _startPoint;
		set { _startPoint = value; Changed?.Invoke(); }
	}

	/// <summary>Whether the figure is closed (last point joined to first).</summary>
	public bool IsClosed
	{
		get => _isClosed;
		set { _isClosed = value; Changed?.Invoke(); }
	}

	/// <summary>Whether the figure contributes to the fill area.</summary>
	public bool IsFilled
	{
		get => _isFilled;
		set { _isFilled = value; Changed?.Invoke(); }
	}

	/// <summary>The segments that make up this figure; edits raise the owning geometry's Changed.</summary>
	public IList<PathSegment> Segments => _segments;

	PathSegmentCollection Hook(PathSegmentCollection segments)
	{
		segments.Changed = () => Changed?.Invoke();
		return segments;
	}

	/// <summary>Adds this figure's path to <paramref name="cr"/>.</summary>
	internal void BuildPath(Cairo.Context cr)
	{
		cr.MoveTo(StartPoint.X, StartPoint.Y);
		foreach (var seg in _segments)
			seg.AddToPath(cr);
		if (IsClosed)
			cr.ClosePath();
	}
}
