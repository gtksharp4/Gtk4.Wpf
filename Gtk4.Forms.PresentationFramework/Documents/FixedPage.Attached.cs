namespace System.Windows.Documents;

public sealed partial class FixedPage
{
	/// <summary>Identifies the Left attached property (default NaN).</summary>
	public static readonly DependencyProperty LeftProperty = DependencyProperty.RegisterAttached(
		"Left", typeof(double), typeof(FixedPage), new PropertyMetadata(double.NaN, OnPositionChanged));

	/// <summary>Identifies the Top attached property (default NaN).</summary>
	public static readonly DependencyProperty TopProperty = DependencyProperty.RegisterAttached(
		"Top", typeof(double), typeof(FixedPage), new PropertyMetadata(double.NaN, OnPositionChanged));

	/// <summary>Identifies the Right attached property (default NaN).</summary>
	public static readonly DependencyProperty RightProperty = DependencyProperty.RegisterAttached(
		"Right", typeof(double), typeof(FixedPage), new PropertyMetadata(double.NaN, OnPositionChanged));

	/// <summary>Identifies the Bottom attached property (default NaN).</summary>
	public static readonly DependencyProperty BottomProperty = DependencyProperty.RegisterAttached(
		"Bottom", typeof(double), typeof(FixedPage), new PropertyMetadata(double.NaN, OnPositionChanged));

	/// <summary>Identifies the NavigateUri attached property (default null).</summary>
	public static readonly DependencyProperty NavigateUriProperty = DependencyProperty.RegisterAttached(
		"NavigateUri", typeof(Uri), typeof(FixedPage), new PropertyMetadata(null));

	/// <summary>Gets the distance from the left edge of the page.</summary>
	public static double GetLeft(UIElement element) => GetDouble(element, LeftProperty);

	/// <summary>Sets the distance from the left edge of the page.</summary>
	public static void SetLeft(UIElement element, double length) => SetDouble(element, LeftProperty, length);

	/// <summary>Gets the distance from the top edge of the page.</summary>
	public static double GetTop(UIElement element) => GetDouble(element, TopProperty);

	/// <summary>Sets the distance from the top edge of the page.</summary>
	public static void SetTop(UIElement element, double length) => SetDouble(element, TopProperty, length);

	/// <summary>Gets the distance from the right edge of the page.</summary>
	public static double GetRight(UIElement element) => GetDouble(element, RightProperty);

	/// <summary>Sets the distance from the right edge of the page.</summary>
	public static void SetRight(UIElement element, double length) => SetDouble(element, RightProperty, length);

	/// <summary>Gets the distance from the bottom edge of the page.</summary>
	public static double GetBottom(UIElement element) => GetDouble(element, BottomProperty);

	/// <summary>Sets the distance from the bottom edge of the page.</summary>
	public static void SetBottom(UIElement element, double length) => SetDouble(element, BottomProperty, length);

	/// <summary>Gets the URI a click on <paramref name="element"/> navigates to.</summary>
	public static Uri? GetNavigateUri(UIElement element)
	{
		if (element == null)
			throw new ArgumentNullException(nameof(element));
		return (Uri?)element.GetValue(NavigateUriProperty);
	}

	/// <summary>Sets the URI a click on <paramref name="element"/> navigates to (stored; no navigation).</summary>
	public static void SetNavigateUri(UIElement element, Uri? uri)
	{
		if (element == null)
			throw new ArgumentNullException(nameof(element));
		element.SetValue(NavigateUriProperty, uri);
	}

	static double GetDouble(UIElement element, DependencyProperty dp)
	{
		if (element == null)
			throw new ArgumentNullException(nameof(element));
		return (double)element.GetValue(dp)!;
	}

	static void SetDouble(UIElement element, DependencyProperty dp, double value)
	{
		if (element == null)
			throw new ArgumentNullException(nameof(element));
		element.SetValue(dp, value);
	}

	static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement child && child.LogicalParent is FixedPage page && page._fixed != null)
		{
			var (x, y) = Position(child);
			page._fixed.Move(child.Widget, x, y);
		}
	}
}
