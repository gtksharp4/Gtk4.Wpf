using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for element-level triggers declared on <see cref="FrameworkElement.Triggers"/>
/// (not inside a Style).  Covers property trigger and event trigger at element level,
/// called via <see cref="FrameworkElement.AttachElementTriggers"/>.
/// </summary>
public class ElementTriggerTests
{
	[Fact]
	public void DetachElementTriggers_is_not_public_API()
	{
		Assert.Null(typeof(FrameworkElement).GetMethod(
			"DetachElementTriggers", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));
	}

	sealed class TEl : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("ElemTrigTag", typeof(string), typeof(TEl),
				new PropertyMetadata(null));
		public static readonly DependencyProperty FlagProperty =
			DependencyProperty.Register("ElemTrigFlag", typeof(bool), typeof(TEl),
				new PropertyMetadata(false));

		public static readonly RoutedEvent PingEvent =
			EventManager.RegisterRoutedEvent("ElemTrigPing", RoutingStrategy.Bubble,
				typeof(RoutedEventHandler), typeof(TEl));

		public string? Tag2 { get => (string?)GetValue(TagProperty); set => SetValue(TagProperty, value); }
		public bool Flag { get => (bool)GetValue(FlagProperty)!; set => SetValue(FlagProperty, value); }
		public void FirePing() => RaiseEvent(new RoutedEventArgs(PingEvent));

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	// ── Property trigger at element level ─────────────────────────────────────

	[Fact]
	public void ElementTrigger_property_applies_setter_when_condition_matches()
	{
		var el = new TEl();
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "triggered" });
		el.Triggers.Add(trigger);
		el.AttachElementTriggers();

		el.Flag = true;

		Assert.Equal("triggered", el.Tag2);
	}

	[Fact]
	public void ElementTrigger_property_reverts_setter_when_condition_stops()
	{
		var el = new TEl();
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "on" });
		el.Triggers.Add(trigger);
		el.AttachElementTriggers();

		el.Flag = true;
		el.Flag = false;

		Assert.Null(el.Tag2);
	}

	[Fact]
	public void ElementTrigger_property_local_value_beats_trigger_setter()
	{
		var el = new TEl();
		el.Tag2 = "local";

		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "triggered" });
		el.Triggers.Add(trigger);
		el.AttachElementTriggers();

		el.Flag = true;

		// Local value (D6) beats trigger layer (D8).
		Assert.Equal("local", el.Tag2);
	}

	[Fact]
	public void ElementTrigger_property_not_active_before_attach()
	{
		var el = new TEl();
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "triggered" });
		el.Triggers.Add(trigger);
		// AttachElementTriggers NOT called yet.

		el.Flag = true;

		// Trigger is not watching yet — setter must not be applied.
		Assert.Null(el.Tag2);
	}

	[Fact]
	public void ElementTrigger_property_detach_stops_watching()
	{
		var el = new TEl();
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "on" });
		el.Triggers.Add(trigger);
		el.AttachElementTriggers();

		el.Flag = true;
		Assert.Equal("on", el.Tag2);

		el.DetachElementTriggers();
		// After detach the trigger setter must be removed.
		Assert.Null(el.Tag2);

		// Flipping the flag after detach must have no effect.
		el.Flag = false;
		el.Flag = true;
		Assert.Null(el.Tag2);
	}

	// ── Event trigger at element level ────────────────────────────────────────

	[Fact]
	public void ElementTrigger_event_runs_action_when_event_fires()
	{
		var el = new TEl();
		var et = new EventTrigger { RoutedEvent = TEl.PingEvent };
		et.Actions.Add(new SetPropertyAction { Property = TEl.TagProperty, Value = "pinged" });
		el.Triggers.Add(et);
		el.AttachElementTriggers();

		el.FirePing();

		Assert.Equal("pinged", el.Tag2);
	}

	[Fact]
	public void ElementTrigger_event_detach_stops_handler()
	{
		var el = new TEl();
		int count = 0;
		el.AddChangeHook(TEl.TagProperty, (d, e) => count++);

		var et = new EventTrigger { RoutedEvent = TEl.PingEvent };
		et.Actions.Add(new SetPropertyAction { Property = TEl.TagProperty, Value = "x" });
		el.Triggers.Add(et);
		el.AttachElementTriggers();
		el.FirePing();
		Assert.Equal(1, count);

		el.DetachElementTriggers();
		el.FirePing();
		// Handler removed — count must stay at 1.
		Assert.Equal(1, count);
	}
}
