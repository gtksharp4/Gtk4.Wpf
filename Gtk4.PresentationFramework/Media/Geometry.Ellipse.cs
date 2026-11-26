using System;

namespace System.Windows.Media;

/// <summary>
/// An ellipse (or circle) geometry.
/// Mirrors <c>System.Windows.Media.EllipseGeometry</c>.
/// </summary>
public class EllipseGeometry : Geometry
{
	/// <summary>Creates a default ellipse.</summary>
	public EllipseGeometry() { }

	/// <summary>Creates an ellipse centred at <paramref name="center"/> with the given radii.</summary>
	public EllipseGeometry(System.Windows.Point center, double radiusX, double radiusY)
	{
		Center = center; RadiusX = radiusX; RadiusY = radiusY;
	}

	/// <summary>Identifies <see cref="Center"/>.</summary>
	public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
		nameof(Center), typeof(System.Windows.Point), typeof(EllipseGeometry), new PropertyMetadata(new System.Windows.Point()));

	/// <summary>Centre point of the ellipse.</summary>
	public System.Windows.Point Center
	{
		get => (System.Windows.Point)GetValue(CenterProperty)!;
		set => SetValue(CenterProperty, value);
	}

	/// <summary>Identifies <see cref="RadiusX"/>.</summary>
	public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(
		nameof(RadiusX), typeof(double), typeof(EllipseGeometry), new PropertyMetadata(0.0));

	/// <summary>Horizontal radius.</summary>
	public double RadiusX
	{
		get => (double)GetValue(RadiusXProperty)!;
		set => SetValue(RadiusXProperty, value);
	}

	/// <summary>Identifies <see cref="RadiusY"/>.</summary>
	public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(
		nameof(RadiusY), typeof(double), typeof(EllipseGeometry), new PropertyMetadata(0.0));

	/// <summary>Vertical radius.</summary>
	public double RadiusY
	{
		get => (double)GetValue(RadiusYProperty)!;
		set => SetValue(RadiusYProperty, value);
	}

	/// <inheritdoc/>
	public override System.Windows.Rect Bounds => new System.Windows.Rect(
		Center.X - RadiusX, Center.Y - RadiusY, RadiusX * 2, RadiusY * 2);

	/// <inheritdoc/>
	internal override void BuildPath(Cairo.Context cr)
	{
		cr.Save();
		cr.Translate(Center.X, Center.Y);
		if (RadiusX > 0 && RadiusY > 0)
			cr.Scale(RadiusX, RadiusY);
		cr.NewSubPath();
		cr.Arc(0, 0, 1, 0, 2 * Math.PI);
		cr.Restore();
	}
}
