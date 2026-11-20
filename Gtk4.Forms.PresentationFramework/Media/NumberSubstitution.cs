using System.Globalization;

namespace System.Windows.Media;

/// <summary>Where digit substitution takes its culture from.</summary>
public enum NumberCultureSource
{
	/// <summary>The language of the text.</summary>
	Text,
	/// <summary>The current user culture.</summary>
	User,
	/// <summary>The <c>CultureOverride</c> value.</summary>
	Override,
}

/// <summary>How digits are substituted.</summary>
public enum NumberSubstitutionMethod
{
	/// <summary>Per the culture default.</summary>
	AsCulture,
	/// <summary>By context.</summary>
	Context,
	/// <summary>European digits.</summary>
	European,
	/// <summary>Native national digits.</summary>
	NativeNational,
	/// <summary>Traditional digits.</summary>
	Traditional,
}

/// <summary>
/// Digit substitution settings. GTK gap: Pango has no digit-substitution control, so these are stored only
/// (not inherited, as in WPF) and text keeps the digits it was given.
/// </summary>
public sealed class NumberSubstitution
{
	/// <summary>Identifies CultureSource (default Text).</summary>
	public static readonly DependencyProperty CultureSourceProperty = DependencyProperty.RegisterAttached(
		"CultureSource", typeof(NumberCultureSource), typeof(NumberSubstitution),
		new PropertyMetadata(NumberCultureSource.Text));

	/// <summary>Identifies CultureOverride (default null).</summary>
	public static readonly DependencyProperty CultureOverrideProperty = DependencyProperty.RegisterAttached(
		"CultureOverride", typeof(CultureInfo), typeof(NumberSubstitution), new PropertyMetadata(null));

	/// <summary>Identifies Substitution (default AsCulture).</summary>
	public static readonly DependencyProperty SubstitutionProperty = DependencyProperty.RegisterAttached(
		"Substitution", typeof(NumberSubstitutionMethod), typeof(NumberSubstitution),
		new PropertyMetadata(NumberSubstitutionMethod.AsCulture));

	/// <summary>Gets CultureSource.</summary>
	public static NumberCultureSource GetCultureSource(DependencyObject target) =>
		(NumberCultureSource)Check(target).GetValue(CultureSourceProperty)!;

	/// <summary>Sets CultureSource.</summary>
	public static void SetCultureSource(DependencyObject target, NumberCultureSource value) =>
		Check(target).SetValue(CultureSourceProperty, value);

	/// <summary>Gets CultureOverride.</summary>
	public static CultureInfo? GetCultureOverride(DependencyObject target) =>
		(CultureInfo?)Check(target).GetValue(CultureOverrideProperty);

	/// <summary>Sets CultureOverride.</summary>
	public static void SetCultureOverride(DependencyObject target, CultureInfo? value) =>
		Check(target).SetValue(CultureOverrideProperty, value);

	/// <summary>Gets Substitution.</summary>
	public static NumberSubstitutionMethod GetSubstitution(DependencyObject target) =>
		(NumberSubstitutionMethod)Check(target).GetValue(SubstitutionProperty)!;

	/// <summary>Sets Substitution.</summary>
	public static void SetSubstitution(DependencyObject target, NumberSubstitutionMethod value) =>
		Check(target).SetValue(SubstitutionProperty, value);

	static DependencyObject Check(DependencyObject target) =>
		target ?? throw new ArgumentNullException(nameof(target));
}
