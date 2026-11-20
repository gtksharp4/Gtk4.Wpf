namespace System.Windows;

/// <summary>Represents the method that handles a routed event that reports a property change.</summary>
/// <typeparam name="T">The type of the changed value.</typeparam>
public delegate void RoutedPropertyChangedEventHandler<T>(object sender, RoutedPropertyChangedEventArgs<T> e);

/// <summary>Event data for routed events that report a property value change.</summary>
/// <typeparam name="T">The type of the changed value.</typeparam>
public class RoutedPropertyChangedEventArgs<T> : RoutedEventArgs
{
	/// <summary>Initializes a new instance with the old and new values.</summary>
	public RoutedPropertyChangedEventArgs(T oldValue, T newValue)
	{
		OldValue = oldValue;
		NewValue = newValue;
	}

	/// <summary>Initializes a new instance with the old and new values and the routed event.</summary>
	public RoutedPropertyChangedEventArgs(T oldValue, T newValue, RoutedEvent routedEvent)
		: base(routedEvent)
	{
		OldValue = oldValue;
		NewValue = newValue;
	}

	/// <summary>Gets the previous value.</summary>
	public T OldValue { get; }

	/// <summary>Gets the new value.</summary>
	public T NewValue { get; }
}
