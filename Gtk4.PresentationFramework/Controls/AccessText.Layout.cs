using System.Windows.Documents;

namespace System.Windows.Controls;

/// <summary>
/// Layout properties of <see cref="AccessText"/>. <c>TextDecorations</c>, <c>LineHeight</c> and
/// <c>BaselineOffset</c> are store-only, as on <see cref="TextBlock"/>: a GTK label has no per-label
/// decoration, line-box or baseline-offset control in this binding.
/// </summary>
public partial class AccessText
{
	/// <summary>Identifies the <see cref="TextWrapping"/> dependency property (default NoWrap).</summary>
	public static readonly DependencyProperty TextWrappingProperty = TextBlock.TextWrappingProperty.AddOwner(
		typeof(AccessText),
		new FrameworkPropertyMetadata(TextWrapping.NoWrap, FrameworkPropertyMetadataOptions.AffectsMeasure,
			(d, _) => ((AccessText)d).ApplyLayout()));

	/// <summary>Identifies the <see cref="TextTrimming"/> dependency property (default None).</summary>
	public static readonly DependencyProperty TextTrimmingProperty = TextBlock.TextTrimmingProperty.AddOwner(
		typeof(AccessText),
		new FrameworkPropertyMetadata(TextTrimming.None, FrameworkPropertyMetadataOptions.AffectsMeasure,
			(d, _) => ((AccessText)d).ApplyLayout()));

	/// <summary>Identifies the <see cref="TextAlignment"/> dependency property (default Left).</summary>
	public static readonly DependencyProperty TextAlignmentProperty = TextBlock.TextAlignmentProperty.AddOwner(
		typeof(AccessText),
		new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.AffectsMeasure,
			(d, _) => ((AccessText)d).ApplyLayout()));

	/// <summary>Identifies the <see cref="TextDecorations"/> dependency property. Store-only.</summary>
	public static readonly DependencyProperty TextDecorationsProperty =
		TextBlock.TextDecorationsProperty.AddOwner(typeof(AccessText));

	/// <summary>Identifies the <see cref="LineHeight"/> dependency property. Store-only.</summary>
	public static readonly DependencyProperty LineHeightProperty =
		TextBlock.LineHeightProperty.AddOwner(typeof(AccessText));

	/// <summary>Identifies the <see cref="BaselineOffset"/> dependency property. Store-only.</summary>
	public static readonly DependencyProperty BaselineOffsetProperty =
		TextBlock.BaselineOffsetProperty.AddOwner(typeof(AccessText));

	/// <summary>Gets or sets how text wraps.</summary>
	public TextWrapping TextWrapping
	{
		get => (TextWrapping)GetValue(TextWrappingProperty)!;
		set => SetValue(TextWrappingProperty, value);
	}

	/// <summary>Gets or sets how overflowing text is trimmed.</summary>
	public TextTrimming TextTrimming
	{
		get => (TextTrimming)GetValue(TextTrimmingProperty)!;
		set => SetValue(TextTrimmingProperty, value);
	}

	/// <summary>Gets or sets the horizontal text alignment.</summary>
	public TextAlignment TextAlignment
	{
		get => (TextAlignment)GetValue(TextAlignmentProperty)!;
		set => SetValue(TextAlignmentProperty, value);
	}

	/// <summary>Gets or sets the text decorations. Stored only.</summary>
	public TextDecorationCollection? TextDecorations
	{
		get => (TextDecorationCollection?)GetValue(TextDecorationsProperty);
		set => SetValue(TextDecorationsProperty, value);
	}

	/// <summary>Gets or sets the line height. Stored only.</summary>
	public double LineHeight
	{
		get => (double)GetValue(LineHeightProperty)!;
		set => SetValue(LineHeightProperty, value);
	}

	/// <summary>Gets or sets the baseline offset. Stored only.</summary>
	public double BaselineOffset
	{
		get => (double)GetValue(BaselineOffsetProperty)!;
		set => SetValue(BaselineOffsetProperty, value);
	}

	void ApplyLayout()
	{
		if (_label == null) return;
		var (wrap, mode) = TextWrappingMap.ToLabel(TextWrapping);
		_label.Wrap = wrap;
		_label.WrapMode = mode;
		_label.Ellipsize = TextBlock.MapTextTrimming(TextTrimming);
		var (justify, xalign) = TextBlock.MapTextAlignment(TextAlignment);
		_label.Justify = justify;
		_label.Xalign = xalign;
	}
}
