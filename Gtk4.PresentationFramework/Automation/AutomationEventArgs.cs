namespace System.Windows.Automation;

/// <summary>Event data that carries the <see cref="AutomationEvent"/> that was raised.</summary>
public class AutomationEventArgs : EventArgs
{
	public AutomationEventArgs(AutomationEvent eventId)
	{
		EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
	}

	public AutomationEvent EventId { get; }
}

/// <summary>The state of an asynchronous content load, reported to clients.</summary>
public enum AsyncContentLoadedState
{
	Beginning,
	Progress,
	Completed,
}

/// <summary>Event data for <see cref="Peers.AutomationPeer.RaiseAsyncContentLoadedEvent"/>.</summary>
public sealed class AsyncContentLoadedEventArgs : AutomationEventArgs
{
	public AsyncContentLoadedEventArgs(AsyncContentLoadedState asyncContentState, double percentComplete)
		: base(AutomationElementIdentifiers.AsyncContentLoadedEvent)
	{
		AsyncContentLoadedState = asyncContentState;
		PercentComplete = percentComplete;
	}

	public AsyncContentLoadedState AsyncContentLoadedState { get; }

	public double PercentComplete { get; }
}
