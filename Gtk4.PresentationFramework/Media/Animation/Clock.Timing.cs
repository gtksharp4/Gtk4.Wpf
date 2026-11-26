namespace System.Windows.Media.Animation;

public abstract partial class Clock
{
	const double Epsilon = 1e-9;

	/// <summary>Sets the parent time, re-evaluates the clock and raises the change events.</summary>
	internal void Update(TimeSpan parentTime)
	{
		var oldState = _state;
		var oldTime = _currentTime;
		var oldSpeed = CurrentGlobalSpeed;
		_elapsed = parentTime;
		Evaluate(out var childTime);
		OnEvaluated(childTime);
		Raise(oldState, oldTime, oldSpeed);
	}

	/// <summary>Called after every evaluation; groups drive their children, animation clocks their targets.</summary>
	internal virtual void OnEvaluated(TimeSpan childTime) { }

	void Evaluate(out TimeSpan childTime)
	{
		childTime = TimeSpan.Zero;
		var begin = Timeline.BeginTime;
		if (_userStopped || begin == null)
		{
			SetValues(ClockState.Stopped, null, null, null);
			return;
		}
		var local = (_elapsed - begin.Value).TotalSeconds * EffectiveSpeed + _localOffset;
		if (local < 0)
		{
			SetValues(ClockState.Active, null, null, null);
			return;
		}

		var span = Timeline.ResolveDuration();
		if (!span.HasTimeSpan)
		{
			childTime = TimeSpan.FromSeconds(local);
			SetValues(ClockState.Active, childTime, _progress ?? 0, 1);
			return;
		}

		var dur = span.TimeSpan.TotalSeconds;
		var reverse = Timeline.AutoReverse;
		var iter = dur * (reverse ? 2 : 1);
		if (dur <= 0)
		{
			SetValues(EndState(), TimeSpan.Zero, reverse ? 0 : 1, 1);
			return;
		}

		var rb = Timeline.RepeatBehavior;
		var max = rb.Type == RepeatBehaviorType.Forever ? double.PositiveInfinity
			: rb.HasCount ? iter * rb.Count
			: rb.HasDuration ? rb.Duration.TotalSeconds
			: iter;
		var done = local >= max;
		var t = done ? max : local;
		int iteration;
		double pos;
		if (done)
		{
			iteration = Math.Max(1, (int)Math.Ceiling(max / iter - Epsilon));
			pos = max - (iteration - 1) * iter;
		}
		else
		{
			iteration = (int)Math.Floor(t / iter) + 1;
			pos = t - (iteration - 1) * iter;
		}

		var forward = reverse && pos > dur ? iter - pos : pos;
		childTime = TimeSpan.FromSeconds(forward);
		var progress = Ease(Math.Min(1, forward / dur));
		var state = done ? EndState() : ClockState.Active;
		if (state == ClockState.Stopped)
			SetValues(state, null, null, null);
		else
			SetValues(state, childTime, progress, iteration);
	}

	ClockState EndState() => Timeline.FillBehavior == FillBehavior.HoldEnd ? ClockState.Filling : ClockState.Stopped;

	void SetValues(ClockState state, TimeSpan? time, double? progress, int? iteration)
	{
		_state = state;
		_currentTime = time;
		_progress = progress;
		_iteration = iteration;
	}

	double Ease(double p)
	{
		var a = Timeline.AccelerationRatio;
		var d = Timeline.DecelerationRatio;
		if (a <= 0 && d <= 0) return p;
		var k = 1 / (1 - a / 2 - d / 2);
		if (p < a) return k * p * p / (2 * a);
		if (p <= 1 - d) return k * (p - a / 2);
		var q = 1 - p;
		return 1 - k * q * q / (2 * d);
	}

	/// <summary>Stops this clock because a parent stopped or was removed.</summary>
	internal void ForceStop()
	{
		var oldState = _state;
		var oldTime = _currentTime;
		var oldSpeed = CurrentGlobalSpeed;
		SetValues(ClockState.Stopped, null, null, null);
		OnEvaluated(TimeSpan.Zero);
		Raise(oldState, oldTime, oldSpeed);
	}

	internal void BeginClock()
	{
		_localOffset = 0;
		_userStopped = false;
		_completedRaised = false;
		_paused = false;
		Update(TimeSpan.Zero);
		AnimationScheduler.Register(this);
	}

	internal void StopClock()
	{
		_userStopped = true;
		Update(TimeSpan.Zero);
	}

	internal void SetPaused(bool paused)
	{
		var oldSpeed = CurrentGlobalSpeed;
		_paused = paused;
		if (CurrentGlobalSpeed != oldSpeed)
		{
			CurrentGlobalSpeedInvalidated?.Invoke(this, EventArgs.Empty);
			Timeline.RaiseGlobalSpeed(this);
		}
	}

	internal void SetSpeed(double ratio)
	{
		var oldSpeed = CurrentGlobalSpeed;
		if (Timeline.BeginTime is { } begin && _elapsed >= begin)
			_localOffset += (_elapsed - begin).TotalSeconds * (EffectiveSpeed - ratio);
		_speedOverride = ratio;
		Update(_elapsed);
		if (oldSpeed != CurrentGlobalSpeed)
		{
			CurrentGlobalSpeedInvalidated?.Invoke(this, EventArgs.Empty);
			Timeline.RaiseGlobalSpeed(this);
		}
	}

	internal void SeekClock(TimeSpan offset, TimeSeekOrigin origin)
	{
		if (_userStopped || Timeline.BeginTime == null) return;
		var local = offset.TotalSeconds;
		if (origin == TimeSeekOrigin.Duration)
		{
			var end = Timeline.GetParentEnd();
			if (!end.HasTimeSpan) return;
			local += (end.TimeSpan - Timeline.BeginTime.Value).TotalSeconds * EffectiveSpeed;
		}
		_localOffset = 0;
		_completedRaised = false;
		Update(Timeline.BeginTime.Value + TimeSpan.FromSeconds(local / EffectiveSpeed));
		if (_state == ClockState.Active && Parent == null) AnimationScheduler.Register(this);
	}

	internal void SkipToFillClock()
	{
		if (_userStopped || Timeline.BeginTime == null) return;
		_localOffset = 0;
		var end = Timeline.GetParentEnd();
		if (end.HasTimeSpan) Update(end.TimeSpan);
	}

	/// <summary>Detaches this clock and its descendants from every animated property.</summary>
	internal virtual void RemoveClock()
	{
		AnimationScheduler.Unregister(this);
		StopClock();
	}
}
