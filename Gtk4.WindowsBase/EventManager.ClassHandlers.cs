using System;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Class-handler registration and routed-event enumeration (WPF <c>EventManager</c> surface).
/// </summary>
public static partial class EventManager
{
	// routedEvent → (classType → handlers in registration order).
	static readonly Dictionary<RoutedEvent, Dictionary<Type, List<ClassHandlerInfo>>> _classHandlers =
		new Dictionary<RoutedEvent, Dictionary<Type, List<ClassHandlerInfo>>>(ReferenceEqualityComparer2.Instance);

	/// <summary>A registered class handler and its <c>handledEventsToo</c> flag.</summary>
	internal readonly struct ClassHandlerInfo
	{
		internal readonly Delegate Handler;
		internal readonly bool HandledEventsToo;

		internal ClassHandlerInfo(Delegate handler, bool handledEventsToo)
		{
			Handler = handler;
			HandledEventsToo = handledEventsToo;
		}
	}

	/// <summary>
	/// Registers a class handler for <paramref name="routedEvent"/> on every instance of
	/// <paramref name="classType"/> (and derived types). Class handlers run before the
	/// instance handlers of the same element.
	/// </summary>
	/// <remarks>
	/// WPF restricts <paramref name="classType"/> to <c>UIElement</c>, <c>ContentElement</c> or
	/// <c>UIElement3D</c>; those live in a higher assembly in this layer, so the check here is
	/// "derives from <see cref="DependencyObject"/>" (the type the routing host operates on).
	/// </remarks>
	public static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler) =>
		RegisterClassHandler(classType, routedEvent, handler, false);

	/// <summary>
	/// Registers a class handler, optionally invoked even for events already marked handled.
	/// </summary>
	/// <param name="classType">The class that declares the class handling.</param>
	/// <param name="routedEvent">The routed event identifier of the event to handle.</param>
	/// <param name="handler">A reference to the class handler implementation.</param>
	/// <param name="handledEventsToo">
	/// <c>true</c> to invoke the handler even if the routed event was already marked handled.
	/// </param>
	/// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="handler"/> is not of the event's handler type, or
	/// <paramref name="classType"/> does not derive from <see cref="DependencyObject"/>.
	/// </exception>
	public static void RegisterClassHandler(
		Type classType, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
	{
		if (classType == null) throw new ArgumentNullException(nameof(classType));
		if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		if (!typeof(DependencyObject).IsAssignableFrom(classType))
			throw new ArgumentException(
				$"'{classType.FullName}' must derive from DependencyObject.", nameof(classType));
		var handlerType = handler.GetType();
		if (handlerType != routedEvent.HandlerType && handlerType != typeof(RoutedEventHandler))
			throw new ArgumentException(
				$"Handler type '{handlerType.FullName}' is not valid for routed event '{routedEvent.Name}'.",
				nameof(handler));

		lock (_lock)
		{
			if (!_classHandlers.TryGetValue(routedEvent, out var byType))
			{
				byType = new Dictionary<Type, List<ClassHandlerInfo>>();
				_classHandlers[routedEvent] = byType;
			}
			if (!byType.TryGetValue(classType, out var list))
			{
				list = new List<ClassHandlerInfo>();
				byType[classType] = list;
			}
			list.Add(new ClassHandlerInfo(handler, handledEventsToo));
		}
	}

	/// <summary>Returns every routed event registered with the event system.</summary>
	public static RoutedEvent[] GetRoutedEvents()
	{
		lock (_lock)
		{
			var result = new RoutedEvent[_registry.Count];
			_registry.Values.CopyTo(result, 0);
			return result;
		}
	}

	/// <summary>
	/// Returns the routed events registered with <paramref name="ownerType"/> as owner, or
	/// <c>null</c> when it owns none (WPF behaviour).
	/// </summary>
	public static RoutedEvent[]? GetRoutedEventsForOwner(Type ownerType)
	{
		if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));
		lock (_lock)
		{
			var result = new List<RoutedEvent>();
			foreach (var re in _registry.Values)
				if (re.OwnerType == ownerType) result.Add(re);
			return result.Count == 0 ? null : result.ToArray();
		}
	}

	/// <summary>
	/// Snapshot of class handlers that apply to <paramref name="elementType"/> for
	/// <paramref name="routedEvent"/>: most-derived class first, then each base class.
	/// Returns <c>null</c> when there are none.
	/// </summary>
	internal static List<ClassHandlerInfo>? GetClassHandlers(Type elementType, RoutedEvent routedEvent)
	{
		lock (_lock)
		{
			if (!_classHandlers.TryGetValue(routedEvent, out var byType)) return null;
			List<ClassHandlerInfo>? result = null;
			for (Type? t = elementType; t != null; t = t.BaseType)
			{
				if (!byType.TryGetValue(t, out var list)) continue;
				result ??= new List<ClassHandlerInfo>();
				result.AddRange(list);
			}
			return result;
		}
	}
}
