using System;

namespace System.Windows.Media;

/// <summary>
/// A line segment geometry from one point to another.
/// Mirrors <c>System.Windows.Media.LineGeometry</c>.
/// </summary>
public class LineGeometry : Geometry
{
	/// <summary>Creates a default line.</summary>
	public LineGeometry() { }

	/// <summary>Creates a line from <paramref name="startPoint"/> to <paramref name="endPoint"/>.</summary>
	public LineGeometry(System.Windows.Point startPoint, System.Windows.Point endPoint)
	{
		StartPoint = startPoint; EndPoint = endPoint;
	}

	/// <summary>Identifies <see cref="StartPoint"/>.</summary>
	public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(
		nameof(StartPoint), typeof(System.Windows.Point), typeof(LineGeometry),
		new PropertyMetadata(new System.Windows.Point()));

	/// <summary>Start point.</summary>
	public System.Windows.Point StartPoint
	{
		get => (System.Windows.Point)GetValue(StartPointProperty)!;
		set => SetValue(StartPointProperty, value);
	}

	/// <summary>Identifies <see cref="EndPoint"/>.</summary>
	public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
		nameof(EndPoint), typeof(System.Windows.Point), typeof(LineGeometry),
		new PropertyMetadata(new System.Windows.Point()));

	/// <summary>End point.</summary>
	public System.Windows.Point EndPoint
	{
		get => (System.Windows.Point)GetValue(EndPointProperty)!;
		set => SetValue(EndPointProperty, value);
	}

	/// <inheritdoc/>
	public override System.Windows.Rect Bounds => new System.Windows.Rect(
		Math.Min(StartPoint.X, EndPoint.X), Math.Min(StartPoint.Y, EndPoint.Y),
		Math.Abs(EndPoint.X - StartPoint.X), Math.Abs(EndPoint.Y - StartPoint.Y));

	/// <inheritdoc/>
	internal override void BuildPath(Cairo.Context cr)
	{
		cr.MoveTo(StartPoint.X, StartPoint.Y);
		cr.LineTo(EndPoint.X, EndPoint.Y);
	}
}
