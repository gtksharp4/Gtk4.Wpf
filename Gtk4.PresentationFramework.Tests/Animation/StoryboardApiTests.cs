using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Animation.AnimationTestHelpers;

namespace Gtk4.PresentationFramework.Tests.Animation;

public class StoryboardApiTests
{
	static (Storyboard sb, AnimTarget el, DoubleAnimation anim) Build(string path = "(AnimTarget.AmountX)")
	{
		var el = new AnimTarget();
		var anim = Linear(0, 100);
		var sb = new Storyboard();
		sb.Children.Add(anim);
		Storyboard.SetTargetProperty(anim, new PropertyPath(AnimTarget.AmountProperty));
		return (sb, el, anim);
	}

	[Fact]
	public void Attached_properties_round_trip_through_the_wpf_statics()
	{
		var anim = Linear(0, 1);
		var el = new AnimTarget();
		Storyboard.SetTargetName(anim, "x");
		Storyboard.SetTargetProperty(anim, new PropertyPath("Opacity"));
		Storyboard.SetTarget(anim, el);
		Assert.Equal("x", Storyboard.GetTargetName(anim));
		Assert.Equal("Opacity", Storyboard.GetTargetProperty(anim)!.Path);
		Assert.Same(el, Storyboard.GetTarget(anim));
		Assert.Throws<ArgumentNullException>(() => Storyboard.SetTargetName(null!, "x"));
	}

	[Fact]
	public void Generated_code_instance_shims_still_compile_and_work()
	{
		var sb = new Storyboard();
		var anim = Linear(0, 1);
		sb.SetTargetName(anim, "box");
		sb.SetTargetPropertyName(anim, "(UIElement.Opacity)");
		sb.Children.Add(anim);
		Assert.Equal("box", sb.GetTargetName(anim));
		Assert.Equal("(UIElement.Opacity)", sb.GetTargetPropertyName(anim));
		Assert.Equal("box", Storyboard.GetTargetName(anim));
	}

	[Fact]
	public void Controllable_begin_exposes_time_state_and_progress()
	{
		var (sb, el, _) = Build();
		Assert.Null(sb.GetCurrentTime(el));
		sb.Begin(el, true);
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(0.5));
		Assert.Equal(50.0, el.Amount, 6);
		Assert.Equal(TimeSpan.FromSeconds(0.5), sb.GetCurrentTime(el));
		Assert.Equal(ClockState.Active, sb.GetCurrentState(el));
		Assert.Equal(0.5, sb.GetCurrentProgress(el)!.Value, 6);
		sb.Stop(el);
		Assert.Equal(10.0, el.Amount);
		Assert.Equal(ClockState.Stopped, sb.GetCurrentState(el));
		sb.Remove(el);
		Assert.Null(sb.GetCurrentState(el));
	}

	[Fact]
	public void Controllable_methods_are_no_ops_on_a_non_controllable_begin()
	{
		var (sb, el, _) = Build();
		sb.Begin(el);
		var clock = sb.ActiveClocks[0];
		clock.Tick(TimeSpan.FromSeconds(0.5));
		sb.Pause(el);
		sb.Seek(el, TimeSpan.FromSeconds(0.9), TimeSeekOrigin.BeginTime);
		sb.Stop(el);
		Assert.False(clock.IsPaused);
		Assert.Equal(50.0, el.Amount, 6);
		Assert.Null(sb.GetCurrentTime(el));
		Assert.Null(sb.GetCurrentState(el));
		Assert.Null(sb.GetCurrentProgress(el));
	}

	[Fact]
	public void Seek_pause_resume_and_skip_to_fill_work_on_a_controllable_begin()
	{
		var (sb, el, _) = Build();
		sb.Begin(el, true);
		sb.Seek(el, TimeSpan.FromSeconds(0.25), TimeSeekOrigin.BeginTime);
		Assert.Equal(25.0, el.Amount, 6);
		sb.Pause(el);
		Assert.True(sb.GetIsPaused(el));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(0.5));
		Assert.Equal(25.0, el.Amount, 6);
		sb.Resume(el);
		sb.SkipToFill(el);
		Assert.Equal(100.0, el.Amount, 6);
		Assert.Equal(ClockState.Filling, sb.GetCurrentState(el));
		sb.Seek(el, TimeSpan.FromSeconds(-0.5), TimeSeekOrigin.Duration);
		Assert.Equal(50.0, el.Amount, 6);
		sb.Remove(el);
		Assert.Equal(10.0, el.Amount);
	}

	[Fact]
	public void Begin_with_compose_layers_over_a_running_animation()
	{
		var (sb, el, _) = Build();
		el.BeginAnimation(AnimTarget.AmountProperty, Linear(0, 100));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(1));
		var by = new Storyboard();
		var add = new DoubleAnimation { By = 5, Duration = TimeSpan.FromSeconds(1) };
		by.Children.Add(add);
		Storyboard.SetTargetProperty(add, new PropertyPath(AnimTarget.AmountProperty));
		by.Begin(el, HandoffBehavior.Compose, true);
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(1));
		Assert.Equal(105.0, el.Amount, 6);
		by.Remove(el);
		el.BeginAnimation(AnimTarget.AmountProperty, null);
	}

	[Fact]
	public void Target_and_TargetName_resolve_the_animated_object()
	{
		var root = new AnimTarget();
		var other = new AnimTarget();
		root.RegisterName("other", other);
		var sb = new Storyboard();
		var byName = Linear(0, 40);
		Storyboard.SetTargetName(byName, "other");
		Storyboard.SetTargetProperty(byName, new PropertyPath(AnimTarget.AmountProperty));
		sb.Children.Add(byName);
		sb.Begin(root, true);
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(0.5));
		Assert.Equal(20.0, other.Amount, 6);
		Assert.Equal(10.0, root.Amount);
		sb.Remove(root);

		var direct = new Storyboard();
		var byTarget = Linear(0, 40);
		Storyboard.SetTarget(byTarget, other);
		Storyboard.SetTargetProperty(byTarget, new PropertyPath(AnimTarget.AmountProperty));
		direct.Children.Add(byTarget);
		direct.Begin(root, true);
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(1));
		Assert.Equal(40.0, other.Amount, 6);
		direct.Remove(root);
	}

	[Fact]
	public void Unknown_target_name_throws_naming_the_path()
	{
		var (sb, el, anim) = Build();
		Storyboard.SetTargetName(anim, "missing");
		var ex = Assert.Throws<InvalidOperationException>(() => sb.Begin(el));
		Assert.Contains("missing", ex.Message);
	}

	[Fact]
	public void Storyboard_clone_copies_children_and_attached_values()
	{
		var (sb, _, anim) = Build();
		Storyboard.SetTargetName(anim, "n");
		var clone = (Storyboard)sb.Clone();
		Assert.Single(clone.Children);
		Assert.Equal("n", Storyboard.GetTargetName(clone.Children[0]));
		Assert.NotSame(anim, clone.Children[0]);
	}
}
