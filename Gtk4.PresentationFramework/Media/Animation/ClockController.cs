namespace System.Windows.Media.Animation;

/// <summary>Interactive control of a root <see cref="Clock"/>. Mirrors WPF's <c>ClockController</c>.</summary>
public sealed class ClockController
{
	internal ClockController(Clock clock) => Clock = clock;

	/// <summary>The controlled clock.</summary>
	public Clock Clock { get; }

	/// <summary>Rate of the clock relative to its parent; must be finite and positive.</summary>
	public double SpeedRatio
	{
		get => Clock.EffectiveSpeed;
		set
		{
			if (!(value > 0) || double.IsInfinity(value))
				throw new ArgumentException("SpeedRatio must be finite and greater than zero.", nameof(value));
			Clock.SetSpeed(value);
		}
	}

	/// <summary>Restarts the clock from its begin time.</summary>
	public void Begin() => Clock.BeginClock();

	/// <summary>Pauses the clock.</summary>
	public void Pause() => Clock.SetPaused(true);

	/// <summary>Resumes a paused clock.</summary>
	public void Resume() => Clock.SetPaused(false);

	/// <summary>Removes the clock so its animations stop affecting their properties.</summary>
	public void Remove() => Clock.RemoveClock();

	/// <summary>Moves the clock to <paramref name="offset"/> from <paramref name="origin"/>.</summary>
	public void Seek(TimeSpan offset, TimeSeekOrigin origin)
	{
		if (origin != TimeSeekOrigin.BeginTime && origin != TimeSeekOrigin.Duration)
			throw new ArgumentException("Invalid TimeSeekOrigin value.", nameof(origin));
		Clock.SeekClock(offset, origin);
	}

	/// <summary>Seeks on the last tick; ticks are synchronous here, so it equals <see cref="Seek"/>.</summary>
	public void SeekAlignedToLastTick(TimeSpan offset, TimeSeekOrigin origin) => Seek(offset, origin);

	/// <summary>Moves the clock to the end of its active period.</summary>
	public void SkipToFill() => Clock.SkipToFillClock();

	/// <summary>Stops the clock; it can be restarted with <see cref="Begin"/>.</summary>
	public void Stop() => Clock.StopClock();
}
