namespace System.Windows.Controls;

public static partial class ToolTipService
{
	/// <summary>
	/// Identifies the ToolTip attached property. <see cref="FrameworkElement.ToolTipProperty"/> is the same
	/// property with <see cref="FrameworkElement"/> added as an owner, which carries the GTK tooltip wiring.
	/// Plain <see cref="PropertyMetadata"/> (WPF: FrameworkPropertyMetadata) because the existing owners use it.
	/// </summary>
	public static readonly DependencyProperty ToolTipProperty =
		DependencyProperty.RegisterAttached("ToolTip", typeof(object), typeof(ToolTipService),
			new PropertyMetadata(null));

	/// <summary>Gets the tooltip set on <paramref name="element"/>.</summary>
	public static object? GetToolTip(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return element.GetValue(ToolTipProperty);
	}

	/// <summary>Sets the tooltip on <paramref name="element"/>.</summary>
	public static void SetToolTip(DependencyObject element, object? value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(ToolTipProperty, value);
	}
}
