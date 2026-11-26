using System.Windows.Media;

namespace System.Windows.Shapes;

/// <summary>
/// Draws a straight line between two points.
/// Mirrors <c>System.Windows.Shapes.Line</c>.
/// </summary>
public class Line : Shape
{
	const FrameworkPropertyMetadataOptions MeasureAndRender =
		FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender;

	static bool IsDoubleFinite(object? value) => value is double d && !double.IsNaN(d) && !double.IsInfinity(d);

	static void OnEndpointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((Line)d).OnMeasureAffected();

	/// <summary>Identifies <see cref="X1"/>.</summary>
	public static readonly DependencyProperty X1Property = DependencyProperty.Register(
		nameof(X1), typeof(double), typeof(Line),
		new FrameworkPropertyMetadata(0.0, MeasureAndRender, OnEndpointChanged), IsDoubleFinite);

	/// <summary>X coordinate of the start point.</summary>
	public double X1
	{
		get => (double)GetValue(X1Property)!;
		set => SetValue(X1Property, value);
	}

	/// <summary>Identifies <see cref="Y1"/>.</summary>
	public static readonly DependencyProperty Y1Property = DependencyProperty.Register(
		nameof(Y1), typeof(double), typeof(Line),
		new FrameworkPropertyMetadata(0.0, MeasureAndRender, OnEndpointChanged), IsDoubleFinite);

	/// <summary>Y coordinate of the start point.</summary>
	public double Y1
	{
		get => (double)GetValue(Y1Property)!;
		set => SetValue(Y1Property, value);
	}

	/// <summary>Identifies <see cref="X2"/>.</summary>
	public static readonly DependencyProperty X2Property = DependencyProperty.Register(
		nameof(X2), typeof(double), typeof(Line),
		new FrameworkPropertyMetadata(0.0, MeasureAndRender, OnEndpointChanged), IsDoubleFinite);

	/// <summary>X coordinate of the end point.</summary>
	public double X2
	{
		get => (double)GetValue(X2Property)!;
		set => SetValue(X2Property, value);
	}

	/// <summary>Identifies <see cref="Y2"/>.</summary>
	public static readonly DependencyProperty Y2Property = DependencyProperty.Register(
		nameof(Y2), typeof(double), typeof(Line),
		new FrameworkPropertyMetadata(0.0, MeasureAndRender, OnEndpointChanged), IsDoubleFinite);

	/// <summary>Y coordinate of the end point.</summary>
	public double Y2
	{
		get => (double)GetValue(Y2Property)!;
		set => SetValue(Y2Property, value);
	}

	internal override Geometry? GetDefiningGeometry() =>
		new LineGeometry(new Point(X1, Y1), new Point(X2, Y2));
}
