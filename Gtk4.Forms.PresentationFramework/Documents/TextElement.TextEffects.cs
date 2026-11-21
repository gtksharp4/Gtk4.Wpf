using System.Windows.Media;

namespace System.Windows.Documents;

public abstract partial class TextElement
{
	/// <summary>Identifies the TextEffects property (default null).</summary>
	public static readonly DependencyProperty TextEffectsProperty = DependencyProperty.Register(
		nameof(TextEffects), typeof(TextEffectCollection), typeof(TextElement),
		new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>Gets or sets the effects on this element's text. Stored only; the label ignores them.</summary>
	public TextEffectCollection? TextEffects
	{
		get => (TextEffectCollection?)GetValue(TextEffectsProperty);
		set => SetValue(TextEffectsProperty, value);
	}
}
