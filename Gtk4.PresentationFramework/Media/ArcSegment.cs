using System;

namespace System.Windows.Media;

/// <summary>
/// An elliptical-arc segment from the current point to <see cref="Point"/>.
/// Mirrors <c>System.Windows.Media.ArcSegment</c>.
///
/// <para>Rendering: the arc is decomposed into one or more cubic Béziers
/// (≤90° each) via <see cref="ArcToBezier"/>, then emitted with
/// <c>Cairo.Context.CurveTo</c>. When the radii are zero or the start and
/// end points coincide the segment degenerates to a straight line.</para>
///
/// <para>Bounds: <see cref="PathGeometry.Bounds"/> expands over the Bézier
/// control points, which may over-estimate the true arc bounding box.</para>
/// </summary>
public class ArcSegment : PathSegment
{
	/// <summary>Creates a default arc segment.</summary>
	public ArcSegment() { }

	/// <summary>Creates an arc segment with all parameters specified.</summary>
	public ArcSegment(
		System.Windows.Point point,
		System.Windows.Size size,
		double rotationAngle,
		bool isLargeArc,
		SweepDirection sweepDirection,
		bool isStroked = true)
	{
		Point = point;
		Size = size;
		RotationAngle = rotationAngle;
		IsLargeArc = isLargeArc;
		SweepDirection = sweepDirection;
		IsStroked = isStroked;
	}

	/// <summary>The end point of the arc.</summary>
	public System.Windows.Point Point { get => _point; set => Set(ref _point, value); }
	System.Windows.Point _point;

	/// <summary>
	/// The radii of the ellipse: <c>Width</c> = rx, <c>Height</c> = ry.
	/// </summary>
	public System.Windows.Size Size { get => _size; set => Set(ref _size, value); }
	System.Windows.Size _size;

	/// <summary>Rotation of the ellipse's x-axis in degrees.</summary>
	public double RotationAngle { get => _rotationAngle; set => Set(ref _rotationAngle, value); }
	double _rotationAngle;

	/// <summary>Whether to use the larger of the two possible arc sweeps.</summary>
	public bool IsLargeArc { get => _isLargeArc; set => Set(ref _isLargeArc, value); }
	bool _isLargeArc;

	/// <summary>Direction in which the arc is drawn.</summary>
	public SweepDirection SweepDirection { get => _sweepDirection; set => Set(ref _sweepDirection, value); }
	SweepDirection _sweepDirection = SweepDirection.Counterclockwise;

	/// <summary>Whether the segment contributes to the stroke.</summary>
	public bool IsStroked { get => _isStroked; set => Set(ref _isStroked, value); }
	bool _isStroked = true;

	/// <inheritdoc/>
	internal override void AddToPath(Cairo.Context cr)
	{
		double x1 = cr.CurrentPoint.X;
		double y1 = cr.CurrentPoint.Y;

		var curves = ArcToBezier.Convert(
			x1, y1,
			Size.Width, Size.Height, RotationAngle,
			IsLargeArc, SweepDirection,
			Point.X, Point.Y);

		if (curves.Count == 0)
		{
			// Degenerate: fall back to a straight line (WPF behaviour).
			cr.LineTo(Point.X, Point.Y);
			return;
		}

		foreach (var (cp1x, cp1y, cp2x, cp2y, ex, ey) in curves)
			cr.CurveTo(cp1x, cp1y, cp2x, cp2y, ex, ey);
	}
}
