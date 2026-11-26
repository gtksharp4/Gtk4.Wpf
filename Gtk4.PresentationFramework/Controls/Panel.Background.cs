using System.Windows.Media;

namespace System.Windows.Controls;

public abstract partial class Panel
{
	// Precise init: Control/TextBlock/Border AddOwner this property from their own static initialisers.
	static Panel() { }

	/// <summary>Identifies the <see cref="Background"/> dependency property (null default; does not inherit).</summary>
	public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
		nameof(Background), typeof(Brush), typeof(Panel),
		new FrameworkPropertyMetadata(null, OnAppearanceChanged));

	/// <summary>Gets or sets the brush that fills the panel area.</summary>
	public Brush? Background
	{
		get => (Brush?)GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}

	internal override string BuildAppearanceCss() => ElementCssStyle.Background(this);
}
