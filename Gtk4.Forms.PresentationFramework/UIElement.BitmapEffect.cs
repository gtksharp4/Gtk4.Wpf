using System.Windows.Media.Effects;

namespace System.Windows;

/// <summary>
/// The obsolete legacy <c>BitmapEffect</c>/<c>BitmapEffectInput</c> properties. Stored only; WPF itself no longer
/// renders them, and neither does this layer. Use <see cref="Effect"/>.
/// </summary>
public abstract partial class UIElement
{
	const string BitmapEffectObsolete =
		"BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.";

	/// <summary>Identifies <see cref="BitmapEffect"/>.</summary>
	public static readonly DependencyProperty BitmapEffectProperty = DependencyProperty.Register(
		"BitmapEffect", typeof(BitmapEffect), typeof(UIElement), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="BitmapEffectInput"/>.</summary>
	public static readonly DependencyProperty BitmapEffectInputProperty = DependencyProperty.Register(
		"BitmapEffectInput", typeof(BitmapEffectInput), typeof(UIElement), new PropertyMetadata(null));

	/// <summary>The legacy bitmap effect. Renders nothing.</summary>
	[Obsolete(BitmapEffectObsolete)]
	public BitmapEffect? BitmapEffect
	{
		get => (BitmapEffect?)GetValue(BitmapEffectProperty);
		set => SetValue(BitmapEffectProperty, value);
	}

	/// <summary>The legacy bitmap effect input. Renders nothing.</summary>
	[Obsolete(BitmapEffectObsolete)]
	public BitmapEffectInput? BitmapEffectInput
	{
		get => (BitmapEffectInput?)GetValue(BitmapEffectInputProperty);
		set => SetValue(BitmapEffectInputProperty, value);
	}
}
