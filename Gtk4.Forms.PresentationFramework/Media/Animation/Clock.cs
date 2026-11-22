namespace System.Windows.Media.Animation;

/// <summary>
/// Tracks the run-time state of a <see cref="Timeline"/>. Clocks advance when ticked by the
/// <see cref="AnimationScheduler"/>, or directly through <see cref="Tick"/> in tests.
/// </summary>
public abstract partial class Clock
{
	TimeSpan _elapsed = TimeSpan.Zero;
	TimeSpan? _currentTime;
	double? _progress;
	int? _iteration;
	ClockState _state = ClockState.Active;
	ClockController? _controller;
	bool _paused;
	bool _userStopped;
	bool _completedRaised;
	double? _speedOverride;
	double _localOffset;

	protected internal Clock(Timeline timeline) => Timeline = timeline;

	/// <summary>The timeline this clock runs.</summary>
	public Timeline Timeline { get; }

	/// <summary>The clock group that owns this clock, or null for a root clock.</summary>
	public ClockGroup? Parent { get; internal set; }

	/// <summary>Current state.</summary>
	public ClockState CurrentState => _state;

	/// <summary>Time inside the active period; null while stopped or before the begin time.</summary>
	public TimeSpan? CurrentTime => _currentTime;

	/// <summary>Progress of the current iteration in 0..1; null while stopped or before the begin time.</summary>
	public double? CurrentProgress => _progress;

	/// <summary>One-based iteration; null while stopped or before the begin time.</summary>
	public int? CurrentIteration => _iteration;

	/// <summary>Effective speed relative to the root; null while stopped.</summary>
	public double? CurrentGlobalSpeed => _state == ClockState.Stopped ? (double?)null : IsPaused ? 0 : GlobalSpeedFactor;

	/// <summary>Whether this clock, or an ancestor, is paused.</summary>
	public bool IsPaused => _paused || (Parent?.IsPaused ?? false);

	/// <summary>The controller, or null when the root is not controllable.</summary>
	public ClockController? Controller => _controller;

	/// <summary>Whether the root of this clock tree has a controller.</summary>
	public bool HasControllableRoot => Root._controller != null;

	/// <summary>Raised when the current time changes.</summary>
	public event EventHandler? CurrentTimeInvalidated;

	/// <summary>Raised when the state changes.</summary>
	public event EventHandler? CurrentStateInvalidated;

	/// <summary>Raised when the global speed changes.</summary>
	public event EventHandler? CurrentGlobalSpeedInvalidated;

	/// <summary>Raised once when the active period ends.</summary>
	public event EventHandler? Completed;

	internal Clock Root => Parent == null ? this : Parent.Root;

	internal double EffectiveSpeed => _speedOverride ?? Timeline.SpeedRatio;

	internal double GlobalSpeedFactor => EffectiveSpeed * (Parent?.GlobalSpeedFactor ?? 1);


	internal void InitializeRoot(bool controllable)
	{
		if (controllable) _controller = new ClockController(this);
	}

	/// <summary>Advances this root clock by <paramref name="delta"/> (the deterministic test seam).</summary>
	public void Tick(TimeSpan delta)
	{
		if (_userStopped || IsPaused || delta <= TimeSpan.Zero) return;
		Update(_elapsed + delta);
	}


	void Raise(ClockState oldState, TimeSpan? oldTime, double? oldSpeed)
	{
		var speed = CurrentGlobalSpeed;
		// WPF Clock.RaiseAccumulatedEvents order: time, speed, state, completed.
		if (_currentTime != oldTime)
		{
			CurrentTimeInvalidated?.Invoke(this, EventArgs.Empty);
			Timeline.RaiseTime(this);
		}
		if (speed != oldSpeed)
		{
			CurrentGlobalSpeedInvalidated?.Invoke(this, EventArgs.Empty);
			Timeline.RaiseGlobalSpeed(this);
		}
		if (_state != oldState)
		{
			CurrentStateInvalidated?.Invoke(this, EventArgs.Empty);
			Timeline.RaiseState(this);
		}
		if (_state != ClockState.Active && oldState == ClockState.Active && !_completedRaised)
		{
			_completedRaised = true;
			Completed?.Invoke(this, EventArgs.Empty);
			Timeline.RaiseCompleted(this);
		}
	}
}
