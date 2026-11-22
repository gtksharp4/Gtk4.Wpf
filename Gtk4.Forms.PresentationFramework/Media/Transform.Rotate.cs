namespace System.Windows.Media;

/// <summary>
/// Rotates content by <see cref="Angle"/> degrees around a centre point.
/// Mirrors <c>System.Windows.Media.RotateTransform</c>.
/// </summary>
public class RotateTransform : Transform
{
	/// <summary>Creates a transform.</summary>
	public RotateTransform(double angle = 0.0, double centerX = 0.0, double centerY = 0.0)
	{
		Angle = angle;
		CenterX = centerX;
		CenterY = centerY;
	}

	/// <summary>Identifies <see cref="Angle"/>.</summary>
	public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(
		nameof(Angle), typeof(double), typeof(RotateTransform), new PropertyMetadata(0.0));

	/// <summary>Rotation angle in degrees (clockwise).</summary>
	public double Angle
	{
		get => (double)GetValue(AngleProperty)!;
		set => SetValue(AngleProperty, value);
	}

	/// <summary>Identifies <see cref="CenterX"/>.</summary>
	public static readonly DependencyProperty CenterXProperty = DependencyProperty.Register(
		nameof(CenterX), typeof(double), typeof(RotateTransform), new PropertyMetadata(0.0));

	/// <summary>Centre X of rotation.</summary>
	public double CenterX
	{
		get => (double)GetValue(CenterXProperty)!;
		set => SetValue(CenterXProperty, value);
	}

	/// <summary>Identifies <see cref="CenterY"/>.</summary>
	public static readonly DependencyProperty CenterYProperty = DependencyProperty.Register(
		nameof(CenterY), typeof(double), typeof(RotateTransform), new PropertyMetadata(0.0));

	/// <summary>Centre Y of rotation.</summary>
	public double CenterY
	{
		get => (double)GetValue(CenterYProperty)!;
		set => SetValue(CenterYProperty, value);
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new RotateTransform();

	/// <inheritdoc/>
	public override Matrix Value
	{
		get
		{
			double rad = Angle * Math.PI / 180.0;
			double cos = Math.Cos(rad), sin = Math.Sin(rad);
			return new Matrix(cos, sin, -sin, cos,
				CenterX * (1 - cos) + CenterY * sin,
				CenterY * (1 - cos) - CenterX * sin);
		}
	}
}
