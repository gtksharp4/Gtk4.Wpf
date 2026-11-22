namespace System.Windows.Media;

/// <summary>
/// Skews content by <see cref="AngleX"/> and <see cref="AngleY"/> degrees around a centre point.
/// Mirrors <c>System.Windows.Media.SkewTransform</c>.
/// </summary>
public class SkewTransform : Transform
{
	/// <summary>Creates a transform.</summary>
	public SkewTransform(double angleX = 0.0, double angleY = 0.0, double centerX = 0.0, double centerY = 0.0)
	{
		AngleX = angleX;
		AngleY = angleY;
		CenterX = centerX;
		CenterY = centerY;
	}

	/// <summary>Identifies <see cref="AngleX"/>.</summary>
	public static readonly DependencyProperty AngleXProperty = DependencyProperty.Register(
		nameof(AngleX), typeof(double), typeof(SkewTransform), new PropertyMetadata(0.0));

	/// <summary>Horizontal skew angle in degrees.</summary>
	public double AngleX
	{
		get => (double)GetValue(AngleXProperty)!;
		set => SetValue(AngleXProperty, value);
	}

	/// <summary>Identifies <see cref="AngleY"/>.</summary>
	public static readonly DependencyProperty AngleYProperty = DependencyProperty.Register(
		nameof(AngleY), typeof(double), typeof(SkewTransform), new PropertyMetadata(0.0));

	/// <summary>Vertical skew angle in degrees.</summary>
	public double AngleY
	{
		get => (double)GetValue(AngleYProperty)!;
		set => SetValue(AngleYProperty, value);
	}

	/// <summary>Identifies <see cref="CenterX"/>.</summary>
	public static readonly DependencyProperty CenterXProperty = DependencyProperty.Register(
		nameof(CenterX), typeof(double), typeof(SkewTransform), new PropertyMetadata(0.0));

	/// <summary>Centre X of the skew.</summary>
	public double CenterX
	{
		get => (double)GetValue(CenterXProperty)!;
		set => SetValue(CenterXProperty, value);
	}

	/// <summary>Identifies <see cref="CenterY"/>.</summary>
	public static readonly DependencyProperty CenterYProperty = DependencyProperty.Register(
		nameof(CenterY), typeof(double), typeof(SkewTransform), new PropertyMetadata(0.0));

	/// <summary>Centre Y of the skew.</summary>
	public double CenterY
	{
		get => (double)GetValue(CenterYProperty)!;
		set => SetValue(CenterYProperty, value);
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new SkewTransform();

	/// <inheritdoc/>
	public override Matrix Value
	{
		get
		{
			double tx = Math.Tan(AngleX * Math.PI / 180.0);
			double ty = Math.Tan(AngleY * Math.PI / 180.0);
			return new Matrix(1, ty, tx, 1, -CenterY * tx, -CenterX * ty);
		}
	}
}
