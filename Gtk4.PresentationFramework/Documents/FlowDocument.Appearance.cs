using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Background, font and line DPs. <see cref="Background"/>, <see cref="FontStyle"/> and <see cref="FontWeight"/>
/// are rendered by FlowDocumentScrollViewer (and FlowDocumentReader); FontStretch, TextEffects, LineHeight and
/// LineStackingStrategy are stored.
/// </summary>
public partial class FlowDocument
{
	/// <summary>Identifies the Background property (default null).</summary>
	public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
		nameof(Background), typeof(Brush), typeof(FlowDocument), new FrameworkPropertyMetadata(null));

	/// <summary>Identifies the FontStyle property (shared with TextElement; default Normal).</summary>
	public static readonly DependencyProperty FontStyleProperty =
		TextElement.FontStyleProperty.AddOwner(typeof(FlowDocument));

	/// <summary>Identifies the FontWeight property (shared with TextElement; default Normal).</summary>
	public static readonly DependencyProperty FontWeightProperty =
		TextElement.FontWeightProperty.AddOwner(typeof(FlowDocument));

	/// <summary>Identifies the FontStretch property (shared with TextElement; default Normal).</summary>
	public static readonly DependencyProperty FontStretchProperty =
		TextElement.FontStretchProperty.AddOwner(typeof(FlowDocument));

	/// <summary>Identifies the TextEffects property (default null).</summary>
	public static readonly DependencyProperty TextEffectsProperty = DependencyProperty.Register(
		nameof(TextEffects), typeof(TextEffectCollection), typeof(FlowDocument), new FrameworkPropertyMetadata(null));

	/// <summary>Identifies the LineHeight property (default NaN; otherwise 0.0034 to 160000).</summary>
	public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register(
		nameof(LineHeight), typeof(double), typeof(FlowDocument),
		new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure),
		v => v is double d && (double.IsNaN(d) || (d >= 0.0034 && d <= 160000)));

	/// <summary>Identifies the LineStackingStrategy property (default MaxHeight).</summary>
	public static readonly DependencyProperty LineStackingStrategyProperty = DependencyProperty.Register(
		nameof(LineStackingStrategy), typeof(LineStackingStrategy), typeof(FlowDocument),
		new FrameworkPropertyMetadata(LineStackingStrategy.MaxHeight, FrameworkPropertyMetadataOptions.AffectsMeasure),
		v => v is LineStackingStrategy s
			&& s >= LineStackingStrategy.MaxHeight && s <= LineStackingStrategy.BlockLineHeight);

	/// <summary>Gets or sets the document background.</summary>
	public Brush? Background
	{
		get => (Brush?)GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}

	/// <summary>Gets or sets the font style.</summary>
	public FontStyle FontStyle
	{
		get => (FontStyle)GetValue(FontStyleProperty)!;
		set => SetValue(FontStyleProperty, value);
	}

	/// <summary>Gets or sets the font weight.</summary>
	public FontWeight FontWeight
	{
		get => (FontWeight)GetValue(FontWeightProperty)!;
		set => SetValue(FontWeightProperty, value);
	}

	/// <summary>Gets or sets the font stretch.</summary>
	public FontStretch FontStretch
	{
		get => (FontStretch)GetValue(FontStretchProperty)!;
		set => SetValue(FontStretchProperty, value);
	}

	/// <summary>Gets or sets the text effects (stored).</summary>
	public TextEffectCollection? TextEffects
	{
		get => (TextEffectCollection?)GetValue(TextEffectsProperty);
		set => SetValue(TextEffectsProperty, value);
	}

	/// <summary>Gets or sets the line height (NaN = from the font).</summary>
	public double LineHeight
	{
		get => (double)GetValue(LineHeightProperty)!;
		set => SetValue(LineHeightProperty, value);
	}

	/// <summary>Gets or sets how line height is determined.</summary>
	public LineStackingStrategy LineStackingStrategy
	{
		get => (LineStackingStrategy)GetValue(LineStackingStrategyProperty)!;
		set => SetValue(LineStackingStrategyProperty, value);
	}
}
