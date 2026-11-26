namespace System.Windows.Input;

/// <summary>
/// Provides logical-focus tracking independent of keyboard focus.
/// A focus scope (e.g. a <see cref="System.Windows.Window"/>) remembers which element
/// inside it last held focus.
/// </summary>
public static partial class FocusManager
{
	/// <summary>
	/// Attached DP that tracks the focused element within a focus scope.
	/// </summary>
	public static readonly System.Windows.DependencyProperty FocusedElementProperty =
		System.Windows.DependencyProperty.RegisterAttached(
			"FocusedElement",
			typeof(System.Windows.IInputElement),
			typeof(FocusManager),
			new System.Windows.PropertyMetadata(null));

	/// <summary>Returns the element that holds logical focus in <paramref name="element"/>'s scope.</summary>
	public static System.Windows.IInputElement? GetFocusedElement(
		System.Windows.DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (System.Windows.IInputElement?)element.GetValue(FocusedElementProperty);
	}

	/// <summary>Sets the logical focus to <paramref name="value"/> within <paramref name="element"/>'s scope.</summary>
	public static void SetFocusedElement(
		System.Windows.DependencyObject element,
		System.Windows.IInputElement? value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(FocusedElementProperty, value);
	}
}