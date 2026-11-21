using System;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>Routing strategy for a <see cref="RoutedEvent"/>.</summary>
public enum RoutingStrategy
{
	/// <summary>Event travels from the source element up to the root.</summary>
	Bubble,
	/// <summary>Event travels from the root down to the source element.</summary>
	Tunnel,
	/// <summary>Event is delivered only to the source element.</summary>
	Direct,
}

/// <summary>
/// Identifies and declares a routed event.  Instances are created only via
/// <see cref="EventManager.RegisterRoutedEvent"/>.
/// </summary>
public sealed partial class RoutedEvent
{
	/// <summary>Name of this routed event.</summary>
	public string Name { get; }

	/// <summary>Routing strategy (Bubble/Tunnel/Direct).</summary>
	public RoutingStrategy RoutingStrategy { get; }

	/// <summary>Delegate type for handlers (e.g. <see cref="RoutedEventHandler"/>).</summary>
	public Type HandlerType { get; }

	/// <summary>Type that owns (registered) this event.</summary>
	public Type OwnerType { get; }

	internal RoutedEvent(
		string name,
		RoutingStrategy routingStrategy,
		Type handlerType,
		Type ownerType)
	{
		Name = name;
		RoutingStrategy = routingStrategy;
		HandlerType = handlerType;
		OwnerType = ownerType;
	}
}

/// <summary>
/// Provides event-registration services for the routed-event system.
/// </summary>
public static partial class EventManager
{
	static readonly Dictionary<(string, Type), RoutedEvent> _registry =
		new Dictionary<(string, Type), RoutedEvent>();
	static readonly object _lock = new object();

	/// <summary>
	/// Registers a new routed event with the event system.
	/// </summary>
	/// <param name="name">Event name, unique per <paramref name="ownerType"/>.</param>
	/// <param name="routingStrategy">Bubble, Tunnel, or Direct.</param>
	/// <param name="handlerType">Delegate type for event handlers.</param>
	/// <param name="ownerType">Type registering this event.</param>
	/// <returns>A <see cref="RoutedEvent"/> identifier to store as a <c>static readonly</c> field.</returns>
	public static RoutedEvent RegisterRoutedEvent(
		string name,
		RoutingStrategy routingStrategy,
		Type handlerType,
		Type ownerType)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));
		if (handlerType == null) throw new ArgumentNullException(nameof(handlerType));
		if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));

		var key = (name, ownerType);
		lock (_lock)
		{
			if (_registry.ContainsKey(key))
				throw new InvalidOperationException(
					$"RoutedEvent '{name}' is already registered on '{ownerType.FullName}'.");
			var re = new RoutedEvent(name, routingStrategy, handlerType, ownerType);
			_registry[key] = re;
			return re;
		}
	}

	/// <summary>
	/// Returns the routed event registered as <paramref name="name"/> on
	/// <paramref name="ownerType"/> (walks the inheritance chain), or <c>null</c>
	/// when not found. Used by compiler-generated code for EventTrigger lookup.
	/// </summary>
	public static RoutedEvent? GetRoutedEvent(Type ownerType, string name)
	{
		if (ownerType == null || name == null) return null;
		lock (_lock)
		{
			Type? t = ownerType;
			while (t != null)
			{
				if (_registry.TryGetValue((name, t), out var re)) return re;
				t = t.BaseType;
			}
		}
		return null;
	}
}