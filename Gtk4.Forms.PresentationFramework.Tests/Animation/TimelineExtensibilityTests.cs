using System;
using System.Windows;
using System.Windows.Media.Animation;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Animation;

public class TimelineExtensibilityTests
{
	class NoOpAnimation : AnimationTimeline
	{
		public override Type TargetPropertyType => typeof(double);

		protected override Freezable CreateInstanceCore() => new NoOpAnimation();
	}

	class CustomTimeline : Timeline
	{
		public Clock? Allocated;

		protected internal override Clock AllocateClock() => Allocated = new DoubleAnimation().CreateClock();

		protected override Freezable CreateInstanceCore() => new CustomTimeline();
	}

	[Fact]
	public void Animation_without_a_value_override_returns_the_destination_value()
	{
		var anim = new NoOpAnimation();
		var clock = anim.CreateClock();
		Assert.Equal(2.0, anim.GetCurrentValue(1.0, 2.0, clock));
	}

	[Fact]
	public void External_timeline_can_override_AllocateClock()
	{
		var timeline = new CustomTimeline();
		var clock = timeline.CreateClock();
		Assert.NotNull(timeline.Allocated);
		Assert.Same(timeline.Allocated, clock);
	}
}
