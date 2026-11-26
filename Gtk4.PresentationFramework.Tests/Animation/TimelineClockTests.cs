using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Animation.AnimationTestHelpers;

namespace Gtk4.PresentationFramework.Tests.Animation;

public class TimelineClockTests
{
	[Fact]
	public void Timeline_defaults_match_wpf()
	{
		var t = new DoubleAnimation();
		Assert.Equal(TimeSpan.Zero, t.BeginTime);
		Assert.Equal(Duration.Automatic, t.Duration);
		Assert.False(t.AutoReverse);
		Assert.Equal(new RepeatBehavior(1), t.RepeatBehavior);
		Assert.Equal(FillBehavior.HoldEnd, t.FillBehavior);
		Assert.Equal(1.0, t.SpeedRatio);
		Assert.Equal(0.0, t.AccelerationRatio);
		Assert.Equal(0.0, t.DecelerationRatio);
		Assert.Null(t.Name);
		Assert.Equal(typeof(double), t.TargetPropertyType);
		Assert.True(t.IsDestinationDefault);
	}

	[Fact]
	public void Timing_properties_are_dependency_properties_and_validated()
	{
		var t = new DoubleAnimation { BeginTime = null, SpeedRatio = 2, Name = "n" };
		Assert.Null(t.GetValue(Timeline.BeginTimeProperty));
		Assert.Equal(2.0, t.GetValue(Timeline.SpeedRatioProperty));
		Assert.Throws<ArgumentException>(() => t.SpeedRatio = 0);
		Assert.Throws<ArgumentException>(() => t.AccelerationRatio = 1.5);
		Assert.Throws<ArgumentException>(() => t.DecelerationRatio = -0.1);
	}

	[Fact]
	public void Clock_reports_progress_time_iteration_and_state()
	{
		var clock = Linear(0, 10, 2).CreateClock();
		Assert.Null(clock.CurrentTime);
		Assert.Null(clock.CurrentProgress);
		Tick(clock, 0.5);
		Assert.Equal(0.25, clock.CurrentProgress!.Value, 6);
		Assert.Equal(TimeSpan.FromSeconds(0.5), clock.CurrentTime);
		Assert.Equal(1, clock.CurrentIteration);
		Assert.Equal(ClockState.Active, clock.CurrentState);
		Tick(clock, 2);
		Assert.Equal(ClockState.Filling, clock.CurrentState);
		Assert.Equal(1.0, clock.CurrentProgress);
	}

	[Fact]
	public void BeginTime_delays_the_clock_and_null_never_starts_it()
	{
		var delayed = Linear(0, 10);
		delayed.BeginTime = TimeSpan.FromSeconds(1);
		var clock = delayed.CreateClock();
		Tick(clock, 0.5);
		Assert.Null(clock.CurrentTime);
		Tick(clock, 1);
		Assert.Equal(0.5, clock.CurrentProgress!.Value, 6);

		var never = Linear(0, 10);
		never.BeginTime = null;
		var c2 = never.CreateClock();
		Tick(c2, 1);
		Assert.Equal(ClockState.Stopped, c2.CurrentState);
	}

	[Fact]
	public void AutoReverse_and_repeat_drive_progress_and_iteration()
	{
		var anim = Linear(0, 10);
		anim.AutoReverse = true;
		anim.RepeatBehavior = new RepeatBehavior(2);
		var clock = anim.CreateClock();
		Tick(clock, 1.5);
		Assert.Equal(0.5, clock.CurrentProgress!.Value, 6);
		Tick(clock, 1);
		Assert.Equal(2, clock.CurrentIteration);
		Assert.Equal(0.5, clock.CurrentProgress!.Value, 6);
		Tick(clock, 1.5);
		Assert.Equal(ClockState.Filling, clock.CurrentState);
		Assert.Equal(0.0, clock.CurrentProgress);
	}

	[Fact]
	public void Acceleration_and_deceleration_ratios_shape_progress()
	{
		var anim = Linear(0, 10);
		anim.AccelerationRatio = 0.5;
		anim.DecelerationRatio = 0.5;
		var clock = anim.CreateClock();
		Tick(clock, 0.25);
		Assert.Equal(0.125, clock.CurrentProgress!.Value, 6);
		Tick(clock, 0.25);
		Assert.Equal(0.5, clock.CurrentProgress!.Value, 6);
	}

