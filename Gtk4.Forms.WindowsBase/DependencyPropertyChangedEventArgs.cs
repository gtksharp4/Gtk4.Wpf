namespace System.Windows;

/// <summary>
/// Handler for CLR events that report a dependency-property change
/// (e.g. <c>UIElement.IsMouseDirectlyOverChanged</c>).
/// </summary>
public delegate void DependencyPropertyChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e);

/// <summary>
/// Provides data for a <see cref="PropertyChangedCallback"/> and
/// <see cref="DependencyObject.OnPropertyChanged"/>.
/// </summary>
public readonly struct DependencyPropertyChangedEventArgs
{
	/// <summary>The property whose effective value changed.</summary>
	public DependencyProperty Property { get; }

	/// <summary>The effective value before the change.</summary>
	public object? OldValue { get; }

	/// <summary>The effective value after the change.</summary>
	public object? NewValue { get; }

	/// <summary>Creates an instance.</summary>
	public DependencyPropertyChangedEventArgs(
		DependencyProperty property,
		object? oldValue,
		object? newValue)
	{
		Property = property;
		OldValue = oldValue;
		NewValue = newValue;
	}
}