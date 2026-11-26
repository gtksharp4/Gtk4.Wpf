using System.Collections.Generic;
using System.Windows.Automation.Peers;

namespace System.Windows.Automation;

/// <summary>One registered client handler.</summary>
sealed class EventSubscription
{
	internal EventSubscription(AutomationEvents source, AutomationElement? element, TreeScope scope, Delegate handler,
		AutomationEvent? clientEvent = null, HashSet<int>? properties = null)
	{
		Source = source;
		Element = element;
		Scope = scope;
		Handler = handler;
		ClientEvent = clientEvent;
		Properties = properties;
	}

	internal AutomationEvents Source { get; }

	/// <summary>The element the scope is relative to; <c>null</c> for a focus subscription, which is global.</summary>
	internal AutomationElement? Element { get; }

	internal TreeScope Scope { get; }

	internal Delegate Handler { get; }

	internal AutomationEvent? ClientEvent { get; }

	internal HashSet<int>? Properties { get; }
}

/// <summary>
/// Delivers peer events to client handlers. Handlers run synchronously on the thread that raised the event,
/// which is the GTK thread; one that throws is logged and never breaks the raiser or the other handlers.
/// </summary>
static class EventSubscriptions
{
	static readonly List<EventSubscription> _subscriptions = new List<EventSubscription>();
	static bool _attached;

	internal static void Add(EventSubscription subscription)
	{
		lock (_subscriptions)
		{
			_subscriptions.Add(subscription);
			AutomationPeer.AddListener(subscription.Source);
			Attach();
		}
	}

	internal static void Remove(Predicate<EventSubscription> match)
	{
		lock (_subscriptions)
		{
			for (int i = _subscriptions.Count - 1; i >= 0; i--)
				if (match(_subscriptions[i]))
				{
					AutomationPeer.RemoveListener(_subscriptions[i].Source);
					_subscriptions.RemoveAt(i);
				}
			if (_subscriptions.Count == 0) Detach();
		}
	}

	static void Attach()
	{
		if (_attached) return;
		_attached = true;
		AutomationPeer.EventRaised += OnEvent;
		AutomationPeer.PropertyChangedRaised += OnPropertyChanged;
	}

	static void Detach()
	{
		if (!_attached) return;
		_attached = false;
		AutomationPeer.EventRaised -= OnEvent;
		AutomationPeer.PropertyChangedRaised -= OnPropertyChanged;
	}

	static EventSubscription[] Snapshot(AutomationEvents source)
	{
		lock (_subscriptions) return _subscriptions.FindAll(s => s.Source == source).ToArray();
	}

	static void OnEvent(AutomationPeer peer, AutomationEvents source)
	{
		foreach (var subscription in Snapshot(source))
		{
			if (!InScope(subscription, peer)) continue;
			var sender = new AutomationElement(peer, null);
			switch (subscription.Handler)
			{
				case AutomationFocusChangedEventHandler focus:
					Deliver(() => focus(sender, new AutomationFocusChangedEventArgs(-1, 0)));
					break;
				case StructureChangedEventHandler structure:
					var pending = AutomationPeer.PendingStructure;
					var args = new StructureChangedEventArgs(pending?.Type ?? StructureChangeType.ChildrenInvalidated,
						AutomationElement.RuntimeIdOf(pending?.Subject ?? peer));
					Deliver(() => structure(sender, args));
					break;
				case AutomationEventHandler plain:
					var plainArgs = AutomationPeer.PendingAsyncContentLoaded is { } loaded
						&& source == AutomationEvents.AsyncContentLoaded
						? loaded : new AutomationEventArgs(subscription.ClientEvent!);
					Deliver(() => plain(sender, plainArgs));
					break;
			}
		}
	}

	static void OnPropertyChanged(AutomationPeer peer, AutomationProperty property, object? oldValue, object? newValue)
	{
		foreach (var subscription in Snapshot(AutomationEvents.PropertyChanged))
		{
			if (!InScope(subscription, peer) || !subscription.Properties!.Contains(property.Id)) continue;
			var handler = (AutomationPropertyChangedEventHandler)subscription.Handler;
			var sender = new AutomationElement(peer, null);
			var args = new AutomationPropertyChangedEventArgs(property, oldValue, newValue);
			Deliver(() => handler(sender, args));
		}
	}

	static bool InScope(EventSubscription subscription, AutomationPeer source)
	{
		var root = subscription.Element?.Peer;
		if (root == null) return true;
		var scope = subscription.Scope;
		if ((scope & TreeScope.Element) != 0 && ReferenceEquals(root, source)) return true;
		if ((scope & (TreeScope.Children | TreeScope.Descendants)) != 0)
			for (var up = ElementNavigation.RawParent(source); up != null; up = ElementNavigation.RawParent(up))
				if (ReferenceEquals(up, root))
					return (scope & TreeScope.Descendants) != 0
						|| ReferenceEquals(ElementNavigation.RawParent(source), root);
		if ((scope & (TreeScope.Parent | TreeScope.Ancestors)) != 0)
			for (var up = ElementNavigation.RawParent(root); up != null; up = ElementNavigation.RawParent(up))
				if (ReferenceEquals(up, source))
					return (scope & TreeScope.Ancestors) != 0
						|| ReferenceEquals(ElementNavigation.RawParent(root), source);
		return false;
	}

	static void Deliver(Action handler)
	{
		try { AutomationThread.Deliver(handler); }
		catch (Exception e)
		{
			System.Diagnostics.Trace.TraceWarning($"An automation event handler threw {e.GetType().Name}: {e.Message}");
		}
	}
}
