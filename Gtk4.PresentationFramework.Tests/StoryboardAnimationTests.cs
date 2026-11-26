using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Round-9A: Duration/RepeatBehavior converters + VSM storyboard wiring.
/// Every test fails without the matching feature.
/// Pure (no GTK loop).
/// </summary>
public class StoryboardAnimationTests
{
	// ── Test-local FrameworkElement with animatable DP ───────────────────────

	sealed class AnimTarget : FrameworkElement
	{
		public static readonly DependencyProperty OpacityDpProperty =
			DependencyProperty.Register("R9Opacity", typeof(double), typeof(AnimTarget),
				new PropertyMetadata(1.0));

		public double OpacityDp
		{
			get => (double)(GetValue(OpacityDpProperty) ?? 1.0);
			set => SetValue(OpacityDpProperty, value);
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	// ── Duration converter ───────────────────────────────────────────────────

	[Fact]
	public void Duration_TimeSpan_hms_parses()
	{
		var d = AnimationConverters.ParseDuration("0:0:1");
		Assert.True(d.HasTimeSpan);
		Assert.Equal(TimeSpan.FromSeconds(1), d.TimeSpan);
	}

	[Fact]
	public void Duration_TimeSpan_decimal_seconds_parses()
	{
		var d = AnimationConverters.ParseDuration("0:0:0.5");
		Assert.True(d.HasTimeSpan);
		Assert.Equal(TimeSpan.FromMilliseconds(500), d.TimeSpan);
	}

	[Fact]
	public void Duration_Forever_parses()
	{
		var d = AnimationConverters.ParseDuration("Forever");
		Assert.Equal(Duration.Forever, d);
	}

	[Fact]
	public void Duration_Automatic_parses()
	{
		var d = AnimationConverters.ParseDuration("Automatic");
		Assert.Equal(Duration.Automatic, d);
	}

	[Fact]
	public void Duration_bad_input_throws_FormatException()
	{
		Assert.Throws<FormatException>(() => AnimationConverters.ParseDuration("notaduration"));
	}

	// ── RepeatBehavior converter ─────────────────────────────────────────────

	[Fact]
	public void RepeatBehavior_Forever_parses()
	{
		var rb = AnimationConverters.ParseRepeatBehavior("Forever");
		Assert.Equal(RepeatBehavior.Forever, rb);
	}

	[Fact]
	public void RepeatBehavior_integer_count_parses()
	{
		var rb = AnimationConverters.ParseRepeatBehavior("3x");
		Assert.True(rb.HasCount);
		Assert.Equal(3.0, rb.Count, precision: 10);
	}

	[Fact]
	public void RepeatBehavior_decimal_count_parses()
	{
		var rb = AnimationConverters.ParseRepeatBehavior("2.5x");
		Assert.True(rb.HasCount);
		Assert.Equal(2.5, rb.Count, precision: 10);
	}

	[Fact]
	public void RepeatBehavior_duration_form_parses()
	{
		var rb = AnimationConverters.ParseRepeatBehavior("0:0:5");
		Assert.True(rb.HasDuration);
		Assert.Equal(TimeSpan.FromSeconds(5), rb.Duration);
	}

	[Fact]
	public void RepeatBehavior_bad_input_throws_FormatException()
	{
		Assert.Throws<FormatException>(() => AnimationConverters.ParseRepeatBehavior("bad"));
	}

	// ── XamlValueConverter routes Duration / RepeatBehavior ──────────────────

	[Fact]
	public void XamlValueConverter_converts_Duration_via_ConvertByType()
	{
		var result = XamlValueConverter.ConvertByType(typeof(Duration), "0:0:2");
		Assert.IsType<Duration>(result);
		var d = (Duration)result!;
		Assert.True(d.HasTimeSpan);
		Assert.Equal(TimeSpan.FromSeconds(2), d.TimeSpan);
	}

	[Fact]
	public void XamlValueConverter_converts_RepeatBehavior_via_ConvertByType()
	{
		var result = XamlValueConverter.ConvertByType(typeof(RepeatBehavior), "Forever");
		Assert.IsType<RepeatBehavior>(result);
		Assert.Equal(RepeatBehavior.Forever, (RepeatBehavior)result!);
	}

	// ── VisualStateManager storyboard wiring ─────────────────────────────────

	sealed class VsmControl : Control
	{
		public static readonly DependencyProperty R9ValProperty =
			DependencyProperty.Register("R9Val", typeof(double), typeof(VsmControl),
				new PropertyMetadata(0.0));

		public double R9Val
		{
			get => (double)(GetValue(R9ValProperty) ?? 0.0);
			set => SetValue(R9ValProperty, value);
		}

		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	/// <summary>
	/// When GoToState enters a state that has a Storyboard and useTransitions=true,
	/// the storyboard clocks become active.
	/// </summary>
	[Fact]
	public void GoToState_with_useTransitions_starts_storyboard()
	{
		var ctrl = new VsmControl();

		var anim = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(1) };
		var sb = new Storyboard();
		sb.Children.Add(anim);
		sb.SetTargetProperty(anim, VsmControl.R9ValProperty);

		var state = new VisualState { Name = "Active", Storyboard = sb };
		var group = new VisualStateGroup { Name = "G" };
		group.States.Add(state);
		VisualStateManager.SetVisualStateGroups(ctrl, new List<VisualStateGroup> { group });

		VisualStateManager.GoToState(ctrl, "Active", useTransitions: true);

		// Storyboard.Begin was called → clocks created.
		Assert.Equal(1, sb.ActiveClocks.Count);
		Assert.Equal(ClockState.Active, sb.ActiveClocks[0].CurrentState);
	}

	/// <summary>
	/// useTransitions=false must NOT start the storyboard.
	/// </summary>
	[Fact]
	public void GoToState_without_useTransitions_does_not_start_storyboard()
	{
		var ctrl = new VsmControl();

		var anim = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(1) };
		var sb = new Storyboard();
		sb.Children.Add(anim);
		sb.SetTargetProperty(anim, VsmControl.R9ValProperty);

		var state = new VisualState { Name = "Idle", Storyboard = sb };
		var group = new VisualStateGroup { Name = "G" };
		group.States.Add(state);
		VisualStateManager.SetVisualStateGroups(ctrl, new List<VisualStateGroup> { group });

		VisualStateManager.GoToState(ctrl, "Idle", useTransitions: false);

		// No clocks created when useTransitions=false.
		Assert.Equal(0, sb.ActiveClocks.Count);
	}

