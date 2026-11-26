using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Animation.AnimationTestHelpers;

namespace Gtk4.PresentationFramework.Tests.Animation;

public class UIElementAnimationTests
{
	[Fact]
	public void BeginAnimation_runs_through_the_scheduler_and_animated_value_sits_above_local()
	{
		var el = new AnimTarget { Amount = 3 };
		el.BeginAnimation(AnimTarget.AmountProperty, Linear(0, 100));
		Assert.True(el.HasAnimatedProperties);
		Assert.Equal(3.0, el.Amount);
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(0.5));
		Assert.Equal(50.0, el.Amount, 6);
		Assert.Equal(3.0, el.GetAnimationBaseValue(AnimTarget.AmountProperty));
		el.BeginAnimation(AnimTarget.AmountProperty, null);
		Assert.Equal(3.0, el.Amount);
	}

	[Fact]
	public void ApplyAnimationClock_with_a_null_clock_removes_the_animation()
	{
		var el = new AnimTarget();
		var clock = Linear(0, 100).CreateClock();
		el.ApplyAnimationClock(AnimTarget.AmountProperty, clock);
		Tick(clock, 0.5);
		el.ApplyAnimationClock(AnimTarget.AmountProperty, null);
		Assert.Equal(10.0, el.Amount);
	}

	[Fact]
	public void FillBehavior_Stop_restores_the_base_value()
	{
		var el = new AnimTarget();
		var anim = Linear(0, 100);
		anim.FillBehavior = FillBehavior.Stop;
		var clock = anim.CreateClock();
		el.ApplyAnimationClock(AnimTarget.AmountProperty, clock);
		Tick(clock, 2);
		Assert.Equal(10.0, el.Amount);
	}

	[Fact]
	public void HoldEnd_keeps_the_end_value_while_filling()
	{
		var el = new AnimTarget();
		var clock = Linear(0, 100).CreateClock();
		el.ApplyAnimationClock(AnimTarget.AmountProperty, clock);
		Tick(clock, 2);
		Assert.Equal(ClockState.Filling, clock.CurrentState);
		Assert.Equal(100.0, el.Amount);
	}

	[Fact]
	public void Mistyped_animation_is_rejected()
	{
		var el = new AnimTarget();
		Assert.Throws<InvalidOperationException>(
			() => el.BeginAnimation(AnimTarget.AmountProperty, new ColorAnimation()));
	}

	[Fact]
	public void Animating_a_brush_color_re_runs_the_owner_property_callback()
	{
		var el = new AnimTarget();
		var brush = new SolidColorBrush(Colors.Red);
		el.SetValue(AnimTarget.FillProperty, brush);
		var before = el.FillCallbacks;
		var clock = new ColorAnimation(Colors.Red, Colors.Blue, TimeSpan.FromSeconds(1)).CreateClock();
		brush.ApplyAnimationClock(SolidColorBrush.ColorProperty, clock);
		Tick(clock, 0.5);
		Assert.True(el.FillCallbacks > before);
	}

	[Fact]
	public void Animating_the_render_transform_changes_its_matrix()
	{
		var el = new AnimTarget();
		var t = new TranslateTransform();
		el.RenderTransform = t;
		var clock = Linear(0, 40).CreateClock();
		t.ApplyAnimationClock(TranslateTransform.XProperty, clock);
		Tick(clock, 0.5);
		Assert.Equal(20.0, el.RenderTransform!.Value.OffsetX, 6);
	}
}
