namespace System.Windows.Controls.Primitives;

/// <summary>
/// Base class for controls that represent a value in a range. Only the value-changed event surface lives here;
/// the range properties (<c>Minimum</c>, <c>Maximum</c>, <c>Value</c>, ...) are declared by each derived control.
/// </summary>
public abstract class RangeBase : Control
{
	/// <summary>Bubble event raised when the value changed.</summary>
	public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
		"ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(RangeBase));

	/// <summary>Occurs when the value changed.</summary>
	public event RoutedPropertyChangedEventHandler<double> ValueChanged
	{
		add => AddHandler(ValueChangedEvent, value);
		remove => RemoveHandler(ValueChangedEvent, value);
	}

	/// <summary>Raises <see cref="ValueChangedEvent"/>; override to react before handlers run.</summary>
	protected virtual void OnValueChanged(double oldValue, double newValue) =>
		RaiseEvent(new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, ValueChangedEvent));
}
