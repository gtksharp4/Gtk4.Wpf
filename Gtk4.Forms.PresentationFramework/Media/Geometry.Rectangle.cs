using System;

namespace System.Windows.Media;

/// <summary>
/// A rectangular geometry, optionally with rounded corners.
/// Mirrors <c>System.Windows.Media.RectangleGeometry</c>.
/// </summary>
public class RectangleGeometry : Geometry
{
	/// <summary>Creates an empty rectangle geometry.</summary>
	public RectangleGeometry() { }

	/// <summary>Creates a rectangle geometry from a <see cref="System.Windows.Rect"/>.</summary>
	public RectangleGeometry(System.Windows.Rect rect) => Rect = rect;

	/// <summary>Creates a rectangle geometry with rounded corners.</summary>
	public RectangleGeometry(System.Windows.Rect rect, double radiusX, double radiusY)
	{
		Rect = rect; RadiusX = radiusX; RadiusY = radiusY;
	}

	/// <summary>Identifies <see cref="Rect"/>.</summary>
	public static readonly DependencyProperty RectProperty = DependencyProperty.Register(
		nameof(Rect), typeof(System.Windows.Rect), typeof(RectangleGeometry), new PropertyMetadata(System.Windows.Rect.Empty));

	/// <summary>The rectangle.</summary>
	public System.Windows.Rect Rect
	{
		get => (System.Windows.Rect)GetValue(RectProperty)!;
		set => SetValue(RectProperty, value);
	}

	/// <summary>Identifies <see cref="RadiusX"/>.</summary>
	public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(
		nameof(RadiusX), typeof(double), typeof(RectangleGeometry), new PropertyMetadata(0.0));

	/// <summary>Horizontal corner radius.</summary>
	public double RadiusX
	{
		get => (double)GetValue(RadiusXProperty)!;
		set => SetValue(RadiusXProperty, value);
	}

	/// <summary>Identifies <see cref="RadiusY"/>.</summary>
	public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(
		nameof(RadiusY), typeof(double), typeof(RectangleGeometry), new PropertyMetadata(0.0));

	/// <summary>Vertical corner radius.</summary>
	public double RadiusY
	{
		get => (double)GetValue(RadiusYProperty)!;
		set => SetValue(RadiusYProperty, value);
	}

	/// <inheritdoc/>
	public override System.Windows.Rect Bounds => Rect.IsEmpty ? new System.Windows.Rect(0, 0, 0, 0) : Rect;

	/// <inheritdoc/>
	internal override void BuildPath(Cairo.Context cr)
	{
		if (Rect.IsEmpty) return;
		double x = Rect.X, y = Rect.Y, w = Rect.Width, h = Rect.Height;
		double rx = Math.Min(RadiusX, w / 2), ry = Math.Min(RadiusY, h / 2);
		if (rx <= 0 || ry <= 0)
		{
			cr.Rectangle(x, y, w, h);
			return;
		}
		System.Windows.Shapes.Rectangle.BuildRoundedRectPath(cr, x, y, w, h, rx, ry);
	}
}
