using System;
using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="Trigger.EnterActions"/>/<see cref="Trigger.ExitActions"/>
/// and the same on <see cref="DataTrigger"/> and <see cref="MultiTrigger"/>.
/// </summary>
public class TriggerEnterExitActionTests
{
	// ── Helpers ──────────────────────────────────────────────────────────────

	/// <summary>Records how many times it was invoked and by which target.</summary>
	sealed class RecordingAction : TriggerAction
	{
		public int InvokeCount { get; private set; }
		public List<FrameworkElement> Targets { get; } = new List<FrameworkElement>();

		internal override void Invoke(FrameworkElement target)
		{
			InvokeCount++;
			Targets.Add(target);
		}
	}

	sealed class TEl : FrameworkElement
	{
		public static readonly DependencyProperty FlagProperty =
			DependencyProperty.Register("EEFlag", typeof(bool), typeof(TEl),
				new PropertyMetadata(false));
		public static readonly DependencyProperty OutProperty =
			DependencyProperty.Register("EEOut", typeof(string), typeof(TEl),
				new PropertyMetadata(null));

		public bool Flag { get => (bool)GetValue(FlagProperty)!; set => SetValue(FlagProperty, value); }
		public string? Out { get => (string?)GetValue(OutProperty); set => SetValue(OutProperty, value); }

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	// ── Trigger ──────────────────────────────────────────────────────────────

	[Fact]
	public void Trigger_EnterActions_run_when_condition_becomes_active()
	{
		var enter = new RecordingAction();
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.EnterActions.Add(enter);
		style.Triggers.Add(trigger);
		el.Style = style;

		el.Flag = true;

		Assert.Equal(1, enter.InvokeCount);
	}

	[Fact]
	public void Trigger_ExitActions_run_when_condition_stops_being_active()
	{
		var exit = new RecordingAction();
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.ExitActions.Add(exit);
		style.Triggers.Add(trigger);
		el.Style = style;

		el.Flag = true;
		Assert.Equal(0, exit.InvokeCount); // not yet

		el.Flag = false;
		Assert.Equal(1, exit.InvokeCount);
	}

	[Fact]
	public void Trigger_EnterActions_do_not_run_when_condition_is_not_met()
	{
		var enter = new RecordingAction();
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.EnterActions.Add(enter);
		style.Triggers.Add(trigger);
		el.Style = style;

		// Flag stays false — enter should never fire.
		Assert.Equal(0, enter.InvokeCount);
	}

	// ── MultiTrigger ─────────────────────────────────────────────────────────

	sealed class TEl2 : FrameworkElement
	{
		public static readonly DependencyProperty AProperty =
			DependencyProperty.Register("MTEA2", typeof(bool), typeof(TEl2), new PropertyMetadata(false));
		public static readonly DependencyProperty BProperty =
			DependencyProperty.Register("MTEB2", typeof(bool), typeof(TEl2), new PropertyMetadata(false));
		public bool A { get => (bool)GetValue(AProperty)!; set => SetValue(AProperty, value); }
		public bool B { get => (bool)GetValue(BProperty)!; set => SetValue(BProperty, value); }
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void MultiTrigger_EnterActions_run_when_all_conditions_met()
	{
		var enter = new RecordingAction();
		var el = new TEl2();
		var style = new Style(typeof(TEl2));
		var mt = new MultiTrigger();
		mt.Conditions.Add(new Condition { Property = TEl2.AProperty, Value = true });
		mt.Conditions.Add(new Condition { Property = TEl2.BProperty, Value = true });
		mt.EnterActions.Add(enter);
		style.Triggers.Add(mt);
		el.Style = style;

		el.A = true;
		Assert.Equal(0, enter.InvokeCount); // only A is true

		el.B = true;
		Assert.Equal(1, enter.InvokeCount); // both true
	}

	[Fact]
	public void MultiTrigger_ExitActions_run_when_condition_breaks()
	{
		var exit = new RecordingAction();
		var el = new TEl2();
		var style = new Style(typeof(TEl2));
		var mt = new MultiTrigger();
		mt.Conditions.Add(new Condition { Property = TEl2.AProperty, Value = true });
		mt.Conditions.Add(new Condition { Property = TEl2.BProperty, Value = true });
		mt.ExitActions.Add(exit);
		style.Triggers.Add(mt);
		el.Style = style;

		el.A = true;
		el.B = true;
		Assert.Equal(0, exit.InvokeCount);

		el.B = false;
		Assert.Equal(1, exit.InvokeCount);
	}
}
