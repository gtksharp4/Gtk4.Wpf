namespace System.Windows;

/// <summary>Event data for <c>MediaElement.ScriptCommand</c>. GTK media streams carry no script commands.</summary>
public sealed class MediaScriptCommandRoutedEventArgs : RoutedEventArgs
{
	internal MediaScriptCommandRoutedEventArgs(
		RoutedEvent routedEvent, object source, string parameterType, string parameterValue)
		: base(routedEvent)
	{
		Source = source;
		ParameterType = parameterType;
		ParameterValue = parameterValue;
	}

	/// <summary>The type of the script command.</summary>
	public string ParameterType { get; }

	/// <summary>The value of the script command.</summary>
	public string ParameterValue { get; }
}
