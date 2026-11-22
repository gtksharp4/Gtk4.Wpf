using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media.Animation;

/// <summary>A cubic Bezier easing curve from (0,0) to (1,1). Mirrors WPF's <c>KeySpline</c>.</summary>
[TypeConverter(typeof(KeySplineConverter))]
public class KeySpline : Freezable
{
	/// <summary>Identifies <see cref="ControlPoint1"/>.</summary>
	public static readonly DependencyProperty ControlPoint1Property = DependencyProperty.Register(
		nameof(ControlPoint1), typeof(Point), typeof(KeySpline), new PropertyMetadata(new Point(0, 0)));

	/// <summary>Identifies <see cref="ControlPoint2"/>.</summary>
	public static readonly DependencyProperty ControlPoint2Property = DependencyProperty.Register(
		nameof(ControlPoint2), typeof(Point), typeof(KeySpline), new PropertyMetadata(new Point(1, 1)));

	/// <summary>Creates the linear spline.</summary>
	public KeySpline() { }

	/// <summary>Creates a spline from the two control points.</summary>
	public KeySpline(Point controlPoint1, Point controlPoint2)
	{
		ControlPoint1 = controlPoint1;
		ControlPoint2 = controlPoint2;
	}

	/// <summary>Creates a spline from control point coordinates.</summary>
	public KeySpline(double x1, double y1, double x2, double y2)
		: this(new Point(x1, y1), new Point(x2, y2)) { }

	/// <summary>The first control point; each coordinate must be in [0, 1].</summary>
	public Point ControlPoint1
	{
		get => (Point)GetValue(ControlPoint1Property)!;
		set => SetValue(ControlPoint1Property, Check(value));
	}

	/// <summary>The second control point; each coordinate must be in [0, 1].</summary>
	public Point ControlPoint2
	{
		get => (Point)GetValue(ControlPoint2Property)!;
		set => SetValue(ControlPoint2Property, Check(value));
	}

	static Point Check(Point p)
	{
		if (p.X < 0 || p.X > 1 || p.Y < 0 || p.Y > 1)
			throw new ArgumentException("KeySpline control point coordinates must be in [0, 1].");
		return p;
	}

	/// <summary>The output progress for a linear progress in [0, 1].</summary>
	public double GetSplineProgress(double linearProgress)
	{
		if (linearProgress <= 0) return 0;
		if (linearProgress >= 1) return 1;
		Point p1 = ControlPoint1, p2 = ControlPoint2;
		if (p1.X == p1.Y && p2.X == p2.Y) return linearProgress;

		double t = linearProgress;
		for (int i = 0; i < 8; i++)
		{
			double x = Bezier(t, p1.X, p2.X) - linearProgress;
			if (Math.Abs(x) < 1e-7) return Bezier(t, p1.Y, p2.Y);
			double d = Derivative(t, p1.X, p2.X);
			if (Math.Abs(d) < 1e-6) break;
			t -= x / d;
		}
		double lo = 0, hi = 1;
		t = linearProgress;
		for (int i = 0; i < 60; i++)
		{
			double x = Bezier(t, p1.X, p2.X);
			if (Math.Abs(x - linearProgress) < 1e-9) break;
			if (x < linearProgress) lo = t;
			else hi = t;
			t = (lo + hi) / 2;
		}
		return Bezier(t, p1.Y, p2.Y);
	}

	static double Bezier(double t, double a, double b)
	{
		double u = 1 - t;
		return 3 * u * u * t * a + 3 * u * t * t * b + t * t * t;
	}

	static double Derivative(double t, double a, double b)
	{
		double u = 1 - t;
		return 3 * u * u * a + 6 * u * t * (b - a) + 3 * t * t * (1 - b);
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new KeySpline Clone() => (KeySpline)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new KeySpline();

	/// <inheritdoc/>
	public override string ToString() => ControlPoint1.ToString(CultureInfo.InvariantCulture) + " "
		+ ControlPoint2.ToString(CultureInfo.InvariantCulture);
}

/// <summary>Converts "x1,y1 x2,y2" to <see cref="KeySpline"/>.</summary>
public class KeySplineConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (!(value is string text))
			throw new NotSupportedException("Cannot convert from " + (value?.GetType().FullName ?? "(null)") + ".");
		var reader = new TokenizerHelper.Reader(text);
		var spline = new KeySpline(reader.NextDouble(), reader.NextDouble(), reader.NextDouble(), reader.NextDouble());
		reader.LastRequired();
		return spline;
	}

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is KeySpline k) return k.ToString();
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
