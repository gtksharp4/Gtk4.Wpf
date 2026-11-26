using System;

namespace System.Windows;

public abstract partial class UIElement
{
	// ── Routed events ─────────────────────────────────────────────────────────

	/// <summary>
	/// Adds a handler for a routed event. Phase-0: handler stored in a simple list;
	/// routing walks the logical parent chain.
	/// </summary>
	public void AddHandler(RoutedEvent routedEvent, Delegate handler)
	{
		if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		RoutedEventHost.AddHandler(this, routedEvent, handler);
	}

	/// <summary>
	/// Adds a handler, optionally receiving already-handled events.
	/// WPF-compatible 3-argument overload; unmodified WPF code uses this form.
	/// </summary>
	public void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
	{
		if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		RoutedEventHost.AddHandler(this, routedEvent, handler, handledEventsToo);
	}

	/// <summary>Removes a previously added routed-event handler.</summary>
	public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
	{
		if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		RoutedEventHost.RemoveHandler(this, routedEvent, handler);
	}

	/// <summary>
	/// Raises <paramref name="e"/> on this element, routing it according to its strategy.
	/// </summary>
	protected void RaiseEvent(RoutedEventArgs e)
	{
		if (e == null) throw new ArgumentNullException(nameof(e));
		RoutedEventHost.RaiseEvent(this, e);
	}
}
