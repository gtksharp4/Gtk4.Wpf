using System.Collections.Generic;

namespace System.Windows.Automation.Peers;

public abstract partial class AutomationPeer
{
	static readonly Dictionary<AutomationEvents, int> _listeners = new Dictionary<AutomationEvents, int>();

	/// <summary>Raised for every automation event that has a listener. The client API subscribes here.</summary>
	internal static event Action<AutomationPeer, AutomationEvents>? EventRaised;

	/// <summary>Raised for every property-changed event that has a listener.</summary>
	internal static event Action<AutomationPeer, AutomationProperty, object?, object?>? PropertyChangedRaised;

	/// <summary>Raised for every notification event.</summary>
	internal static event Action<AutomationPeer, AutomationNotificationKind, AutomationNotificationProcessing,
		string?, string?>? NotificationRaised;

	internal static void AddListener(AutomationEvents eventId)
	{
		lock (_listeners)
		{
			_listeners.TryGetValue(eventId, out int count);
			_listeners[eventId] = count + 1;
		}
	}

	internal static void RemoveListener(AutomationEvents eventId)
	{
		lock (_listeners)
		{
			if (_listeners.TryGetValue(eventId, out int count) && count > 0)
				_listeners[eventId] = count - 1;
		}
	}

	/// <summary>Returns <c>true</c> when a client listens for <paramref name="eventId"/>.</summary>
	public static bool ListenerExists(AutomationEvents eventId)
	{
		lock (_listeners)
			return _listeners.TryGetValue(eventId, out int count) && count > 0;
	}

	/// <summary>Raises <paramref name="eventId"/> for listening clients.</summary>
	public void RaiseAutomationEvent(AutomationEvents eventId)
	{
		if (!ListenerExists(eventId)) return;
		EventRaised?.Invoke(EventsSource ?? this, eventId);
	}

	/// <summary>Raises a property-changed event when the value actually changed.</summary>
	public void RaisePropertyChangedEvent(AutomationProperty property, object? oldValue, object? newValue)
	{
		if (property == null) throw new ArgumentNullException(nameof(property));
		if (!ListenerExists(AutomationEvents.PropertyChanged) || Equals(oldValue, newValue)) return;
		PropertyChangedRaised?.Invoke(EventsSource ?? this, property, oldValue, newValue);
	}

	/// <summary>Raises a notification event, for example to announce a text change.</summary>
	public void RaiseNotificationEvent(AutomationNotificationKind notificationKind,
		AutomationNotificationProcessing notificationProcessing, string? displayString, string? activityId)
	{
		NotificationRaised?.Invoke(EventsSource ?? this, notificationKind, notificationProcessing,
			displayString, activityId);
	}

	/// <summary>Raises the async-content-loaded event.</summary>
	public void RaiseAsyncContentLoadedEvent(AsyncContentLoadedEventArgs args)
	{
		if (args == null) throw new ArgumentNullException(nameof(args));
		RaiseAutomationEvent(AutomationEvents.AsyncContentLoaded);
	}

	void RaiseStructureChanged() => RaiseAutomationEvent(AutomationEvents.StructureChanged);
}

/// <summary>The state of an asynchronous content load, reported to clients.</summary>
public enum AsyncContentLoadedState
{
	Beginning,
	Progress,
	Completed,
}

/// <summary>Event data for <see cref="AutomationPeer.RaiseAsyncContentLoadedEvent"/>.</summary>
public class AsyncContentLoadedEventArgs : EventArgs
{
	public AsyncContentLoadedEventArgs(AsyncContentLoadedState asyncContentState, double percentComplete)
	{
		AsyncContentLoadedState = asyncContentState;
		PercentComplete = percentComplete;
	}

	public AsyncContentLoadedState AsyncContentLoadedState { get; }

	public double PercentComplete { get; }
}
