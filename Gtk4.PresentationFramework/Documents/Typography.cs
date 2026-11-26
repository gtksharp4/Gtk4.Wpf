namespace System.Windows.Documents;

/// <summary>
/// Provides WPF Typography attached dependency properties that control OpenType
/// font features. Each property maps to one or more Pango <c>font_features</c>
/// tokens (see <see cref="TypographyPango.BuildFontFeatures(DependencyObject)"/>).
/// <para>
/// <b>Pango application:</b> set Typography properties on a
/// <see cref="System.Windows.Controls.TextBlock"/> or any <see cref="DependencyObject"/>.
/// When rendering, <c>TypographyPango.BuildFontFeatures</c> converts the values to a
/// <c>font_features='…'</c> Pango span attribute.
/// </para>
/// </summary>
public static partial class Typography
{
	// ── Capitals ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>Typography.Capitals</c> attached property.
	/// Default: <see cref="FontCapitals.Normal"/>.
	/// </summary>
	public static readonly DependencyProperty CapitalsProperty =
		DependencyProperty.RegisterAttached(
			"Capitals",
			typeof(FontCapitals),
			typeof(Typography),
			new PropertyMetadata(FontCapitals.Normal));

	/// <summary>Gets the <see cref="FontCapitals"/> value on <paramref name="element"/>.</summary>
	public static FontCapitals GetCapitals(DependencyObject element)
		=> (FontCapitals)element.GetValue(CapitalsProperty)!;

	/// <summary>Sets the <see cref="FontCapitals"/> value on <paramref name="element"/>.</summary>
	public static void SetCapitals(DependencyObject element, FontCapitals value)
		=> element.SetValue(CapitalsProperty, value);

	// ── NumeralStyle ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>Typography.NumeralStyle</c> attached property.
	/// Default: <see cref="FontNumeralStyle.Normal"/>.
	/// </summary>
	public static readonly DependencyProperty NumeralStyleProperty =
		DependencyProperty.RegisterAttached(
			"NumeralStyle",
			typeof(FontNumeralStyle),
			typeof(Typography),
			new PropertyMetadata(FontNumeralStyle.Normal));

	/// <summary>Gets the <see cref="FontNumeralStyle"/> value on <paramref name="element"/>.</summary>
	public static FontNumeralStyle GetNumeralStyle(DependencyObject element)
		=> (FontNumeralStyle)element.GetValue(NumeralStyleProperty)!;

	/// <summary>Sets the <see cref="FontNumeralStyle"/> value on <paramref name="element"/>.</summary>
	public static void SetNumeralStyle(DependencyObject element, FontNumeralStyle value)
		=> element.SetValue(NumeralStyleProperty, value);

	// ── Fraction ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>Typography.Fraction</c> attached property.
	/// Default: <see cref="FontFraction.Normal"/>.
	/// </summary>
	public static readonly DependencyProperty FractionProperty =
		DependencyProperty.RegisterAttached(
			"Fraction",
			typeof(FontFraction),
			typeof(Typography),
			new PropertyMetadata(FontFraction.Normal));

	/// <summary>Gets the <see cref="FontFraction"/> value on <paramref name="element"/>.</summary>
	public static FontFraction GetFraction(DependencyObject element)
		=> (FontFraction)element.GetValue(FractionProperty)!;

	/// <summary>Sets the <see cref="FontFraction"/> value on <paramref name="element"/>.</summary>
	public static void SetFraction(DependencyObject element, FontFraction value)
		=> element.SetValue(FractionProperty, value);

	// ── StandardLigatures ────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>Typography.StandardLigatures</c> attached property.
	/// Default: <c>true</c> (ligatures enabled; no <c>liga=0</c> token emitted).
	/// </summary>
	public static readonly DependencyProperty StandardLigaturesProperty =
		DependencyProperty.RegisterAttached(
			"StandardLigatures",
			typeof(bool),
			typeof(Typography),
			new PropertyMetadata(true));

	/// <summary>Gets the <c>StandardLigatures</c> value on <paramref name="element"/>.</summary>
	public static bool GetStandardLigatures(DependencyObject element)
		=> (bool)element.GetValue(StandardLigaturesProperty)!;

	/// <summary>Sets the <c>StandardLigatures</c> value on <paramref name="element"/>.</summary>
	public static void SetStandardLigatures(DependencyObject element, bool value)
		=> element.SetValue(StandardLigaturesProperty, value);

	// ── Kerning ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>Typography.Kerning</c> attached property.
	/// Default: <c>true</c> (kerning enabled; no <c>kern=0</c> token emitted).
	/// </summary>
	public static readonly DependencyProperty KerningProperty =
		DependencyProperty.RegisterAttached(
			"Kerning",
			typeof(bool),
			typeof(Typography),
			new PropertyMetadata(true));

	/// <summary>Gets the <c>Kerning</c> value on <paramref name="element"/>.</summary>
	public static bool GetKerning(DependencyObject element)
		=> (bool)element.GetValue(KerningProperty)!;

	/// <summary>Sets the <c>Kerning</c> value on <paramref name="element"/>.</summary>
	public static void SetKerning(DependencyObject element, bool value)
		=> element.SetValue(KerningProperty, value);

	// ── Variants ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>Typography.Variants</c> attached property.
	/// Default: <see cref="FontVariants.Normal"/>.
	/// </summary>
	public static readonly DependencyProperty VariantsProperty =
		DependencyProperty.RegisterAttached(
			"Variants",
			typeof(FontVariants),
			typeof(Typography),
			new PropertyMetadata(FontVariants.Normal));

	/// <summary>Gets the <see cref="FontVariants"/> value on <paramref name="element"/>.</summary>
	public static FontVariants GetVariants(DependencyObject element)
		=> (FontVariants)element.GetValue(VariantsProperty)!;

	/// <summary>Sets the <see cref="FontVariants"/> value on <paramref name="element"/>.</summary>
	public static void SetVariants(DependencyObject element, FontVariants value)
		=> element.SetValue(VariantsProperty, value);
}
