using System;
using System.Windows;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class EventSetterValidationTests
{
	static readonly RoutedEvent Ping = SetterTestElement.PingEvent;

	[Fact]
	public void Ctor_null_arguments_report_WPF_parameter_names()
	{
		RoutedEventHandler h = (_, _) => { };
		Assert.Equal("routedEvent", Assert.Throws<ArgumentNullException>(() => new EventSetter(null!, h)).ParamName);
		Assert.Equal("handler", Assert.Throws<ArgumentNullException>(() => new EventSetter(Ping, null!)).ParamName);
	}

	[Fact]
	public void Handler_of_wrong_type_throws_unless_RoutedEventHandler()
	{
		Delegate wrong = new Action(() => { });
		Assert.Throws<ArgumentException>(() => new EventSetter(UIElement.MouseDownEvent, wrong));
		Assert.Throws<ArgumentException>(() => new EventSetter { Event = UIElement.MouseDownEvent, Handler = wrong });
		var ok = new EventSetter(UIElement.MouseDownEvent, new RoutedEventHandler((_, _) => { }));
		Assert.NotNull(ok.Handler);
	}

	[Fact]
	public void Trigger_containing_an_EventSetter_throws_on_attach()
	{
		var setter = new EventSetter(Ping, new RoutedEventHandler((_, _) => { }));
		TriggerBase[] triggers =
		{
			Make(new Trigger { Property = SetterTestElement.FlagProperty, Value = true }, t => t.Setters, setter),
			Make(new DataTrigger { Binding = new Binding("X"), Value = 1 }, t => t.Setters, setter),
			Make(new MultiTrigger(), t => t.Setters, setter),
			Make(new MultiDataTrigger(), t => t.Setters, setter),
		};
		foreach (var trigger in triggers)
		{
			var style = new Style(typeof(SetterTestElement));
			style.Triggers.Add(trigger);
			var ex = Assert.Throws<InvalidOperationException>(() => new SetterTestElement { Style = style });
			Assert.Contains("EventSetter", ex.Message);
		}
	}

	static T Make<T>(T trigger, Func<T, SetterBaseCollection> setters, SetterBase s) where T : TriggerBase
	{
		setters(trigger).Add(s);
		return trigger;
	}
}
