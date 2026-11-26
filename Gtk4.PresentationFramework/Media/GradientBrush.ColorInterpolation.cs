namespace System.Windows.Media;

/// <summary>How gradient colours are interpolated.</summary>
public enum ColorInterpolationMode
{
	ScRgbLinearInterpolation = 0,
	SRgbLinearInterpolation = 1,
}

public abstract partial class GradientBrush
{
	/// <summary>Identifies <see cref="ColorInterpolationMode"/>.</summary>
	public static readonly DependencyProperty ColorInterpolationModeProperty = DependencyProperty.Register(
		nameof(ColorInterpolationMode), typeof(ColorInterpolationMode), typeof(GradientBrush),
		new PropertyMetadata(ColorInterpolationMode.SRgbLinearInterpolation));

	/// <summary>Colour space used between stops. Stored only: Cairo always interpolates in sRGB.</summary>
	public ColorInterpolationMode ColorInterpolationMode
	{
		get => (ColorInterpolationMode)GetValue(ColorInterpolationModeProperty)!;
		set => SetValue(ColorInterpolationModeProperty, value);
	}
}
