using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class EventSetterTests
{
	static readonly RoutedEvent Ping = SetterTestElement.PingEvent;

	static Style StyleWith(RoutedEventHandler h, bool handledToo = false)
	{
		var style = new Style(typeof(SetterTestElement));
		style.Setters.Add(new EventSetter(Ping, h) { HandledEventsToo = handledToo });
		return style;
	}

	[Fact]
	public void Ctors_and_defaults()
	{
		RoutedEventHandler h = (_, _) => { };
		var s = new EventSetter(Ping, h);
		Assert.Same(Ping, s.Event);
		Assert.Same(h, s.Handler);
		Assert.False(s.HandledEventsToo);
		Assert.Null(new EventSetter().Event);
	}

	[Fact]
	public void Null_event_or_handler_throws()
	{
		Assert.Throws<ArgumentNullException>(() => new EventSetter().Event = null!);
		Assert.Throws<ArgumentNullException>(() => new EventSetter().Handler = null!);
	}

	[Fact]
	public void Handler_is_added_on_apply()
	{
		int n = 0;
		var el = new SetterTestElement();
		el.Style = StyleWith((_, _) => n++);
		el.Ping();
		Assert.Equal(1, n);
	}

	[Fact]
	public void Handler_is_removed_when_style_replaced_or_nulled()
	{
		int n = 0;
		var el = new SetterTestElement();
		el.Style = StyleWith((_, _) => n++);
		el.Style = new Style(typeof(SetterTestElement));
		el.Ping();
		Assert.Equal(0, n);

		el.Style = StyleWith((_, _) => n++);
		el.Style = null;
		el.Ping();
		Assert.Equal(0, n);
	}

	[Fact]
	public void Handler_is_not_added_twice_when_style_reapplied()
	{
		int n = 0;
		var el = new SetterTestElement();
		var style = StyleWith((_, _) => n++);
		el.Style = style;
		style.Apply(el);
		el.Ping();
		Assert.Equal(1, n);
	}

	[Fact]
	// Instance handlers are added first here: style handlers run in registration order (WPF runs them last).
	public void Handled_events_are_skipped_unless_HandledEventsToo()
	{
		int plain = 0, too = 0;
		var a = new SetterTestElement();
		var b = new SetterTestElement();
		a.AddHandler(Ping, new RoutedEventHandler((_, e) => e.Handled = true));
		b.AddHandler(Ping, new RoutedEventHandler((_, e) => e.Handled = true));
		a.Style = StyleWith((_, _) => plain++);
		b.Style = StyleWith((_, _) => too++, handledToo: true);
		a.Ping();
		b.Ping();
		Assert.Equal(0, plain);
		Assert.Equal(1, too);
	}
}
