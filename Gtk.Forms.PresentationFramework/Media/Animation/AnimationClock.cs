using System;

namespace System.Windows.Media.Animation;

/// <summary>State of an <see cref="AnimationClock"/>.</summary>
public enum ClockState
{
	/// <summary>The clock is running (or in its delay period).</summary>
	Active,
	/// <summary>The clock has completed its active period.</summary>
	Filling,
	/// <summary>The clock has stopped (FillBehavior.Stop or explicitly stopped).</summary>
	Stopped
}

/// <summary>
/// Tracks time progression for a single <see cref="AnimationTimeline"/> running against
/// one target DP on one target object.
///
/// Design: the clock stores a local elapsed time and can be ticked either by the
/// <see cref="AnimationScheduler"/> (via GLib.Timeout) or directly via
/// <see cref="Tick"/> for deterministic unit testing (no GLib loop required).
/// </summary>
public sealed class AnimationClock
{
	readonly AnimationTimeline _timeline;
	readonly DependencyObject _target;
	readonly DependencyProperty _targetProperty;

	TimeSpan _elapsed = TimeSpan.Zero;
	bool _paused;
	ClockState _state = ClockState.Active;

	internal AnimationClock(
		AnimationTimeline timeline,
		DependencyObject target,
		DependencyProperty targetProperty)
	{
		_timeline = timeline;
		_target = target;
		_targetProperty = targetProperty;
	}

	/// <summary>Current state of this clock.</summary>
	public ClockState CurrentState => _state;

	/// <summary>Total elapsed time (not counting pauses).</summary>
	public TimeSpan CurrentTime => _elapsed;

	/// <summary>Normalised progress within the current iteration [0,1].</summary>
	public double CurrentProgress { get; private set; }

	/// <summary>
	/// Advances this clock by <paramref name="delta"/> and writes the interpolated
	/// value to the target DP via the animation layer.
	/// Safe to call without a GLib main loop — used by unit tests.
	/// </summary>
	public void Tick(TimeSpan delta)
	{
		if (_state == ClockState.Stopped) return;
		if (_paused) return;
		if (delta <= TimeSpan.Zero) return;

		_elapsed += delta;

		// Respect BeginTime delay.
		var beginTime = _timeline.BeginTime ?? TimeSpan.Zero;
		if (_elapsed < beginTime) return;

		var localTime = _elapsed - beginTime;

		// Resolve the effective duration.
		// Duration.Automatic (the Timeline default) maps to 1 second, matching WPF behaviour.
		// Duration.Forever is infinite — keep clock Active at current progress indefinitely.
		Duration effectiveDuration = _timeline.Duration;
		if (effectiveDuration.DurationType == DurationType.Automatic)
			effectiveDuration = new Duration(TimeSpan.FromSeconds(1));

		if (!effectiveDuration.HasTimeSpan)
		{
			// Forever: run indefinitely at current progress (no completion).
			ApplyValue(CurrentProgress);
			return;
		}

		var dur = effectiveDuration.TimeSpan;
		if (dur == TimeSpan.Zero)
		{
			// Zero-duration: immediately at end.
			Complete();
			return;
		}

		double iterDuration = dur.TotalSeconds * (_timeline.AutoReverse ? 2.0 : 1.0);
		double totalSeconds = localTime.TotalSeconds;

		RepeatBehavior rb = _timeline.RepeatBehavior;
		bool isForever = rb.Type == RepeatBehaviorType.Forever;

		// Compute max total run time.
		double maxSeconds = isForever
			? double.PositiveInfinity
			: rb.HasCount
				? iterDuration * rb.Count
				: rb.HasDuration
					? rb.Duration.TotalSeconds
					: iterDuration; // default = count 1

		if (totalSeconds >= maxSeconds)
		{
			// Completed all iterations.
			Complete();
			return;
		}

		// Position within the current iteration.
		double posInIter = totalSeconds % iterDuration;
		double progress;
		if (_timeline.AutoReverse && posInIter >= dur.TotalSeconds)
		{
			// In the reverse phase.
			progress = 1.0 - (posInIter - dur.TotalSeconds) / dur.TotalSeconds;
		}
		else
		{
			progress = Math.Min(posInIter / dur.TotalSeconds, 1.0);
		}

		CurrentProgress = progress;
		ApplyValue(progress);
	}

	/// <summary>Pauses this clock; <see cref="Tick"/> calls are ignored while paused.</summary>
	public void Pause() => _paused = true;

	/// <summary>Resumes a paused clock.</summary>
	public void Resume() => _paused = false;

	/// <summary>Stops this clock and clears the animation value.</summary>
	public void Stop()
	{
		_state = ClockState.Stopped;
		_target.ClearAnimationValue(_targetProperty);
	}

	// ── internals ────────────────────────────────────────────────────────────

	void ApplyValue(double progress)
	{
		CurrentProgress = progress;
		var baseValue = _target.ReadLocalValue(_targetProperty);
		if (ReferenceEquals(baseValue, DependencyProperty.UnsetValue))
			baseValue = _targetProperty.GetMetadata(_target.GetType()).DefaultValue;

		var value = _timeline.GetCurrentValue(baseValue, baseValue, this);
		_target.SetAnimationValue(_targetProperty, value);
	}

	void Complete()
	{
		if (_state == ClockState.Stopped) return;

		if (_timeline.FillBehavior == FillBehavior.HoldEnd)
		{
			// Snap to the final value and hold it.
			ApplyValue(_timeline.AutoReverse ? 0.0 : 1.0);
			_state = ClockState.Filling;
		}
		else
		{
			// FillBehavior.Stop: remove the animation value.
			_state = ClockState.Stopped;
			_target.ClearAnimationValue(_targetProperty);
		}
	}
}