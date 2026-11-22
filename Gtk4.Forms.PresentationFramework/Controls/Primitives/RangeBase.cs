namespace System.Windows.Controls.Primitives;

/// <summary>
/// Base class for controls that represent a value in a range (<see cref="Slider"/>, <see cref="ProgressBar"/>).
/// The range properties live in <c>RangeBase.Properties.cs</c>.
/// </summary>
public abstract partial class RangeBase : Control
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

	/// <summary>Called when <see cref="Minimum"/> changed.</summary>
	protected virtual void OnMinimumChanged(double oldMinimum, double newMinimum)
	{
	}

	/// <summary>Called when <see cref="Maximum"/> changed.</summary>
	protected virtual void OnMaximumChanged(double oldMaximum, double newMaximum)
	{
	}

	/// <summary>Raises <see cref="ValueChangedEvent"/>; override to react before handlers run.</summary>
	protected virtual void OnValueChanged(double oldValue, double newValue) =>
		RaiseEvent(new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, ValueChangedEvent));
}
