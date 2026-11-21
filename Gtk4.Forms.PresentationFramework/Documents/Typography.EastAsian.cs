namespace System.Windows.Documents;

public static partial class Typography
{
	/// <summary>Identifies the <c>Typography.NumeralAlignment</c> attached property (default Normal; inherits).</summary>
	public static readonly DependencyProperty NumeralAlignmentProperty = DependencyProperty.RegisterAttached(
		"NumeralAlignment", typeof(FontNumeralAlignment), typeof(Typography),
		new FrameworkPropertyMetadata(FontNumeralAlignment.Normal, true),
		v => v != null && Enum.IsDefined(typeof(FontNumeralAlignment), v));

	/// <summary>Gets <c>Typography.NumeralAlignment</c> on <paramref name="element"/>.</summary>
	public static FontNumeralAlignment GetNumeralAlignment(DependencyObject element) =>
		(FontNumeralAlignment)Value(element, NumeralAlignmentProperty);

	/// <summary>Sets <c>Typography.NumeralAlignment</c> on <paramref name="element"/>.</summary>
	public static void SetNumeralAlignment(DependencyObject element, FontNumeralAlignment value) =>
		Value(element, NumeralAlignmentProperty, value);

	/// <summary>Identifies the <c>Typography.EastAsianWidths</c> attached property (default Normal; inherits).</summary>
	public static readonly DependencyProperty EastAsianWidthsProperty = DependencyProperty.RegisterAttached(
		"EastAsianWidths", typeof(FontEastAsianWidths), typeof(Typography),
		new FrameworkPropertyMetadata(FontEastAsianWidths.Normal, true),
		v => v != null && Enum.IsDefined(typeof(FontEastAsianWidths), v));

	/// <summary>Gets <c>Typography.EastAsianWidths</c> on <paramref name="element"/>.</summary>
	public static FontEastAsianWidths GetEastAsianWidths(DependencyObject element) =>
		(FontEastAsianWidths)Value(element, EastAsianWidthsProperty);

	/// <summary>Sets <c>Typography.EastAsianWidths</c> on <paramref name="element"/>.</summary>
	public static void SetEastAsianWidths(DependencyObject element, FontEastAsianWidths value) =>
		Value(element, EastAsianWidthsProperty, value);

	/// <summary>Identifies the <c>Typography.EastAsianLanguage</c> attached property (default Normal; inherits).</summary>
	public static readonly DependencyProperty EastAsianLanguageProperty = DependencyProperty.RegisterAttached(
		"EastAsianLanguage", typeof(FontEastAsianLanguage), typeof(Typography),
		new FrameworkPropertyMetadata(FontEastAsianLanguage.Normal, true),
		v => v != null && Enum.IsDefined(typeof(FontEastAsianLanguage), v));

	/// <summary>Gets <c>Typography.EastAsianLanguage</c> on <paramref name="element"/>.</summary>
	public static FontEastAsianLanguage GetEastAsianLanguage(DependencyObject element) =>
		(FontEastAsianLanguage)Value(element, EastAsianLanguageProperty);

	/// <summary>Sets <c>Typography.EastAsianLanguage</c> on <paramref name="element"/>.</summary>
	public static void SetEastAsianLanguage(DependencyObject element, FontEastAsianLanguage value) =>
		Value(element, EastAsianLanguageProperty, value);

}
