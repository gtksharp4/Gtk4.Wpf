using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="EventTrigger"/> and <see cref="TriggerAction"/>.
/// </summary>
public class TriggerEventTests
{
	sealed class TEl : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("EvtTag", typeof(string), typeof(TEl),
				new PropertyMetadata(null));
		public string? Tag2 { get => (string?)GetValue(TagProperty); set => SetValue(TagProperty, value); }

		public static readonly RoutedEvent ClickEvent =
			EventManager.RegisterRoutedEvent("TElEvt1Click", RoutingStrategy.Bubble,
				typeof(RoutedEventHandler), typeof(TEl));
		public void FireClick() => RaiseEvent(new RoutedEventArgs(ClickEvent));

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void EventTrigger_runs_action_when_routed_event_fires()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var et = new EventTrigger { RoutedEvent = TEl.ClickEvent };
		et.Actions.Add(new SetPropertyAction { Property = TEl.TagProperty, Value = "clicked" });
		style.Triggers.Add(et);
		el.Style = style;

		el.FireClick();

		Assert.Equal("clicked", el.Tag2);
	}

	[Fact]
	public void EventTrigger_handler_removed_when_style_replaced()
	{
		var el = new TEl();
		int count = 0;
		el.AddChangeHook(TEl.TagProperty, (d, e) => count++);

		var style = new Style(typeof(TEl));
		var et = new EventTrigger { RoutedEvent = TEl.ClickEvent };
		et.Actions.Add(new SetPropertyAction { Property = TEl.TagProperty, Value = "x" });
		style.Triggers.Add(et);

		el.Style = style;
		el.FireClick();
		Assert.Equal(1, count);

		el.Style = new Style(typeof(TEl));
		el.FireClick();
		Assert.Equal(1, count); // no additional fire after style replaced
	}
}