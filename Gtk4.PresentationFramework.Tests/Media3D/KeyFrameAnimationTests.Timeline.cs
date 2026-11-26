using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Animation.AnimationTestHelpers;

namespace Gtk4.PresentationFramework.Tests.Media3D;

public partial class KeyFrameAnimationTests
{
	[Fact]
	public void ParallelTimeline_runs_children_in_parallel_and_drives_a_property()
	{
		var target = Target();
		var timeline = new ParallelTimeline();
		timeline.Children.Add(new DoubleAnimation(0, 90, TimeSpan.FromSeconds(1)));
		timeline.Children.Add(new DoubleAnimation(0, 10, TimeSpan.FromSeconds(2)));
		var group = timeline.CreateClock();
		Assert.Equal(2, group.Children.Count);
		var first = (AnimationClock)group.Children[0];
		var second = (AnimationClock)group.Children[1];
		target.ApplyAnimationClock(AxisAngleRotation3D.AngleProperty, first);
		var other = new TranslateTransform3D();
		other.ApplyAnimationClock(TranslateTransform3D.OffsetXProperty, second);
		Tick(group, 1);
		Assert.Equal(90.0, target.Angle, 6);
		Assert.Equal(5.0, other.OffsetX, 6);
		Assert.Equal(TimeSpan.FromSeconds(2), group.Timeline.ResolveNaturalDuration());
	}

	[Fact]
	public void ParallelTimeline_slip_behavior_is_stored_with_the_wpf_default()
	{
		var timeline = new ParallelTimeline();
		Assert.Equal(SlipBehavior.Grow, timeline.SlipBehavior);
		timeline.SlipBehavior = SlipBehavior.Slip;
		Assert.Equal(SlipBehavior.Slip, timeline.GetValue(ParallelTimeline.SlipBehaviorProperty));
		Assert.IsType<ClockGroup>(timeline.CreateClock());
	}
}
