namespace System.Windows;

/// <summary>Event data for routed events that report a failure, such as <c>MediaElement.MediaFailed</c>.</summary>
public sealed class ExceptionRoutedEventArgs : RoutedEventArgs
{
	internal ExceptionRoutedEventArgs(RoutedEvent routedEvent, object source, Exception errorException)
		: base(routedEvent)
	{
		Source = source;
		ErrorException = errorException ?? throw new ArgumentNullException(nameof(errorException));
	}

	/// <summary>The exception that describes the failure.</summary>
	public Exception ErrorException { get; }
}
