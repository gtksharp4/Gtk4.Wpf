namespace System.Windows.Documents;

public static partial class Typography
{
	static object Value(DependencyObject element, DependencyProperty p)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return element.GetValue(p)!;
	}

	static void Value(DependencyObject element, DependencyProperty p, object value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(p, value);
	}

	/// <summary>Identifies the <c>Typography.ContextualLigatures</c> attached property (default true; inherits).</summary>
	public static readonly DependencyProperty ContextualLigaturesProperty = DependencyProperty.RegisterAttached(
		"ContextualLigatures", typeof(bool), typeof(Typography), new FrameworkPropertyMetadata(true, true));

	/// <summary>Gets <c>Typography.ContextualLigatures</c> on <paramref name="element"/>.</summary>
	public static bool GetContextualLigatures(DependencyObject element) =>
		(bool)Value(element, ContextualLigaturesProperty);

	/// <summary>Sets <c>Typography.ContextualLigatures</c> on <paramref name="element"/>.</summary>
	public static void SetContextualLigatures(DependencyObject element, bool value) =>
		Value(element, ContextualLigaturesProperty, value);

	/// <summary>Identifies the <c>Typography.DiscretionaryLigatures</c> attached property (default false; inherits).</summary>
	public static readonly DependencyProperty DiscretionaryLigaturesProperty = DependencyProperty.RegisterAttached(
		"DiscretionaryLigatures", typeof(bool), typeof(Typography), new FrameworkPropertyMetadata(false, true));

	/// <summary>Gets <c>Typography.DiscretionaryLigatures</c> on <paramref name="element"/>.</summary>
	public static bool GetDiscretionaryLigatures(DependencyObject element) =>
		(bool)Value(element, DiscretionaryLigaturesProperty);

	/// <summary>Sets <c>Typography.DiscretionaryLigatures</c> on <paramref name="element"/>.</summary>
	public static void SetDiscretionaryLigatures(DependencyObject element, bool value) =>
		Value(element, DiscretionaryLigaturesProperty, value);

	/// <summary>Identifies the <c>Typography.HistoricalLigatures</c> attached property (default false; inherits).</summary>
	public static readonly DependencyProperty HistoricalLigaturesProperty = DependencyProperty.RegisterAttached(
		"HistoricalLigatures", typeof(bool), typeof(Typography), new FrameworkPropertyMetadata(false, true));

	/// <summary>Gets <c>Typography.HistoricalLigatures</c> on <paramref name="element"/>.</summary>
	public static bool GetHistoricalLigatures(DependencyObject element) =>
		(bool)Value(element, HistoricalLigaturesProperty);

	/// <summary>Sets <c>Typography.HistoricalLigatures</c> on <paramref name="element"/>.</summary>
	public static void SetHistoricalLigatures(DependencyObject element, bool value) =>
		Value(element, HistoricalLigaturesProperty, value);

	/// <summary>Identifies the <c>Typography.ContextualAlternates</c> attached property (default true; inherits).</summary>
	public static readonly DependencyProperty ContextualAlternatesProperty = DependencyProperty.RegisterAttached(
		"ContextualAlternates", typeof(bool), typeof(Typography), new FrameworkPropertyMetadata(true, true));

	/// <summary>Gets <c>Typography.ContextualAlternates</c> on <paramref name="element"/>.</summary>
	public static bool GetContextualAlternates(DependencyObject element) =>
		(bool)Value(element, ContextualAlternatesProperty);

	/// <summary>Sets <c>Typography.ContextualAlternates</c> on <paramref name="element"/>.</summary>
	public static void SetContextualAlternates(DependencyObject element, bool value) =>
		Value(element, ContextualAlternatesProperty, value);

}
