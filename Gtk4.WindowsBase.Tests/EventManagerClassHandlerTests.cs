using System;
using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>Base element for class-handler tests; each test registers its own event.</summary>
class ChBase : DependencyObject
{
	public DependencyObject? Parent
	{
		get => InheritanceParent;
		set => InheritanceParent = value;
	}

	static int _counter;

	public static RoutedEvent NewEvent(RoutingStrategy strategy = RoutingStrategy.Bubble) =>
		EventManager.RegisterRoutedEvent(
			"Ch" + System.Threading.Interlocked.Increment(ref _counter), strategy,
			typeof(RoutedEventHandler), typeof(ChBase));
}

class ChDerived : ChBase { }

class ChUnrelated : DependencyObject { }

public class EventManagerClassHandlerTests
{
	static RoutedEventHandler Log(List<string> log, string tag, bool handle = false) =>
		(s, e) => { log.Add(tag); if (handle) e.Handled = true; };

	[Fact]
	public void Class_handler_runs_before_instance_handler()
	{
		var ev = ChBase.NewEvent();
		var log = new List<string>();
		var el = new ChBase();
		el.AddHandler(ev, Log(log, "instance"));
		EventManager.RegisterClassHandler(typeof(ChBase), ev, Log(log, "class"));
		el.RaiseEvent(new RoutedEventArgs(ev));
		Assert.Equal(new[] { "class", "instance" }, log);
	}

	[Fact]
	public void Most_derived_class_handler_runs_first()
	{
		var ev = ChBase.NewEvent();
		var log = new List<string>();
		EventManager.RegisterClassHandler(typeof(ChBase), ev, Log(log, "base"));
		EventManager.RegisterClassHandler(typeof(ChDerived), ev, Log(log, "derived"));
		new ChDerived().RaiseEvent(new RoutedEventArgs(ev));
		Assert.Equal(new[] { "derived", "base" }, log);
	}

	[Fact]
	public void Base_class_handler_not_invoked_for_derived_only_registration_on_base_instance()
	{
		var ev = ChBase.NewEvent();
		var log = new List<string>();
		EventManager.RegisterClassHandler(typeof(ChDerived), ev, Log(log, "derived"));
		new ChBase().RaiseEvent(new RoutedEventArgs(ev));
		Assert.Empty(log);
	}

	[Fact]
	public void HandledEventsToo_false_is_skipped_once_handled()
	{
		var ev = ChBase.NewEvent();
		var log = new List<string>();
		EventManager.RegisterClassHandler(typeof(ChDerived), ev, Log(log, "derived", handle: true));
		EventManager.RegisterClassHandler(typeof(ChBase), ev, Log(log, "base"));
		new ChDerived().RaiseEvent(new RoutedEventArgs(ev));
		Assert.Equal(new[] { "derived" }, log);
	}

	[Fact]
	public void HandledEventsToo_true_still_runs_once_handled()
	{
		var ev = ChBase.NewEvent();
		var log = new List<string>();
		EventManager.RegisterClassHandler(typeof(ChDerived), ev, Log(log, "derived", handle: true));
		EventManager.RegisterClassHandler(typeof(ChBase), ev, Log(log, "base"), true);
		new ChDerived().RaiseEvent(new RoutedEventArgs(ev));
		Assert.Equal(new[] { "derived", "base" }, log);
	}

	[Fact]
	public void Class_handler_marking_handled_suppresses_instance_handlers()
	{
		var ev = ChBase.NewEvent();
		var log = new List<string>();
		var el = new ChBase();
		el.AddHandler(ev, Log(log, "instance"));
		el.AddHandler(ev, Log(log, "instance-too"), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(ChBase), ev, Log(log, "class", handle: true));
		el.RaiseEvent(new RoutedEventArgs(ev));
		Assert.Equal(new[] { "class", "instance-too" }, log);
	}

	[Fact]
	public void Unrelated_type_registration_does_not_fire()
	{
		var ev = ChBase.NewEvent();
		var log = new List<string>();
		EventManager.RegisterClassHandler(typeof(ChUnrelated), ev, Log(log, "unrelated"));
		new ChBase().RaiseEvent(new RoutedEventArgs(ev));
		Assert.Empty(log);
	}

	[Fact]
	public void Bubble_route_invokes_class_handlers_at_every_hop()
	{
		var ev = ChBase.NewEvent();
		var hops = new List<object>();
		EventManager.RegisterClassHandler(typeof(ChBase), ev,
			new RoutedEventHandler((s, e) => hops.Add(s)));
		var parent = new ChBase();
		var child = new ChDerived { Parent = parent };
		child.RaiseEvent(new RoutedEventArgs(ev));
		Assert.Equal(new object[] { child, parent }, hops);
	}

	[Fact]
	public void Tunnel_route_invokes_class_handlers_root_first()
	{
		var ev = ChBase.NewEvent(RoutingStrategy.Tunnel);
		var hops = new List<object>();
		EventManager.RegisterClassHandler(typeof(ChBase), ev,
			new RoutedEventHandler((s, e) => hops.Add(s)));
		var parent = new ChBase();
		var child = new ChBase { Parent = parent };
		child.RaiseEvent(new RoutedEventArgs(ev));
		Assert.Equal(new object[] { parent, child }, hops);
	}

	[Fact]
	public void Null_arguments_throw_ArgumentNullException()
	{
		var ev = ChBase.NewEvent();
		RoutedEventHandler h = (s, e) => { };
		Assert.Throws<ArgumentNullException>(() => EventManager.RegisterClassHandler(null!, ev, h));
		Assert.Throws<ArgumentNullException>(() =>
			EventManager.RegisterClassHandler(typeof(ChBase), null!, h));
		Assert.Throws<ArgumentNullException>(() =>
			EventManager.RegisterClassHandler(typeof(ChBase), ev, null!, true));
	}

	[Fact]
	public void Mismatched_delegate_type_throws_ArgumentException()
	{
		var ev = ChBase.NewEvent();
		EventHandler wrong = (s, e) => { };
		Assert.Throws<ArgumentException>(() => EventManager.RegisterClassHandler(typeof(ChBase), ev, wrong));
	}

	[Fact]
	public void Non_DependencyObject_class_type_throws_ArgumentException()
	{
		var ev = ChBase.NewEvent();
		RoutedEventHandler h = (s, e) => { };
		Assert.Throws<ArgumentException>(() => EventManager.RegisterClassHandler(typeof(string), ev, h));
	}

	[Fact]
	public void GetRoutedEventsForOwner_returns_registered_events()
	{
		var ev = ChBase.NewEvent();
		var owned = EventManager.GetRoutedEventsForOwner(typeof(ChBase));
		Assert.NotNull(owned);
		Assert.Contains(ev, owned!);
		Assert.Null(EventManager.GetRoutedEventsForOwner(typeof(ChUnrelated)));
		Assert.Contains(ev, EventManager.GetRoutedEvents());
	}
}
