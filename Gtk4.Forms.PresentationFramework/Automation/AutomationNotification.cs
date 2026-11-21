namespace System.Windows.Automation;

/// <summary>The kind of a notification raised by <see cref="Peers.AutomationPeer.RaiseNotificationEvent"/>.</summary>
public enum AutomationNotificationKind
{
	ItemAdded,
	ItemRemoved,
	ActionCompleted,
	ActionAborted,
	Other,
}

/// <summary>How a notification is processed by a client.</summary>
public enum AutomationNotificationProcessing
{
	ImportantAll,
	ImportantMostRecent,
	All,
	MostRecent,
	CurrentThenMostRecent,
}
