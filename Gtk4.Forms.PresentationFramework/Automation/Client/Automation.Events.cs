using System.Collections.Generic;
using System.Windows.Automation.Peers;

namespace System.Windows.Automation;

public static partial class Automation
{
	/// <summary>
	/// Subscribes to an event raised by the peers. Property-changed, structure-changed and focus events have their
	/// own overloads. Handlers run on the GTK thread, synchronously with the raising code. Events without a peer
	/// source in this layer (window opened and closed) are accepted but never raised.
	/// </summary>
	public static void AddAutomationEventHandler(AutomationEvent eventId, AutomationElement element, TreeScope scope,
		AutomationEventHandler handler)
	{
		if (eventId == null) throw new ArgumentNullException(nameof(eventId));
		Verify(element, scope, handler);
		var source = SourceOf(eventId);
		if (source is AutomationEvents.PropertyChanged or AutomationEvents.StructureChanged
			or AutomationEvents.AutomationFocusChanged)
			throw new ArgumentException("Use the dedicated overload for this event.", nameof(eventId));
		AutomationThread.Call(() => EventSubscriptions.Add(new EventSubscription(
			source ?? (AutomationEvents)(-1), element, scope, handler, eventId)));
	}

	public static void RemoveAutomationEventHandler(AutomationEvent eventId, AutomationElement element,
		AutomationEventHandler handler)
	{
		if (eventId == null) throw new ArgumentNullException(nameof(eventId));
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		AutomationThread.Call(() => EventSubscriptions.Remove(s =>
			s.ClientEvent?.Id == eventId.Id && s.Element == element && s.Handler == (Delegate)handler));
	}

	/// <exception cref="ArgumentException"><paramref name="properties"/> is empty.</exception>
	public static void AddAutomationPropertyChangedEventHandler(AutomationElement element, TreeScope scope,
		AutomationPropertyChangedEventHandler handler, params AutomationProperty[] properties)
	{
		Verify(element, scope, handler);
		if (properties == null) throw new ArgumentNullException(nameof(properties));
		if (properties.Length == 0)
			throw new ArgumentException("At least one property is required.", nameof(properties));
		var ids = new HashSet<int>();
		foreach (var property in properties)
			ids.Add((property ?? throw new ArgumentNullException(nameof(properties))).Id);
		AutomationThread.Call(() => EventSubscriptions.Add(new EventSubscription(
			AutomationEvents.PropertyChanged, element, scope, handler, properties: ids)));
	}

	public static void RemoveAutomationPropertyChangedEventHandler(AutomationElement element,
		AutomationPropertyChangedEventHandler handler)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		AutomationThread.Call(() => EventSubscriptions.Remove(s =>
			s.Source == AutomationEvents.PropertyChanged && s.Element == element && s.Handler == (Delegate)handler));
	}

	public static void AddStructureChangedEventHandler(AutomationElement element, TreeScope scope,
		StructureChangedEventHandler handler)
	{
		Verify(element, scope, handler);
		AutomationThread.Call(() => EventSubscriptions.Add(
			new EventSubscription(AutomationEvents.StructureChanged, element, scope, handler)));
	}

	public static void RemoveStructureChangedEventHandler(AutomationElement element,
		StructureChangedEventHandler handler)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		AutomationThread.Call(() => EventSubscriptions.Remove(s =>
			s.Source == AutomationEvents.StructureChanged && s.Element == element && s.Handler == (Delegate)handler));
	}

	/// <summary>Subscribes to changes of the focused element anywhere in the application.</summary>
	public static void AddAutomationFocusChangedEventHandler(AutomationFocusChangedEventHandler eventHandler)
	{
		if (eventHandler == null) throw new ArgumentNullException(nameof(eventHandler));
		AutomationThread.Call(() => EventSubscriptions.Add(new EventSubscription(
			AutomationEvents.AutomationFocusChanged, null, TreeScope.Subtree, eventHandler)));
	}

	public static void RemoveAutomationFocusChangedEventHandler(AutomationFocusChangedEventHandler eventHandler)
	{
		if (eventHandler == null) throw new ArgumentNullException(nameof(eventHandler));
		AutomationThread.Call(() => EventSubscriptions.Remove(s =>
			s.Source == AutomationEvents.AutomationFocusChanged && s.Handler == (Delegate)eventHandler));
	}

	/// <summary>Removes every handler this client registered.</summary>
	public static void RemoveAllEventHandlers() => AutomationThread.Call(() => EventSubscriptions.Remove(s => true));

	static void Verify(AutomationElement element, TreeScope scope, Delegate handler)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		if (scope == 0) throw new ArgumentException("The scope must not be empty.", nameof(scope));
	}
}
