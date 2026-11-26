using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Windows;

/// <summary>
/// Extends <see cref="DependencyObject"/> with routed-event handler registration and
/// event raising.  In full WPF this surface lives on <c>UIElement</c> (PresentationCore);
/// for Phase 0 it is attached here so the property/event system is self-contained in
/// <c>Gtk4.WindowsBase</c>.
/// </summary>
public static partial class RoutedEventHost
{
	// Per-object handler list: routedEvent → list of (handler, handledEventsToo).
	// ConditionalWeakTable holds the element weakly so entries are collected with the element.
	static readonly ConditionalWeakTable<DependencyObject, HandlerMap>
		_handlers = new ConditionalWeakTable<DependencyObject, HandlerMap>();
	static readonly object _lock = new object();

	/// <summary>Per-element map from RoutedEvent to its handler list.</summary>
	sealed class HandlerMap
	{
		internal readonly Dictionary<RoutedEvent, List<(Delegate handler, bool handledEventsToo)>>
			ByEvent = new Dictionary<RoutedEvent, List<(Delegate, bool)>>(
				ReferenceEqualityComparer2.Instance);
	}

	/// <summary>
	/// Adds a routed-event handler to <paramref name="element"/>.
	/// </summary>
	/// <param name="element">The element to register the handler on.</param>
	/// <param name="routedEvent">The event to listen for.</param>
	/// <param name="handler">The delegate to invoke.</param>
	/// <param name="handledEventsToo">
	/// When <c>true</c>, the handler is invoked even after <see cref="RoutedEventArgs.Handled"/>
	/// has been set to <c>true</c>.
	/// </param>
	public static void AddHandler(
		this DependencyObject element,
		RoutedEvent routedEvent,
		Delegate handler,
		bool handledEventsToo = false)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
		if (handler == null) throw new ArgumentNullException(nameof(handler));

		lock (_lock)
		{
			var map = _handlers.GetOrCreateValue(element);
			if (!map.ByEvent.TryGetValue(routedEvent, out var list))
			{
				list = new List<(Delegate, bool)>();
				map.ByEvent[routedEvent] = list;
			}
			list.Add((handler, handledEventsToo));
		}
	}

	/// <summary>Removes a previously added handler.</summary>
	public static void RemoveHandler(
		this DependencyObject element,
		RoutedEvent routedEvent,
		Delegate handler)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
		if (handler == null) throw new ArgumentNullException(nameof(handler));

		lock (_lock)
		{
			if (!_handlers.TryGetValue(element, out var map)) return;
			if (!map.ByEvent.TryGetValue(routedEvent, out var list)) return;
			for (int i = list.Count - 1; i >= 0; i--)
				if (list[i].handler == handler) { list.RemoveAt(i); break; }
		}
	}

	/// <summary>
	/// Raises <paramref name="e"/> on <paramref name="source"/>, routing according to
	/// <see cref="RoutedEvent.RoutingStrategy"/>.
	/// Phase 0: Bubble and Direct are implemented; Tunnel routing walks the same parent
	/// chain in reverse (ancestors first) which is an approximation.
	/// </summary>
	public static void RaiseEvent(this DependencyObject source, RoutedEventArgs e)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (e == null) throw new ArgumentNullException(nameof(e));
		if (e.RoutedEvent == null) throw new InvalidOperationException("RoutedEventArgs.RoutedEvent must be set before raising.");

		e.Source ??= source;

		switch (e.RoutedEvent.RoutingStrategy)
		{
			case RoutingStrategy.Direct:
				InvokeHandlers(source, source, e);
				break;

			case RoutingStrategy.Bubble:
				BubbleEvent(source, e);
				break;

			case RoutingStrategy.Tunnel:
				TunnelEvent(source, e);
				break;
		}
	}

	// ── Private routing implementations ──────────────────────────────────────────

	static void BubbleEvent(DependencyObject source, RoutedEventArgs e)
	{
		var current = source;
		while (current != null)
		{
			InvokeHandlers(current, source, e);
			// Do NOT break on Handled here: routing continues so that elements with
			// handledEventsToo=true still receive the event.  InvokeHandlers skips
			// handlers that did not opt in.
			current = current.InheritanceParent;
		}
	}

	static void TunnelEvent(DependencyObject source, RoutedEventArgs e)
	{
		// Build the ancestor chain.
		var chain = new List<DependencyObject>();
		var current = source;
		while (current != null) { chain.Add(current); current = current.InheritanceParent; }

		// Invoke from root → source.
		for (int i = chain.Count - 1; i >= 0; i--)
		{
			// Like Bubble: keep routing once Handled so handledEventsToo handlers
			// (instance and class) further down the tunnel are still reached.
			InvokeHandlers(chain[i], source, e);
		}
	}

	static void InvokeHandlers(DependencyObject element, DependencyObject originalSource, RoutedEventArgs e)
	{
		// WPF ordering: class handlers (EventManager.RegisterClassHandler) for the element's
		// type chain, most-derived first, run before the element's instance handlers.
		var routedEvent = e.RoutedEvent;
		var classHandlers = EventManager.GetClassHandlers(element.GetType(), routedEvent);
		if (classHandlers != null)
		{
			foreach (var info in classHandlers)
			{
				if (e.Handled && !info.HandledEventsToo) continue;
				InvokeHandler(info.Handler, element, e);
			}
			// A class handler may re-raise with the same args (e.g. MouseDown cracking into
			// MouseLeftButtonDown); restore the identity before the instance handlers run.
			e.RoutedEvent = routedEvent;
		}

		List<(Delegate handler, bool handledEventsToo)>? list;
		lock (_lock)
		{
			if (!_handlers.TryGetValue(element, out var map)) return;
			if (!map.ByEvent.TryGetValue(routedEvent, out var raw)) return;
			list = new List<(Delegate, bool)>(raw); // snapshot
		}
		foreach (var (handler, handledEventsToo) in list)
		{
			if (e.Handled && !handledEventsToo) continue;
			InvokeHandler(handler, element, e);
		}
	}
}

/// <summary>
/// Reference-equality comparer for <see cref="RoutedEvent"/> dictionary keys.
/// </summary>
internal sealed class ReferenceEqualityComparer2 : IEqualityComparer<RoutedEvent>
{
	internal static readonly ReferenceEqualityComparer2 Instance = new ReferenceEqualityComparer2();
	bool IEqualityComparer<RoutedEvent>.Equals(RoutedEvent? x, RoutedEvent? y) => ReferenceEquals(x, y);
	int IEqualityComparer<RoutedEvent>.GetHashCode(RoutedEvent obj) =>
		System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
}