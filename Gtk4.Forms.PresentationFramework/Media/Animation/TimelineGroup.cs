using System.Collections.ObjectModel;

namespace System.Windows.Media.Animation;

/// <summary>A <see cref="Timeline"/> that owns child timelines.</summary>
public abstract class TimelineGroup : Timeline
{
	/// <summary>The child timelines.</summary>
	public TimelineCollection Children { get; } = new TimelineCollection();

	/// <summary>Creates a controllable clock group.</summary>
	public new ClockGroup CreateClock() => (ClockGroup)base.CreateClock();

	protected internal override Clock AllocateClock() => new ClockGroup(this);

	internal override Duration GetAutomaticDuration()
	{
		var max = TimeSpan.Zero;
		foreach (var child in Children)
		{
			var end = child.GetParentEnd();
			if (!end.HasTimeSpan) return Duration.Forever;
			if (end.TimeSpan > max) max = end.TimeSpan;
		}
		return new Duration(max);
	}

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var target = (TimelineGroup)destination;
		foreach (var child in Children)
			target.Children.Add((Timeline)(currentValueOnly ? child.CloneCurrentValue() : child.Clone()));
	}
}

/// <summary>Plays its children in parallel. Mirrors WPF's <c>ParallelTimeline</c>.</summary>
public partial class ParallelTimeline : TimelineGroup
{
	/// <summary>Creates an empty parallel timeline.</summary>
	public ParallelTimeline() { }

	/// <summary>Creates a parallel timeline with a begin time.</summary>
	public ParallelTimeline(TimeSpan? beginTime) => BeginTime = beginTime;

	/// <summary>Creates a parallel timeline with a begin time and duration.</summary>
	public ParallelTimeline(TimeSpan? beginTime, Duration duration)
		: this(beginTime) => Duration = duration;

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new ParallelTimeline();
}

/// <summary>Collection of <see cref="Timeline"/> children.</summary>
public class TimelineCollection : Collection<Timeline> { }
