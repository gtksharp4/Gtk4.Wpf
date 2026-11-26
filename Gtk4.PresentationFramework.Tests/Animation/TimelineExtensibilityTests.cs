using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Animation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Animation;

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

		// protected internal: protected alone is CS0507 with InternalsVisibleTo access.
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

	class BareTimeline : Timeline
	{
		protected override Freezable CreateInstanceCore() => new BareTimeline();
	}

	class OwnClockTimeline : Timeline
	{
		protected internal override Clock AllocateClock() => new OwnClock(this);

		protected override Freezable CreateInstanceCore() => new OwnClockTimeline();
	}

	class OwnClock : Clock
	{
		public OwnClock(Timeline timeline) : base(timeline)
		{
		}
	}

	[Fact]
	public void Timeline_without_an_AllocateClock_override_compiles_and_throws_NotSupported()
	{
		Assert.Throws<NotSupportedException>(() => new BareTimeline().CreateClock());
	}

	[Fact]
	public void External_timeline_can_build_a_clock_for_itself()
	{
		var timeline = new OwnClockTimeline();
		Assert.Same(timeline, timeline.CreateClock().Timeline);
	}

	[Fact]
	public void Clock_constructor_and_AllocateClock_are_protected_internal()
	{
		var ctor = typeof(Clock).GetConstructor(
			BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Timeline) }, null);
		Assert.NotNull(ctor);
		Assert.True(ctor!.IsFamilyOrAssembly);
		var allocate = typeof(Timeline).GetMethod("AllocateClock", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.NotNull(allocate);
		Assert.True(allocate!.IsFamilyOrAssembly);
		Assert.True(allocate.IsVirtual);
	}
}
