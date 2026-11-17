namespace System.Windows;

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