	/// <summary>
	/// Transitioning away from a state with a running storyboard stops it.
	/// Clocks are cleared from ActiveClocks on Stop(); capture the clock first.
	/// </summary>
	[Fact]
	public void GoToState_stops_previous_state_storyboard_on_exit()
	{
		var ctrl = new VsmControl();

		var anim = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(10) };
		var sb = new Storyboard();
		sb.Children.Add(anim);
		sb.SetTargetProperty(anim, VsmControl.R9ValProperty);

		var stateA = new VisualState { Name = "A", Storyboard = sb };
		var stateB = new VisualState { Name = "B" };
		var group = new VisualStateGroup { Name = "G" };
		group.States.Add(stateA);
		group.States.Add(stateB);
		VisualStateManager.SetVisualStateGroups(ctrl, new List<VisualStateGroup> { group });

		// Enter A with transitions → storyboard starts.
		VisualStateManager.GoToState(ctrl, "A", useTransitions: true);
		Assert.Equal(1, sb.ActiveClocks.Count);
		var clock = sb.ActiveClocks[0]; // capture before Stop clears the list
		Assert.Equal(ClockState.Active, clock.CurrentState);

		// Leave A → go to B → storyboard stops (clears ActiveClocks).
		VisualStateManager.GoToState(ctrl, "B", useTransitions: false);
		Assert.Equal(0, sb.ActiveClocks.Count);  // cleared by Stop()
		Assert.Equal(ClockState.Stopped, clock.CurrentState); // captured clock is stopped
	}

	/// <summary>
	/// A running storyboard actually animates the property when ticked.
	/// </summary>
	[Fact]
	public void GoToState_storyboard_animates_property_when_ticked()
	{
		var ctrl = new VsmControl();
		ctrl.R9Val = 0.0;

		var anim = new DoubleAnimation { From = 0, To = 10, Duration = TimeSpan.FromSeconds(1) };
		var sb = new Storyboard();
		sb.Children.Add(anim);
		sb.SetTargetProperty(anim, VsmControl.R9ValProperty);

		var state = new VisualState { Name = "Run", Storyboard = sb };
		var group = new VisualStateGroup { Name = "G" };
		group.States.Add(state);
		VisualStateManager.SetVisualStateGroups(ctrl, new List<VisualStateGroup> { group });

		VisualStateManager.GoToState(ctrl, "Run", useTransitions: true);

		// Tick 0.5 s → value at halfway.
		sb.ActiveClocks[0].Tick(TimeSpan.FromSeconds(0.5));
		Assert.Equal(5.0, ctrl.R9Val, precision: 10);
	}
}