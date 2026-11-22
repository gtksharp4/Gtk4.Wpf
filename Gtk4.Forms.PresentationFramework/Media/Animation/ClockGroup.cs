using System.Collections.Generic;

namespace System.Windows.Media.Animation;

/// <summary>A clock that owns child clocks, created from a <see cref="TimelineGroup"/>.</summary>
public class ClockGroup : Clock
{
	readonly List<Clock> _children = new List<Clock>();

	internal ClockGroup(TimelineGroup timeline) : base(timeline)
	{
		foreach (var child in timeline.Children)
		{
			var clock = child.AllocateClock();
			clock.Parent = this;
			_children.Add(clock);
		}
	}

	/// <summary>The group's timeline.</summary>
	public new TimelineGroup Timeline => (TimelineGroup)base.Timeline;

	/// <summary>The child clocks, in the order of the timeline's children.</summary>
	public IReadOnlyList<Clock> Children => _children;

	internal override void OnEvaluated(TimeSpan childTime)
	{
		var stopped = CurrentState == ClockState.Stopped;
		var pending = !stopped && CurrentTime == null;
		foreach (var child in _children)
		{
			if (stopped) child.ForceStop();
			else if (pending) child.Update(TimeSpan.FromTicks(-1));
			else child.Update(childTime);
		}
	}

	internal override void RemoveClock()
	{
		foreach (var child in _children)
			child.RemoveClock();
		base.RemoveClock();
	}
}
