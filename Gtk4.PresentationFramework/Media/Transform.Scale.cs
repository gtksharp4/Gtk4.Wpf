namespace System.Windows.Media;

/// <summary>
/// Scales content by <see cref="ScaleX"/> and <see cref="ScaleY"/> around a centre point.
/// Mirrors <c>System.Windows.Media.ScaleTransform</c>.
/// </summary>
public class ScaleTransform : Transform
{
	/// <summary>Creates a transform.</summary>
	public ScaleTransform(double scaleX = 1.0, double scaleY = 1.0, double centerX = 0.0, double centerY = 0.0)
	{
		ScaleX = scaleX;
		ScaleY = scaleY;
		CenterX = centerX;
		CenterY = centerY;
	}

	/// <summary>Identifies <see cref="ScaleX"/>.</summary>
	public static readonly DependencyProperty ScaleXProperty = DependencyProperty.Register(
		nameof(ScaleX), typeof(double), typeof(ScaleTransform), new PropertyMetadata(1.0));

	/// <summary>Horizontal scale factor.</summary>
	public double ScaleX
	{
		get => (double)GetValue(ScaleXProperty)!;
		set => SetValue(ScaleXProperty, value);
	}

	/// <summary>Identifies <see cref="ScaleY"/>.</summary>
	public static readonly DependencyProperty ScaleYProperty = DependencyProperty.Register(
		nameof(ScaleY), typeof(double), typeof(ScaleTransform), new PropertyMetadata(1.0));

	/// <summary>Vertical scale factor.</summary>
	public double ScaleY
	{
		get => (double)GetValue(ScaleYProperty)!;
		set => SetValue(ScaleYProperty, value);
	}

	/// <summary>Identifies <see cref="CenterX"/>.</summary>
	public static readonly DependencyProperty CenterXProperty = DependencyProperty.Register(
		nameof(CenterX), typeof(double), typeof(ScaleTransform), new PropertyMetadata(0.0));

	/// <summary>Centre X of the scale.</summary>
	public double CenterX
	{
		get => (double)GetValue(CenterXProperty)!;
		set => SetValue(CenterXProperty, value);
	}

	/// <summary>Identifies <see cref="CenterY"/>.</summary>
	public static readonly DependencyProperty CenterYProperty = DependencyProperty.Register(
		nameof(CenterY), typeof(double), typeof(ScaleTransform), new PropertyMetadata(0.0));

	/// <summary>Centre Y of the scale.</summary>
	public double CenterY
	{
		get => (double)GetValue(CenterYProperty)!;
		set => SetValue(CenterYProperty, value);
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new ScaleTransform();

	/// <inheritdoc/>
	public override Matrix Value
	{
		get => new Matrix(ScaleX, 0, 0, ScaleY, CenterX * (1 - ScaleX), CenterY * (1 - ScaleY));
	}
}
