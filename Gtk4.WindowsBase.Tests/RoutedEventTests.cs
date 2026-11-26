using System;
using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;
// ── Domain types ─────────────────────────────────────────────────────────────────

/// <summary>Element that participates in routed events via the InheritanceParent chain.</summary>
class Element : DependencyObject
{
	public DependencyObject? Parent
	{
		get => InheritanceParent;
		set => InheritanceParent = value;
	}

	public static readonly RoutedEvent ClickEvent =
		EventManager.RegisterRoutedEvent(
			"Click", RoutingStrategy.Bubble,
			typeof(RoutedEventHandler), typeof(Element));

	public static readonly RoutedEvent TapEvent =
		EventManager.RegisterRoutedEvent(
			"Tap", RoutingStrategy.Direct,
			typeof(RoutedEventHandler), typeof(Element));

	public static readonly RoutedEvent PreviewClickEvent =
		EventManager.RegisterRoutedEvent(
			"PreviewClick", RoutingStrategy.Tunnel,
			typeof(RoutedEventHandler), typeof(Element));
}

// ── Tests ────────────────────────────────────────────────────────────────────────

public class RoutedEventTests
{
	[Fact]
	public void Bubble_reaches_ancestor_handler()
	{
		var parent = new Element();
		var child = new Element { Parent = parent };

		var visited = new List<DependencyObject>();
		parent.AddHandler(Element.ClickEvent, new RoutedEventHandler((s, e) => visited.Add((DependencyObject)s)));

		child.RaiseEvent(new RoutedEventArgs(Element.ClickEvent));

		Assert.Contains(parent, visited);
	}

	[Fact]
	public void Bubble_visits_source_then_ancestor()
	{
		var parent = new Element();
		var child = new Element { Parent = parent };

		var order = new List<DependencyObject>();
		child.AddHandler(Element.ClickEvent, new RoutedEventHandler((s, e) => order.Add((DependencyObject)s)));
		parent.AddHandler(Element.ClickEvent, new RoutedEventHandler((s, e) => order.Add((DependencyObject)s)));

		child.RaiseEvent(new RoutedEventArgs(Element.ClickEvent));

		Assert.Equal(new DependencyObject[] { child, parent }, order);
	}

	[Fact]
	public void Handled_true_stops_bubble()
	{
		var parent = new Element();
		var child = new Element { Parent = parent };

		bool parentInvoked = false;
		child.AddHandler(Element.ClickEvent, new RoutedEventHandler((s, e) => e.Handled = true));
		parent.AddHandler(Element.ClickEvent, new RoutedEventHandler((s, e) => parentInvoked = true));

		child.RaiseEvent(new RoutedEventArgs(Element.ClickEvent));

		Assert.False(parentInvoked);
	}

	[Fact]
	public void HandledEventsToo_invoked_even_after_handled()
	{
		var parent = new Element();
		var child = new Element { Parent = parent };

		bool parentInvoked = false;
		child.AddHandler(Element.ClickEvent, new RoutedEventHandler((s, e) => e.Handled = true));
		parent.AddHandler(Element.ClickEvent,
			new RoutedEventHandler((s, e) => parentInvoked = true),
			handledEventsToo: true);

		child.RaiseEvent(new RoutedEventArgs(Element.ClickEvent));

		Assert.True(parentInvoked);
	}

	[Fact]
	public void Direct_event_does_not_bubble()
	{
		var parent = new Element();
		var child = new Element { Parent = parent };

		bool parentInvoked = false;
		parent.AddHandler(Element.TapEvent, new RoutedEventHandler((s, e) => parentInvoked = true));

		child.RaiseEvent(new RoutedEventArgs(Element.TapEvent));

		Assert.False(parentInvoked);
	}

	[Fact]
	public void Direct_event_reaches_source_handler()
	{
		var el = new Element();
		bool fired = false;
		el.AddHandler(Element.TapEvent, new RoutedEventHandler((s, e) => fired = true));

		el.RaiseEvent(new RoutedEventArgs(Element.TapEvent));

		Assert.True(fired);
	}

	[Fact]
	public void Tunnel_event_visits_root_then_source()
	{
		var root = new Element();
		var child = new Element { Parent = root };

		var order = new List<DependencyObject>();
		root.AddHandler(Element.PreviewClickEvent, new RoutedEventHandler((s, e) => order.Add((DependencyObject)s)));
		child.AddHandler(Element.PreviewClickEvent, new RoutedEventHandler((s, e) => order.Add((DependencyObject)s)));

		child.RaiseEvent(new RoutedEventArgs(Element.PreviewClickEvent));

		Assert.Equal(new DependencyObject[] { root, child }, order);
	}

	[Fact]
	public void RemoveHandler_stops_invocation()
	{
		var el = new Element();
		int count = 0;
		RoutedEventHandler h = (s, e) => count++;
		el.AddHandler(Element.TapEvent, h);
		el.RemoveHandler(Element.TapEvent, h);

		el.RaiseEvent(new RoutedEventArgs(Element.TapEvent));

		Assert.Equal(0, count);
	}

	[Fact]
	public void Source_is_set_to_raising_element()
	{
		var el = new Element();
		object? src = null;
		el.AddHandler(Element.TapEvent, new RoutedEventHandler((s, e) => src = e.Source));

		el.RaiseEvent(new RoutedEventArgs(Element.TapEvent));

		Assert.Same(el, src);
	}
}