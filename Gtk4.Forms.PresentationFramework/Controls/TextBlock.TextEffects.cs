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
	/// Gets or sets the effects on the text. The getter never returns null: like WPF's default value factory it
	/// stores an empty collection on first read, so an implicit collection add in markup works.
	/// </summary>
	public TextEffectCollection TextEffects
	{
		get
		{
			var effects = (TextEffectCollection?)GetValue(TextEffectsProperty);
			if (effects == null)
			{
				effects = new TextEffectCollection();
				SetValue(TextEffectsProperty, effects);
			}
			return effects;
		}
		set => SetValue(TextEffectsProperty, value);
	}
}
