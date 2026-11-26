namespace System.Windows.Media;

/// <summary>Event data for a failed image operation (WPF <c>ExceptionEventArgs</c>).</summary>
public class ExceptionEventArgs : EventArgs
{
	/// <summary>Creates the event data.</summary>
	public ExceptionEventArgs(Exception errorException) => ErrorException = errorException;

	/// <summary>The exception that caused the failure.</summary>
	public Exception ErrorException { get; }
}
