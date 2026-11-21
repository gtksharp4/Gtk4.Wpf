using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Windows;

/// <summary>
/// Attaches a routed-event handler to every element the style is applied to, and removes it when
/// the style is replaced or cleared. WPF runs style handlers after instance handlers; here they are added at
/// apply time and run in registration order (engine limitation, not GTK). Not valid in triggers: attaching a
/// trigger that contains one throws.
/// </summary>
public class EventSetter : SetterBase
{
	sealed class Attached
	{
		public RoutedEvent Event = null!;
		public Delegate Handler = null!;
	}

	static readonly ConditionalWeakTable<UIElement, Dictionary<EventSetter, Attached>> _attached =
		new ConditionalWeakTable<UIElement, Dictionary<EventSetter, Attached>>();

	/// <summary>Creates an empty setter.</summary>
	public EventSetter() { }

	/// <summary>Creates a setter for <paramref name="routedEvent"/> and <paramref name="handler"/>.</summary>
	public EventSetter(RoutedEvent routedEvent, Delegate handler)
	{
		if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		_event = routedEvent;
		Handler = handler;
	}

	RoutedEvent? _event;
	Delegate? _handler;

	/// <summary>The routed event to handle.</summary>
	/// <exception cref="ArgumentNullException">The value is null.</exception>
	public RoutedEvent? Event
	{
		get => _event;
		set
		{
			CheckSealed();
			_event = value ?? throw new ArgumentNullException(nameof(value));
		}
	}

	/// <summary>The handler to attach.</summary>
	/// <exception cref="ArgumentNullException">The value is null.</exception>
	/// <exception cref="ArgumentException">The handler type does not match the event.</exception>
	public Delegate? Handler
	{
		get => _handler;
		set
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			CheckSealed();
			if (_event != null && value.GetType() != _event.HandlerType && value is not RoutedEventHandler)
				throw new ArgumentException(
					$"Handler type {value.GetType()} does not match {_event.HandlerType}.", nameof(value));
			_handler = value;
		}
	}

	/// <summary>Whether the handler also receives events already marked handled.</summary>
	public bool HandledEventsToo { get; set; }

	/// <inheritdoc/>
	internal override void Apply(DependencyObject target) => ApplyToStyleLayer(target);

	/// <inheritdoc/>
	internal override void ApplyToStyleLayer(DependencyObject target)
	{
		if (target is not UIElement el || _event == null || _handler == null) return;
		Unapply(target, null);
		el.AddHandler(_event, _handler, HandledEventsToo);
		_attached.GetOrCreateValue(el)[this] = new Attached { Event = _event, Handler = _handler };
	}

	/// <inheritdoc/>
	internal override void ApplyToTriggerLayer(
		DependencyObject target, object contributor, List<DependencyProperty> appliedDps) { }

	/// <inheritdoc/>
	internal override void Unapply(DependencyObject target, object? contributor)
	{
		if (contributor != null || target is not UIElement el) return;
		if (!_attached.TryGetValue(el, out var map) || !map.TryGetValue(this, out var a)) return;
		map.Remove(this);
		el.RemoveHandler(a.Event, a.Handler);
	}
}
