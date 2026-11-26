using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for the animation engine (Round 8B).
/// All tests use the deterministic <see cref="AnimationClock.Tick"/> seam;
/// no real GLib main loop is required.
/// </summary>
public class AnimationTests
{
	// ── Test-local FrameworkElement subclasses ───────────────────────────────

	sealed class DoubleTarget : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(
				"AnimTestDoubleValue", typeof(double), typeof(DoubleTarget),
				new PropertyMetadata(0.0));

		public double Value
		{
			get => (double)(GetValue(ValueProperty) ?? 0.0);
			set => SetValue(ValueProperty, value);
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	sealed class ColorTarget : FrameworkElement
	{
		public static readonly DependencyProperty ColorProperty =
			DependencyProperty.Register(
				"AnimTestColor", typeof(Color), typeof(ColorTarget),
				new PropertyMetadata(default(Color)));

		public Color Color
		{
			get => GetValue(ColorProperty) is Color c ? c : default;
			set => SetValue(ColorProperty, value);
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	sealed class TriggerTarget : FrameworkElement
	{
		// "new" suppresses CS0108: this test-local DP uses registration name
		// "AnimTestOpacity" to avoid colliding with UIElement.OpacityProperty.
		public new static readonly DependencyProperty OpacityProperty =
			DependencyProperty.Register(
				"AnimTestOpacity", typeof(double), typeof(TriggerTarget),
				new PropertyMetadata(1.0));

		public new static readonly RoutedEvent LoadedEvent =
			EventManager.RegisterRoutedEvent(
				"AnimTestLoaded", RoutingStrategy.Bubble,
				typeof(RoutedEventHandler), typeof(TriggerTarget));

		public new double Opacity
		{
			get => (double)(GetValue(OpacityProperty) ?? 1.0);
			set => SetValue(OpacityProperty, value);
		}

		public void FireLoaded() => RaiseEvent(new RoutedEventArgs(LoadedEvent));

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	// ── Helper: build a clock directly (bypasses GLib scheduler) ─────────────

	static AnimationClock MakeClock(
		AnimationTimeline anim,
		DependencyObject target,
		DependencyProperty dp) =>
		new AnimationClock(anim, target, dp);

	static double GetDouble(DependencyObject obj, DependencyProperty dp) =>
		(double)(obj.GetValue(dp) ?? 0.0);

	// ── DoubleAnimation basic interpolation ──────────────────────────────────

	[Fact]
	public void DoubleAnimation_at_near_t0_is_near_From()
	{
		var target = new DoubleTarget();
		var anim = new DoubleAnimation
		{
			From = 0, To = 10,
			Duration = TimeSpan.FromSeconds(1)
		};
		var clock = MakeClock(anim, target, DoubleTarget.ValueProperty);

		// Very small tick → value should be close to From (0).
		clock.Tick(TimeSpan.FromMilliseconds(1));
		Assert.True(GetDouble(target, DoubleTarget.ValueProperty) < 0.1);
	}

	[Fact]
	public void DoubleAnimation_at_t_half_equals_5()
	{
		var target = new DoubleTarget();
		var anim = new DoubleAnimation
		{
			From = 0, To = 10,
			Duration = TimeSpan.FromSeconds(1)
		};
		var clock = MakeClock(anim, target, DoubleTarget.ValueProperty);

		clock.Tick(TimeSpan.FromSeconds(0.5));

		Assert.Equal(5.0, GetDouble(target, DoubleTarget.ValueProperty), precision: 10);
	}

	[Fact]
	public void DoubleAnimation_at_t1_equals_To_and_state_is_Filling()
	{
		var target = new DoubleTarget();
		var anim = new DoubleAnimation
		{
			From = 0, To = 10,
			Duration = TimeSpan.FromSeconds(1)
		};
		var clock = MakeClock(anim, target, DoubleTarget.ValueProperty);

		clock.Tick(TimeSpan.FromSeconds(1.0));

		Assert.Equal(10.0, GetDouble(target, DoubleTarget.ValueProperty), precision: 10);
		Assert.Equal(ClockState.Filling, clock.CurrentState);
	}

	[Fact]
	public void DoubleAnimation_HoldEnd_keeps_animation_value_after_complete()
	{
		var target = new DoubleTarget();
		target.Value = 42.0;
		var anim = new DoubleAnimation
		{
			From = 0, To = 10,
			Duration = TimeSpan.FromSeconds(1),
			FillBehavior = FillBehavior.HoldEnd
		};
		var clock = MakeClock(anim, target, DoubleTarget.ValueProperty);

		clock.Tick(TimeSpan.FromSeconds(2));

		// Animation layer holds 10; local value 42 is shadowed.
		Assert.Equal(10.0, target.Value, precision: 10);
		Assert.Equal(ClockState.Filling, clock.CurrentState);
	}

	[Fact]
	public void DoubleAnimation_FillBehavior_Stop_reverts_to_local_value()
	{
		var target = new DoubleTarget();
		target.Value = 42.0;
		var anim = new DoubleAnimation
		{
			From = 0, To = 10,
			Duration = TimeSpan.FromSeconds(1),
			FillBehavior = FillBehavior.Stop
		};
		var clock = MakeClock(anim, target, DoubleTarget.ValueProperty);

		clock.Tick(TimeSpan.FromSeconds(2));

		// FillBehavior.Stop clears animation layer → local value returns.
		Assert.Equal(42.0, target.Value, precision: 10);
		Assert.Equal(ClockState.Stopped, clock.CurrentState);
	}

	// ── Animation layer beats local ──────────────────────────────────────────

	[Fact]
	public void Animation_layer_beats_local_value_while_running()
	{
		var target = new DoubleTarget();
		target.Value = 99.0;

		var anim = new DoubleAnimation
		{
			From = 0, To = 10,
			Duration = TimeSpan.FromSeconds(1)
		};
		var clock = MakeClock(anim, target, DoubleTarget.ValueProperty);
		clock.Tick(TimeSpan.FromSeconds(0.5));

		// Animation (5.0) beats local (99.0).
		Assert.Equal(5.0, target.Value, precision: 10);
	}

	[Fact]
	public void After_Stop_and_clear_local_value_returns()
	{
		var target = new DoubleTarget();
		target.Value = 77.0;

		var anim = new DoubleAnimation
		{
			From = 0, To = 10,
			Duration = TimeSpan.FromSeconds(1),
			FillBehavior = FillBehavior.Stop
		};
		var clock = MakeClock(anim, target, DoubleTarget.ValueProperty);
		clock.Tick(TimeSpan.FromSeconds(0.5));

		Assert.Equal(5.0, target.Value, precision: 10); // animation wins

		// Complete with Stop fill → clears animation layer.
		clock.Tick(TimeSpan.FromSeconds(1.0)); // total 1.5s → past end

		Assert.Equal(77.0, target.Value, precision: 10); // local returns
	}

	// ── AutoReverse ──────────────────────────────────────────────────────────

	[Fact]
	public void AutoReverse_at_t1_5s_value_equals_5()
	{
		var target = new DoubleTarget();
		var anim = new DoubleAnimation
		{
			From = 0, To = 10,
			Duration = TimeSpan.FromSeconds(1),
			AutoReverse = true,
			RepeatBehavior = new RepeatBehavior(1)
		};
		var clock = MakeClock(anim, target, DoubleTarget.ValueProperty);

		// At t=1.5s (halfway through the reverse phase) value should be 5.
		clock.Tick(TimeSpan.FromSeconds(1.5));
		Assert.Equal(5.0, GetDouble(target, DoubleTarget.ValueProperty), precision: 10);
	}

	[Fact]
	public void AutoReverse_completes_at_From_under_HoldEnd()
	{
		var target = new DoubleTarget();
		var anim = new DoubleAnimation
		{
			From = 0, To = 10,
			Duration = TimeSpan.FromSeconds(1),
			AutoReverse = true,
			FillBehavior = FillBehavior.HoldEnd,
			RepeatBehavior = new RepeatBehavior(1)
		};
		var clock = MakeClock(anim, target, DoubleTarget.ValueProperty);

		// 2.5s > one forward (1s) + one reverse (1s) = 2s → past end.
		clock.Tick(TimeSpan.FromSeconds(2.5));

		Assert.Equal(0.0, GetDouble(target, DoubleTarget.ValueProperty), precision: 10);
		Assert.Equal(ClockState.Filling, clock.CurrentState);
	}

	// ── RepeatBehavior.Forever ───────────────────────────────────────────────

	[Fact]
	public void RepeatBehavior_Forever_keeps_clock_Active_past_duration()
	{
		var target = new DoubleTarget();
		var anim = new DoubleAnimation
		{
			From = 0, To = 10,
			Duration = TimeSpan.FromSeconds(1),
			RepeatBehavior = RepeatBehavior.Forever
		};
		var clock = MakeClock(anim, target, DoubleTarget.ValueProperty);

		// Tick to 3.5s → 3 full iterations + 0.5 into the 4th.
		clock.Tick(TimeSpan.FromSeconds(3.5));

		Assert.Equal(ClockState.Active, clock.CurrentState);
		// progress at 3.5s: 3.5 mod 1.0 = 0.5 → value = 5.
		Assert.Equal(5.0, GetDouble(target, DoubleTarget.ValueProperty), precision: 10);
	}

	// ── ColorAnimation ───────────────────────────────────────────────────────

	[Fact]
	public void ColorAnimation_interpolates_channels_at_half()
	{
		var target = new ColorTarget();
		var anim = new ColorAnimation
		{
			From = Color.FromArgb(0, 0, 0, 0),
			To   = Color.FromArgb(255, 100, 200, 50),
			Duration = TimeSpan.FromSeconds(1)
		};
		var clock = MakeClock(anim, target, ColorTarget.ColorProperty);

		clock.Tick(TimeSpan.FromSeconds(0.5));

		var c = target.Color;
		// Each channel should be ~50% between From and To.
		Assert.InRange(c.A, (byte)127, (byte)129);
		Assert.InRange(c.R, (byte)49, (byte)51);
		Assert.InRange(c.G, (byte)99, (byte)101);
		Assert.InRange(c.B, (byte)24, (byte)26);
	}

	// ── BeginStoryboard wires into EventTrigger ──────────────────────────────

	[Fact]
	public void BeginStoryboard_starts_animation_on_EventTrigger_fire()
	{
		var target = new TriggerTarget();
		target.Opacity = 1.0;

		var anim = new DoubleAnimation
		{
			From = 1.0, To = 0.0,
			Duration = TimeSpan.FromSeconds(1)
		};

		var sb = new Storyboard();
		sb.Children.Add(anim);
		sb.SetTargetProperty(anim, TriggerTarget.OpacityProperty);

		var et = new EventTrigger { RoutedEvent = TriggerTarget.LoadedEvent };
		et.Actions.Add(new BeginStoryboard { Storyboard = sb });

		var style = new Style(typeof(TriggerTarget));
		style.Triggers.Add(et);
		target.Style = style;

		// Before event: no animation, local value visible.
		Assert.Equal(1.0, target.Opacity, precision: 10);

		// Fire event → BeginStoryboard.Invoke → Storyboard.Begin.
		target.FireLoaded();

		// Tick the clock manually — GLib ticker is not running.
		Assert.Equal(1, sb.ActiveClocks.Count);
		sb.ActiveClocks[0].Tick(TimeSpan.FromSeconds(0.5));

		// At 0.5s: 1.0→0.0, progress=0.5 → value≈0.5.
		Assert.Equal(0.5, target.Opacity, precision: 10);
	}

	[Fact]
	public void BeginStoryboard_null_storyboard_is_a_noop()
	{
		var target = new TriggerTarget();
		var action = new BeginStoryboard { Storyboard = null };
		// Must not throw.
		action.Invoke(target);
	}

	// ── T6: Duration.Automatic resolves to 1 second ──────────────────────────

	[Fact]
	public void T6_DoubleAnimation_no_Duration_set_ticked_1s_reaches_To()
	{
		// Duration defaults to Automatic.  Without the fix the clock stays at progress=0.
		var target = new DoubleTarget();
		var anim = new DoubleAnimation { From = 0, To = 10 }; // no Duration → Automatic

		Assert.Equal(Duration.Automatic, anim.Duration); // precondition

		var clock = MakeClock(anim, target, DoubleTarget.ValueProperty);
		clock.Tick(TimeSpan.FromSeconds(1.0));

		// 1 s elapsed == the resolved 1-second duration → animation complete, value = To.
		Assert.Equal(10.0, GetDouble(target, DoubleTarget.ValueProperty), precision: 10);
		Assert.Equal(ClockState.Filling, clock.CurrentState);
	}

	[Fact]
	public void T6_DoubleAnimation_no_Duration_at_half_second_is_half_way()
	{
		var target = new DoubleTarget();
		var anim = new DoubleAnimation { From = 0, To = 10 };
		var clock = MakeClock(anim, target, DoubleTarget.ValueProperty);

		clock.Tick(TimeSpan.FromSeconds(0.5));

		// 0.5 s / 1 s = 50% → value ≈ 5.
		Assert.Equal(5.0, GetDouble(target, DoubleTarget.ValueProperty), precision: 10);
		Assert.Equal(ClockState.Active, clock.CurrentState);
	}
}