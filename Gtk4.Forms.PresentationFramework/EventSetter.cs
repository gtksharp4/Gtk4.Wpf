using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Windows;

/// <summary>
/// Attaches a routed-event handler to every element the style is applied to, and removes it when
/// the style is replaced or cleared. In WPF style handlers run after instance handlers; here they
/// are added at apply time and so run in registration order (GTK gap). Not valid in triggers.
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
		Event = routedEvent;
		Handler = handler;
	}

	RoutedEvent? _event;
	Delegate? _handler;

	/// <summary>The routed event to handle.</summary>
	/// <exception cref="ArgumentNullException">The value is null.</exception>
	public RoutedEvent? Event
	{
		get => _event;
		set => _event = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>The handler to attach.</summary>
	/// <exception cref="ArgumentNullException">The value is null.</exception>
	public Delegate? Handler
	{
		get => _handler;
		set => _handler = value ?? throw new ArgumentNullException(nameof(value));
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
