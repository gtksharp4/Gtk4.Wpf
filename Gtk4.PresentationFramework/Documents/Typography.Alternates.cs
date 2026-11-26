namespace System.Windows.Documents;

public static partial class Typography
{
	static bool NonNegative(object? v) => v is int i && i >= 0;

	/// <summary>Identifies the <c>Typography.StandardSwashes</c> attached property (default 0; inherits).</summary>
	public static readonly DependencyProperty StandardSwashesProperty = DependencyProperty.RegisterAttached(
		"StandardSwashes", typeof(int), typeof(Typography), new FrameworkPropertyMetadata(0, true), NonNegative);

	/// <summary>Gets <c>Typography.StandardSwashes</c> on <paramref name="element"/>.</summary>
	public static int GetStandardSwashes(DependencyObject element) =>
		(int)Value(element, StandardSwashesProperty);

	/// <summary>Sets <c>Typography.StandardSwashes</c> on <paramref name="element"/>.</summary>
	public static void SetStandardSwashes(DependencyObject element, int value) =>
		Value(element, StandardSwashesProperty, value);

	/// <summary>Identifies the <c>Typography.ContextualSwashes</c> attached property (default 0; inherits).</summary>
	public static readonly DependencyProperty ContextualSwashesProperty = DependencyProperty.RegisterAttached(
		"ContextualSwashes", typeof(int), typeof(Typography), new FrameworkPropertyMetadata(0, true), NonNegative);

	/// <summary>Gets <c>Typography.ContextualSwashes</c> on <paramref name="element"/>.</summary>
	public static int GetContextualSwashes(DependencyObject element) =>
		(int)Value(element, ContextualSwashesProperty);

	/// <summary>Sets <c>Typography.ContextualSwashes</c> on <paramref name="element"/>.</summary>
	public static void SetContextualSwashes(DependencyObject element, int value) =>
		Value(element, ContextualSwashesProperty, value);

	/// <summary>Identifies the <c>Typography.StylisticAlternates</c> attached property (default 0; inherits).</summary>
	public static readonly DependencyProperty StylisticAlternatesProperty = DependencyProperty.RegisterAttached(
		"StylisticAlternates", typeof(int), typeof(Typography), new FrameworkPropertyMetadata(0, true), NonNegative);

	/// <summary>Gets <c>Typography.StylisticAlternates</c> on <paramref name="element"/>.</summary>
	public static int GetStylisticAlternates(DependencyObject element) =>
		(int)Value(element, StylisticAlternatesProperty);

	/// <summary>Sets <c>Typography.StylisticAlternates</c> on <paramref name="element"/>.</summary>
	public static void SetStylisticAlternates(DependencyObject element, int value) =>
		Value(element, StylisticAlternatesProperty, value);

	/// <summary>Identifies the <c>Typography.AnnotationAlternates</c> attached property (default 0; inherits).</summary>
	public static readonly DependencyProperty AnnotationAlternatesProperty = DependencyProperty.RegisterAttached(
		"AnnotationAlternates", typeof(int), typeof(Typography), new FrameworkPropertyMetadata(0, true), NonNegative);

	/// <summary>Gets <c>Typography.AnnotationAlternates</c> on <paramref name="element"/>.</summary>
	public static int GetAnnotationAlternates(DependencyObject element) =>
		(int)Value(element, AnnotationAlternatesProperty);

	/// <summary>Sets <c>Typography.AnnotationAlternates</c> on <paramref name="element"/>.</summary>
	public static void SetAnnotationAlternates(DependencyObject element, int value) =>
		Value(element, AnnotationAlternatesProperty, value);

}
