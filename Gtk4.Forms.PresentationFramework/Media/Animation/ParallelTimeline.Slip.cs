namespace System.Windows.Media.Animation;

/// <summary>How a <see cref="ParallelTimeline"/> reacts to a child that cannot start on time.</summary>
public enum SlipBehavior
{
	/// <summary>The group's duration grows to cover its children.</summary>
	Grow,

	/// <summary>The group slips with its children.</summary>
	Slip,
}

public partial class ParallelTimeline
{
	/// <summary>Identifies <see cref="SlipBehavior"/>.</summary>
	public static readonly DependencyProperty SlipBehaviorProperty = DependencyProperty.Register(
		nameof(SlipBehavior), typeof(SlipBehavior), typeof(ParallelTimeline), new PropertyMetadata(SlipBehavior.Grow));

	/// <summary>Creates a parallel timeline with a begin time, duration and slip behaviour.</summary>
	public ParallelTimeline(TimeSpan? beginTime, Duration duration, RepeatBehavior repeatBehavior)
		: this(beginTime, duration) => RepeatBehavior = repeatBehavior;

	/// <summary>How the group slips; stored only, the clock tree does not slip.</summary>
	public SlipBehavior SlipBehavior
	{
		get => (SlipBehavior)GetValue(SlipBehaviorProperty)!;
		set => SetValue(SlipBehaviorProperty, value);
	}
}
