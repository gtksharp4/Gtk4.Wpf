namespace System.Windows.Media.Animation;

public abstract partial class Timeline
{
	/// <summary>Raised when a clock of this timeline changes speed.</summary>
	public event EventHandler? CurrentGlobalSpeedInvalidated;

	/// <summary>Raised when a clock of this timeline changes state.</summary>
	public event EventHandler? CurrentStateInvalidated;

	/// <summary>Raised when a clock of this timeline changes time.</summary>
	public event EventHandler? CurrentTimeInvalidated;

	/// <summary>Raised when a clock of this timeline completes.</summary>
	public event EventHandler? Completed;

	internal void RaiseGlobalSpeed(Clock c) => CurrentGlobalSpeedInvalidated?.Invoke(c, EventArgs.Empty);

	internal void RaiseState(Clock c) => CurrentStateInvalidated?.Invoke(c, EventArgs.Empty);

	internal void RaiseTime(Clock c) => CurrentTimeInvalidated?.Invoke(c, EventArgs.Empty);

	internal void RaiseCompleted(Clock c) => Completed?.Invoke(c, EventArgs.Empty);

	/// <summary>Creates a controllable root clock for this timeline.</summary>
	public Clock CreateClock() => CreateClock(true);

	/// <summary>Creates a root clock; <paramref name="hasControllableRoot"/> gives it a controller.</summary>
	public Clock CreateClock(bool hasControllableRoot)
	{
		var clock = AllocateClock();
		clock.InitializeRoot(hasControllableRoot);
		return clock;
	}

	/// <summary>Creates the clock for this timeline; derived timelines override it.</summary>
	protected internal virtual Clock AllocateClock() =>
		throw new NotSupportedException("This timeline type does not override AllocateClock.");

	/// <summary>The duration used when <see cref="Duration"/> is Automatic.</summary>
	internal virtual Duration GetAutomaticDuration() => new Duration(TimeSpan.FromSeconds(1));

	/// <summary>The span of one iteration, resolving Automatic.</summary>
	internal Duration ResolveDuration() => Duration.DurationType == DurationType.Automatic
		? GetAutomaticDuration()
		: Duration;

	/// <summary>The end of the active period in parent time, or Forever.</summary>
	internal Duration GetParentEnd()
	{
		if (BeginTime == null) return new Duration(TimeSpan.Zero);
		var dur = ResolveDuration();
		var rb = RepeatBehavior;
		if (rb.Type == RepeatBehaviorType.Forever || !dur.HasTimeSpan) return Duration.Forever;
		var iter = dur.TimeSpan.TotalSeconds * (AutoReverse ? 2 : 1);
		var active = rb.HasCount ? iter * rb.Count : rb.HasDuration ? rb.Duration.TotalSeconds : iter;
		return new Duration(BeginTime.Value + TimeSpan.FromSeconds(active / SpeedRatio));
	}
}
