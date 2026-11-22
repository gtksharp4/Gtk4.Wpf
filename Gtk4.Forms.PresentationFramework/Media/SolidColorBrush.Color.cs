namespace System.Windows.Media;

public partial class SolidColorBrush
{
	/// <summary>Identifies <see cref="Color"/>. Default <see cref="Colors.Transparent"/>.</summary>
	public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
		nameof(Color), typeof(Color), typeof(SolidColorBrush), new PropertyMetadata(Colors.Transparent));

	/// <summary>The fill colour.</summary>
	public Color Color
	{
		get => (Color)GetValue(ColorProperty)!;
		set => SetValue(ColorProperty, value);
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new SolidColorBrush();
}
