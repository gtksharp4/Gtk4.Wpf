using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Animation.AnimationTestHelpers;

namespace Gtk4.PresentationFramework.Tests.Animation;

public class AnimatableTests
{
	[Fact]
	public void Transform_and_brush_are_animatable_freezables()
	{
		Assert.IsAssignableFrom<Animatable>(new TranslateTransform());
		Assert.IsAssignableFrom<Animatable>(new SolidColorBrush());
		Assert.IsAssignableFrom<IAnimatable>(new AnimTarget());
		Assert.IsAssignableFrom<Animatable>(new DoubleAnimation());
	}

	[Fact]
	public void BeginAnimation_animates_a_translate_transform_dp()
	{
		var t = new TranslateTransform();
		var clock = Linear(0, 100).CreateClock();
		t.ApplyAnimationClock(TranslateTransform.XProperty, clock);
		Tick(clock, 0.5);
		Assert.Equal(50.0, t.X, 6);
		Assert.True(t.HasAnimatedProperties);
	}

	[Fact]
	public void Null_animation_removes_animations_and_restores_the_base_value()
	{
		var t = new TranslateTransform { X = 7 };
		var clock = Linear(0, 100).CreateClock();
		t.ApplyAnimationClock(TranslateTransform.XProperty, clock);
		Tick(clock, 0.5);
		t.BeginAnimation(TranslateTransform.XProperty, null);
		Assert.Equal(7.0, t.X);
		Assert.Equal(7.0, t.GetAnimationBaseValue(TranslateTransform.XProperty));
		Assert.False(t.HasAnimatedProperties);
	}

	[Fact]
	public void Frozen_animatable_throws_on_BeginAnimation_and_ApplyAnimationClock()
	{
		var t = new TranslateTransform();
		t.Freeze();
		Assert.Throws<InvalidOperationException>(() => t.BeginAnimation(TranslateTransform.XProperty, Linear(0, 1)));
		Assert.Throws<InvalidOperationException>(
			() => t.ApplyAnimationClock(TranslateTransform.XProperty, Linear(0, 1).CreateClock()));
	}

	[Fact]
	public void Animated_value_wins_over_local_and_base_value_is_kept()
	{
		var t = new TranslateTransform { X = 5 };
		var clock = Linear(0, 10).CreateClock();
		t.ApplyAnimationClock(TranslateTransform.XProperty, clock);
		Tick(clock, 1);
		Assert.Equal(10.0, t.X);
		Assert.Equal(5.0, t.GetAnimationBaseValue(TranslateTransform.XProperty));
		Assert.Equal(5.0, t.ReadLocalValue(TranslateTransform.XProperty));
	}

	[Fact]
	public void FillBehavior_Stop_restores_the_base_value_when_the_clock_ends()
	{
		var t = new TranslateTransform { X = 5 };
		var anim = Linear(0, 10);
		anim.FillBehavior = FillBehavior.Stop;
		var clock = anim.CreateClock();
		t.ApplyAnimationClock(TranslateTransform.XProperty, clock);
		Tick(clock, 0.5);
		Assert.Equal(5.0, t.X, 6);
		Tick(clock, 1);
		Assert.Equal(ClockState.Stopped, clock.CurrentState);
		Assert.Equal(5.0, t.X);
	}

	[Fact]
	public void SnapshotAndReplace_starts_the_new_animation_from_the_current_animated_value()
	{
		var t = new TranslateTransform();
		var first = Linear(0, 100).CreateClock();
		t.ApplyAnimationClock(TranslateTransform.XProperty, first);
		Tick(first, 0.5);
		var second = new DoubleAnimation { To = 0, Duration = TimeSpan.FromSeconds(1) }.CreateClock();
		t.ApplyAnimationClock(TranslateTransform.XProperty, second, HandoffBehavior.SnapshotAndReplace);
		Tick(second, 0.5);
		Assert.Equal(25.0, t.X, 6);
		Tick(first, 0.25);
		Assert.Equal(25.0, t.X, 6);
	}

	[Fact]
	public void Compose_layers_the_clocks_so_a_by_animation_adds_to_the_previous_one()
	{
		var t = new TranslateTransform();
		var first = Linear(0, 100).CreateClock();
		t.ApplyAnimationClock(TranslateTransform.XProperty, first);
		Tick(first, 1);
		var by = new DoubleAnimation { By = 10, Duration = TimeSpan.FromSeconds(1) }.CreateClock();
		t.ApplyAnimationClock(TranslateTransform.XProperty, by, HandoffBehavior.Compose);
		Tick(by, 1);
		Assert.Equal(110.0, t.X, 6);
	}

	[Fact]
	public void Animating_a_property_with_the_wrong_animation_type_throws()
	{
		var t = new TranslateTransform();
		var color = new ColorAnimation(Colors.Red, TimeSpan.FromSeconds(1));
		Assert.Throws<InvalidOperationException>(() => t.BeginAnimation(TranslateTransform.XProperty, color));
		Assert.Throws<ArgumentNullException>(() => t.BeginAnimation(null!, Linear(0, 1)));
	}

	[Fact]
	public void SolidColorBrush_color_is_a_dp_with_transparent_default_and_animates()
	{
		var b = new SolidColorBrush();
		Assert.Equal(Colors.Transparent, b.Color);
		var clock = new ColorAnimation(Colors.Black, Colors.White, TimeSpan.FromSeconds(1)).CreateClock();
		b.ApplyAnimationClock(SolidColorBrush.ColorProperty, clock);
		Tick(clock, 0.5);
		Assert.Equal(Color.FromArgb(255, 128, 128, 128), b.Color);
	}

	[Fact]
	public void Clone_of_a_frozen_transform_is_unfrozen_and_keeps_its_value()
	{
		var t = new TranslateTransform(3, 4);
		t.Freeze();
		var clone = (TranslateTransform)t.Clone();
		Assert.False(clone.IsFrozen);
		Assert.Equal(3.0, clone.X);
		clone.X = 9;
		Assert.Equal(3.0, t.X);
	}

	[Fact]
	public void CloneCurrentValue_copies_the_animated_value()
	{
		var t = new TranslateTransform();
		var clock = Linear(0, 100).CreateClock();
		t.ApplyAnimationClock(TranslateTransform.XProperty, clock);
		Tick(clock, 0.5);
		Assert.Equal(50.0, ((TranslateTransform)t.CloneCurrentValue()).X, 6);
	}

	[Fact]
	public void Identity_transform_is_frozen_and_group_changes_bubble()
	{
		Assert.True(Transform.Identity.IsFrozen);
		var group = new TransformGroup();
		var child = new TranslateTransform();
		group.Children.Add(child);
		var changed = 0;
		group.Changed += (s, e) => changed++;
		child.X = 5;
		Assert.True(changed > 0);
		Assert.Equal(5.0, group.Value.OffsetX);
	}
}
