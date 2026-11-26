using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// <see cref="TextBlock"/>'s text effects. Stored only: the Pango label cannot transform single glyphs, so
/// the effects (and their animated transforms) never change the rendering.
/// </summary>
public partial class TextBlock
{
	/// <summary>Identifies the <see cref="TextEffects"/> dependency property (same identity as TextElement's).</summary>
	public static readonly DependencyProperty TextEffectsProperty =
		System.Windows.Documents.TextElement.TextEffectsProperty.AddOwner(typeof(TextBlock),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>
	/// Gets or sets the effects on the text. A plain property read: reading never creates a local value, so a
	/// style or trigger setter still applies. Deviation from WPF: the default is null, not an empty frozen
	/// collection, so code that adds effects creates the collection first (the markup compiler emits that).
	/// </summary>
	public TextEffectCollection? TextEffects
	{
		get => (TextEffectCollection?)GetValue(TextEffectsProperty);
		set => SetValue(TextEffectsProperty, value);
	}
}
