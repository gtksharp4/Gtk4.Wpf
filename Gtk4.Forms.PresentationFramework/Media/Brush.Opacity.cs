namespace System.Windows.Media;

public abstract partial class Brush
{
	/// <summary>Identifies <see cref="Opacity"/>. Default 1.</summary>
	public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register(
		nameof(Opacity), typeof(double), typeof(Brush), new PropertyMetadata(1.0));

	/// <summary>
	/// Overall opacity multiplier; 0 = fully transparent, 1 = fully opaque.
	/// Applied on top of any per-colour alpha.
	/// </summary>
	public double Opacity
	{
		get => (double)GetValue(OpacityProperty)!;
		set => SetValue(OpacityProperty, value);
	}
}
