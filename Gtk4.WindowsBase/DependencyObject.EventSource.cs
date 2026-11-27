namespace System.Windows;

public partial class DependencyObject
{
	/// <summary>
	/// Lets an element re-target <see cref="RoutedEventArgs.Source"/> while a bubbling or tunnelling route is
	/// built (WPF <c>UIElement.AdjustEventSource</c>). Return the new source, or <c>null</c> for no change; the new
	/// source applies to this element's handlers and to every element further along the route.
	/// </summary>
	internal virtual object? AdjustEventSource(RoutedEventArgs e) => null;
}
