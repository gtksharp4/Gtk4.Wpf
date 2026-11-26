namespace System.Windows.Controls;

/// <summary>
/// <c>BaselineOffset</c> and <c>LineStackingStrategy</c> on <see cref="TextBlock"/>. Both are
/// <b>store-only</b>: <c>Gtk.Label</c> has no line-box or baseline-offset control in GtkSharp4, so the
/// values round-trip through the DP system but do not change layout.
/// </summary>
public partial class TextBlock
{
	/// <summary>Identifies the <see cref="BaselineOffset"/> property (default NaN = auto).</summary>
	public static readonly DependencyProperty BaselineOffsetProperty = DependencyProperty.RegisterAttached(
		nameof(BaselineOffset), typeof(double), typeof(TextBlock),
		new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the <see cref="LineStackingStrategy"/> property (default MaxHeight; inherits).</summary>
	public static readonly DependencyProperty LineStackingStrategyProperty = DependencyProperty.RegisterAttached(
		nameof(LineStackingStrategy), typeof(LineStackingStrategy), typeof(TextBlock),
		new FrameworkPropertyMetadata(LineStackingStrategy.MaxHeight,
			FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Gets or sets the baseline offset.</summary>
	public double BaselineOffset
	{
		get => (double)GetValue(BaselineOffsetProperty)!;
		set => SetValue(BaselineOffsetProperty, value);
	}

	/// <summary>Gets or sets how line heights are determined.</summary>
	public LineStackingStrategy LineStackingStrategy
	{
		get => (LineStackingStrategy)GetValue(LineStackingStrategyProperty)!;
		set => SetValue(LineStackingStrategyProperty, value);
	}
}
