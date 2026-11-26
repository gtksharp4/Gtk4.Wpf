namespace System.Windows.Data;

/// <summary>
/// Event data for <see cref="Data.Binding.SourceUpdatedEvent"/> and <see cref="Data.Binding.TargetUpdatedEvent"/>.
/// </summary>
public class DataTransferEventArgs : RoutedEventArgs
{
	internal DataTransferEventArgs(RoutedEvent routedEvent, DependencyObject targetObject, DependencyProperty property)
		: base(routedEvent)
	{
		TargetObject = targetObject;
		Property = property;
	}

	/// <summary>The binding target element.</summary>
	public DependencyObject TargetObject { get; }

	/// <summary>The binding target property.</summary>
	public DependencyProperty Property { get; }
}