	[Fact]
	public void SpeedRatio_scales_the_clock()
	{
		var anim = Linear(0, 10, 2);
		anim.SpeedRatio = 2;
		var clock = anim.CreateClock();
		Tick(clock, 0.5);
		Assert.Equal(0.5, clock.CurrentProgress!.Value, 6);
	}

	[Fact]
	public void Events_use_the_clock_as_sender_in_wpf_order()
	{
		var anim = Linear(0, 10);
		var clock = anim.CreateClock();
		var log = new List<string>();
		object? sender = null;
		clock.CurrentStateInvalidated += (s, e) => { sender = s; log.Add("state"); };
		clock.CurrentTimeInvalidated += (s, e) => log.Add("time");
		clock.Completed += (s, e) => log.Add("completed");
		anim.Completed += (s, e) => log.Add("timeline-completed");
		Tick(clock, 0.5);
		Tick(clock, 1);
		Assert.Same(clock, sender);
		Assert.Equal(new[] { "time", "time", "state", "completed", "timeline-completed" }, log.ToArray());
	}

	[Fact]
	public void Controller_seeks_pauses_skips_and_stops()
	{
		var clock = Linear(0, 10).CreateClock();
		var c = clock.Controller!;
		Tick(clock, 0.1);
		c.Seek(TimeSpan.FromSeconds(0.6), TimeSeekOrigin.BeginTime);
		Assert.Equal(0.6, clock.CurrentProgress!.Value, 6);
		c.Seek(TimeSpan.FromSeconds(-0.2), TimeSeekOrigin.Duration);
		Assert.Equal(0.8, clock.CurrentProgress!.Value, 6);
		c.Pause();
		Assert.True(clock.IsPaused);
		Tick(clock, 0.1);
		Assert.Equal(0.8, clock.CurrentProgress!.Value, 6);
		c.Resume();
		c.SkipToFill();
		Assert.Equal(ClockState.Filling, clock.CurrentState);
		c.Stop();
		Assert.Equal(ClockState.Stopped, clock.CurrentState);
		Assert.Null(clock.CurrentTime);
		c.Begin();
		Tick(clock, 0.25);
		Assert.Equal(0.25, clock.CurrentProgress!.Value, 6);
		c.SpeedRatio = 2;
		Tick(clock, 0.25);
		Assert.Equal(0.75, clock.CurrentProgress!.Value, 6);
		Assert.Throws<ArgumentException>(() => c.SpeedRatio = 0);
	}

	[Fact]
	public void Non_controllable_root_has_no_controller()
	{
		Assert.Null(Linear(0, 1).CreateClock(false).Controller);
		Assert.NotNull(Linear(0, 1).CreateClock().Controller);
	}

	[Fact]
	public void Clock_group_automatic_duration_is_the_longest_child()
	{
		var group = new ParallelTimeline();
		group.Children.Add(Linear(0, 1, 1));
		group.Children.Add(Linear(0, 1, 3));
		var clock = group.CreateClock();
		Assert.Equal(2, clock.Children.Count);
		Tick(clock, 2);
		Assert.Equal(ClockState.Active, clock.CurrentState);
		Assert.Equal(ClockState.Filling, clock.Children[0].CurrentState);
		Tick(clock, 1.5);
		Assert.Equal(ClockState.Filling, clock.CurrentState);
	}

	[Fact]
	public void Typed_base_subclass_runs_and_clones()
	{
		var el = new AnimTarget();
		var anim = new SquareAnimation { Scale = 40, Duration = TimeSpan.FromSeconds(1) };
		var clock = ((SquareAnimation)anim.Clone()).CreateClock();
		el.ApplyAnimationClock(AnimTarget.AmountProperty, clock);
		Tick(clock, 0.5);
		Assert.Equal(10.0 + 40 * 0.25, el.Amount, 6);
		Assert.Equal(typeof(double), anim.TargetPropertyType);
	}

	[Fact]
	public void Frozen_timeline_rejects_changes()
	{
		var anim = Linear(0, 1);
		anim.Freeze();
		Assert.Throws<InvalidOperationException>(() => anim.Duration = TimeSpan.FromSeconds(3));
	}

	[Fact]
	public void Color_and_thickness_animations_derive_from_typed_bases()
	{
		Assert.IsAssignableFrom<ColorAnimationBase>(new ColorAnimation());
		Assert.IsAssignableFrom<ThicknessAnimationBase>(new ThicknessAnimation());
		Assert.Equal(typeof(Thickness), new ThicknessAnimation().TargetPropertyType);
	}
}
