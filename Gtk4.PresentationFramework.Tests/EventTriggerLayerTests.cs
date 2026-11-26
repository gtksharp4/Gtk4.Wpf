using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// E6: SetPropertyAction writes through the trigger layer (D8), not the local
/// layer, so it does not permanently shadow a style setter.
/// A style setter sets the initial value; an event trigger's SetPropertyAction
/// overrides it via D8; the style value must still be accessible from the layer
/// (style wins once the trigger layer entry is for a different contributor).
/// </summary>
public class EventTriggerLayerTests
{
	sealed class TEl : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("EvtLayerTag", typeof(string), typeof(TEl),
				new PropertyMetadata("default"));

		public static readonly RoutedEvent PingEvent =
			EventManager.RegisterRoutedEvent("TElEvtLayerPing", RoutingStrategy.Bubble,
				typeof(RoutedEventHandler), typeof(TEl));

		public string? Tag2 { get => (string?)GetValue(TagProperty); set => SetValue(TagProperty, value); }

		public void FirePing() => RaiseEvent(new RoutedEventArgs(PingEvent));

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void SetPropertyAction_writes_trigger_layer_not_local()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));

		// Style setter provides "styled".
		style.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "styled" });

		var et = new EventTrigger { RoutedEvent = TEl.PingEvent };
		et.Actions.Add(new SetPropertyAction { Property = TEl.TagProperty, Value = "event" });
		style.Triggers.Add(et);
		el.Style = style;

		// Before event: style value visible.
		Assert.Equal("styled", el.Tag2);

		// Fire event: trigger layer overrides style.
		el.FirePing();
		Assert.Equal("event", el.Tag2);
	}

	[Fact]
	public void SetPropertyAction_does_not_set_local_value()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var et = new EventTrigger { RoutedEvent = TEl.PingEvent };
		et.Actions.Add(new SetPropertyAction { Property = TEl.TagProperty, Value = "event" });
		style.Triggers.Add(et);
		el.Style = style;

		el.FirePing();

		// ReadLocalValue must still return UnsetValue — trigger layer, not local.
		Assert.Same(DependencyProperty.UnsetValue, el.ReadLocalValue(TEl.TagProperty));
	}
}