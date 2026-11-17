namespace System.Windows.Controls;

/// <summary>
/// Provides the <see cref="ToolTip"/> attached property as a service on any
/// <see cref="DependencyObject"/>. Delegates to
/// <see cref="FrameworkElement.ToolTipProperty"/>.
/// </summary>
public static class ToolTipService
{
	/// <summary>
	/// Gets the tooltip set on <paramref name="element"/> via
	/// <see cref="FrameworkElement.ToolTipProperty"/>.
	/// </summary>
	public static object? GetToolTip(DependencyObject element)
		=> FrameworkElement.GetToolTip(element);

	/// <summary>
	/// Sets the tooltip on <paramref name="element"/> via
	/// <see cref="FrameworkElement.ToolTipProperty"/>.
	/// </summary>
	public static void SetToolTip(DependencyObject element, object? value)
		=> FrameworkElement.SetToolTip(element, value);
}