namespace System.Windows.Documents;

public static partial class Typography
{
	/// <summary>Identifies the <c>Typography.HistoricalForms</c> attached property (default false; inherits).</summary>
	public static readonly DependencyProperty HistoricalFormsProperty = DependencyProperty.RegisterAttached(
		"HistoricalForms", typeof(bool), typeof(Typography), new FrameworkPropertyMetadata(false, true));

	/// <summary>Gets <c>Typography.HistoricalForms</c> on <paramref name="element"/>.</summary>
	public static bool GetHistoricalForms(DependencyObject element) =>
		(bool)Value(element, HistoricalFormsProperty);

	/// <summary>Sets <c>Typography.HistoricalForms</c> on <paramref name="element"/>.</summary>
	public static void SetHistoricalForms(DependencyObject element, bool value) =>
		Value(element, HistoricalFormsProperty, value);

	/// <summary>Identifies the <c>Typography.CaseSensitiveForms</c> attached property (default false; inherits).</summary>
	public static readonly DependencyProperty CaseSensitiveFormsProperty = DependencyProperty.RegisterAttached(
		"CaseSensitiveForms", typeof(bool), typeof(Typography), new FrameworkPropertyMetadata(false, true));

	/// <summary>Gets <c>Typography.CaseSensitiveForms</c> on <paramref name="element"/>.</summary>
	public static bool GetCaseSensitiveForms(DependencyObject element) =>
		(bool)Value(element, CaseSensitiveFormsProperty);

	/// <summary>Sets <c>Typography.CaseSensitiveForms</c> on <paramref name="element"/>.</summary>
	public static void SetCaseSensitiveForms(DependencyObject element, bool value) =>
		Value(element, CaseSensitiveFormsProperty, value);

	/// <summary>Identifies the <c>Typography.CapitalSpacing</c> attached property (default false; inherits).</summary>
	public static readonly DependencyProperty CapitalSpacingProperty = DependencyProperty.RegisterAttached(
		"CapitalSpacing", typeof(bool), typeof(Typography), new FrameworkPropertyMetadata(false, true));

	/// <summary>Gets <c>Typography.CapitalSpacing</c> on <paramref name="element"/>.</summary>
	public static bool GetCapitalSpacing(DependencyObject element) =>
		(bool)Value(element, CapitalSpacingProperty);

	/// <summary>Sets <c>Typography.CapitalSpacing</c> on <paramref name="element"/>.</summary>
	public static void SetCapitalSpacing(DependencyObject element, bool value) =>
		Value(element, CapitalSpacingProperty, value);

	/// <summary>Identifies the <c>Typography.SlashedZero</c> attached property (default false; inherits).</summary>
	public static readonly DependencyProperty SlashedZeroProperty = DependencyProperty.RegisterAttached(
		"SlashedZero", typeof(bool), typeof(Typography), new FrameworkPropertyMetadata(false, true));

	/// <summary>Gets <c>Typography.SlashedZero</c> on <paramref name="element"/>.</summary>
	public static bool GetSlashedZero(DependencyObject element) =>
		(bool)Value(element, SlashedZeroProperty);

	/// <summary>Sets <c>Typography.SlashedZero</c> on <paramref name="element"/>.</summary>
	public static void SetSlashedZero(DependencyObject element, bool value) =>
		Value(element, SlashedZeroProperty, value);

	/// <summary>Identifies the <c>Typography.MathematicalGreek</c> attached property (default false; inherits).</summary>
	public static readonly DependencyProperty MathematicalGreekProperty = DependencyProperty.RegisterAttached(
		"MathematicalGreek", typeof(bool), typeof(Typography), new FrameworkPropertyMetadata(false, true));

	/// <summary>Gets <c>Typography.MathematicalGreek</c> on <paramref name="element"/>.</summary>
	public static bool GetMathematicalGreek(DependencyObject element) =>
		(bool)Value(element, MathematicalGreekProperty);

	/// <summary>Sets <c>Typography.MathematicalGreek</c> on <paramref name="element"/>.</summary>
	public static void SetMathematicalGreek(DependencyObject element, bool value) =>
		Value(element, MathematicalGreekProperty, value);

	/// <summary>Identifies the <c>Typography.EastAsianExpertForms</c> attached property (default false; inherits).</summary>
	public static readonly DependencyProperty EastAsianExpertFormsProperty = DependencyProperty.RegisterAttached(
		"EastAsianExpertForms", typeof(bool), typeof(Typography), new FrameworkPropertyMetadata(false, true));

	/// <summary>Gets <c>Typography.EastAsianExpertForms</c> on <paramref name="element"/>.</summary>
	public static bool GetEastAsianExpertForms(DependencyObject element) =>
		(bool)Value(element, EastAsianExpertFormsProperty);

	/// <summary>Sets <c>Typography.EastAsianExpertForms</c> on <paramref name="element"/>.</summary>
	public static void SetEastAsianExpertForms(DependencyObject element, bool value) =>
		Value(element, EastAsianExpertFormsProperty, value);

}
