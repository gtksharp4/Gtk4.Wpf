using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pins the idempotent re-attach contract (Round-16 A4): attaching a trigger twice
/// must not orphan the first subscription, so a single detach fully removes it.
/// Without the guard a second attach leaks the first <c>TriggerState</c>/handler and
/// one detach leaves a dangling contributor.
/// </summary>
public class ElementTriggerReattachTests
{
	sealed class TEl : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("ReTrigTag", typeof(string), typeof(TEl),
				new PropertyMetadata(null));
		public static readonly DependencyProperty FlagProperty =
			DependencyProperty.Register("ReTrigFlag", typeof(bool), typeof(TEl),
				new PropertyMetadata(false));

		public static readonly RoutedEvent PingEvent =
			EventManager.RegisterRoutedEvent("ReTrigPing", RoutingStrategy.Bubble,
				typeof(RoutedEventHandler), typeof(TEl));

		public string? Tag2 { get => (string?)GetValue(TagProperty); set => SetValue(TagProperty, value); }
		public bool Flag { get => (bool)GetValue(FlagProperty)!; set => SetValue(FlagProperty, value); }
		public void FirePing() => RaiseEvent(new RoutedEventArgs(PingEvent));

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	sealed class CountAction : TriggerAction
	{
		internal int Count;
		internal override void Invoke(FrameworkElement target) => Count++;
	}

	[Fact]
	public void Property_trigger_double_attach_then_single_detach_fully_reverts()
	{
		var el = new TEl();
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "on" });
		el.Triggers.Add(trigger);

		el.AttachElementTriggers();
		el.AttachElementTriggers(); // re-attach: must replace, not duplicate.

		el.Flag = true;
		Assert.Equal("on", el.Tag2);

		el.DetachElementTriggers();
		// A single detach must clear the setter — no orphaned TriggerState contributor.
		Assert.Null(el.Tag2);
	}

	[Fact]
	public void Event_trigger_double_attach_fires_action_once_per_event()
	{
		var el = new TEl();
		var action = new CountAction();
		var et = new EventTrigger { RoutedEvent = TEl.PingEvent };
		et.Actions.Add(action);
		el.Triggers.Add(et);

		el.AttachElementTriggers();
		el.AttachElementTriggers(); // re-attach: must not leave two handlers.

		el.FirePing();
		Assert.Equal(1, action.Count);

		el.DetachElementTriggers();
		el.FirePing();
		// A single detach removes the only handler — no orphaned subscription.
		Assert.Equal(1, action.Count);
	}
}
