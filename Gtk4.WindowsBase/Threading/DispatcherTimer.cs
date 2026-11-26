using System;

namespace System.Windows.Threading;

/// <summary>
/// A timer that fires <see cref="Tick"/> on the dispatcher thread at a specified
/// <see cref="Interval"/>.
/// </summary>
/// <remarks>
/// <para>
/// Backed by a pluggable <see cref="ScheduleAction"/> delegate.
/// The GTK presentation layer replaces this with <c>GLib.Timeout.Add</c>:
/// <code>
/// DispatcherTimer.ScheduleAction = (ms, cb) => GLib.Timeout.Add(ms, cb);
/// DispatcherTimer.RemoveSourceAction = id => GLib.Source.Remove(id);
/// </code>
/// </para>
/// <para>
/// <b>Test seam.</b> When <see cref="FireActionOverride"/> is non-null it is called
/// by <see cref="Start"/> instead of <see cref="ScheduleAction"/>. Tests capture the
/// fire callback and call it directly via <see cref="FireForTest"/>.
/// </para>
/// </remarks>
public class DispatcherTimer
{
	// ── GTK bridge delegates ─────────────────────────────────────────────────────

	/// <summary>
	/// Schedules a repeating callback at the given interval in milliseconds.
	/// Return value: an opaque handle passed to <see cref="RemoveSourceAction"/>.
	/// Replace with <c>(ms, cb) =&gt; GLib.Timeout.Add(ms, cb)</c> in production.
	/// Default: fires once inline (suitable for unit tests when no test-seam override).
	/// </summary>
	public static Func<uint, Func<bool>, uint> ScheduleAction { get; set; } =
		(ms, cb) => { cb(); return 0; };

	/// <summary>
	/// Removes the source registered by <see cref="ScheduleAction"/>.
	/// Replace with <c>id =&gt; GLib.Source.Remove(id)</c> in production.
	/// Default: no-op.
	/// </summary>
	public static Action<uint> RemoveSourceAction { get; set; } = _ => { };

	// ── Test seam ────────────────────────────────────────────────────────────────

	/// <summary>
	/// When set, replaces <see cref="ScheduleAction"/> inside <see cref="Start"/>.
	/// The supplied delegate receives the internal fire callback; call it to simulate
	/// a timer tick. Leave null in production.
	/// </summary>
	internal static Action<Action>? FireActionOverride { get; set; }

	/// <summary>Fires one tick, honoring <see cref="IsEnabled"/>. For tests only.</summary>
	internal void FireForTest()
	{
		if (_isEnabled) FireTick();
	}

	// ── Properties ───────────────────────────────────────────────────────────────

	TimeSpan _interval = TimeSpan.FromSeconds(1);

	/// <summary>The period between <see cref="Tick"/> firings.</summary>
	public TimeSpan Interval
	{
		get => _interval;
		set => _interval = value;
	}

	bool _isEnabled;

	/// <summary>
	/// Returns <c>true</c> when the timer is running. Setting to <c>true</c> calls
	/// <see cref="Start"/>; setting to <c>false</c> calls <see cref="Stop"/>.
	/// </summary>
	public bool IsEnabled
	{
		get => _isEnabled;
		set { if (value) Start(); else Stop(); }
	}

	/// <summary>The priority at which <see cref="Tick"/> is raised.</summary>
	public DispatcherPriority Priority { get; set; } = DispatcherPriority.Background;

	/// <summary>An arbitrary tag object for caller use.</summary>
	public object? Tag { get; set; }

	// ── Events ───────────────────────────────────────────────────────────────────

	/// <summary>Fired on the dispatcher thread each time the timer interval elapses.</summary>
	public event EventHandler? Tick;

	// ── Control ──────────────────────────────────────────────────────────────────

	uint _sourceId;
	bool _sourceRegistered;
	readonly object _lock = new object();

	/// <summary>Starts the timer. If already running, restarts it.</summary>
	public void Start()
	{
		lock (_lock)
		{
			StopCore();
			_isEnabled = true;

			if (FireActionOverride != null)
			{
				// Test seam: hand the fire callback to the override; skip schedule.
				// The callback honors IsEnabled so Stop() before firing is a no-op.
				FireActionOverride(() => { if (_isEnabled) FireTick(); });
				return;
			}

			uint ms = (uint)Math.Max(1, (long)_interval.TotalMilliseconds);
			_sourceId = ScheduleAction(ms, OnScheduledTick);
			_sourceRegistered = true;
		}
	}

	/// <summary>Stops the timer. No <see cref="Tick"/> will fire after this returns.</summary>
	public void Stop()
	{
		lock (_lock) { StopCore(); }
	}

	// ── Internals ────────────────────────────────────────────────────────────────

	void StopCore()
	{
		_isEnabled = false;
		if (_sourceRegistered)
		{
			RemoveSourceAction(_sourceId);
			_sourceRegistered = false;
			_sourceId = 0;
		}
	}

	// Called by the scheduled source (GLib or the default inline delegate).
	bool OnScheduledTick()
	{
		bool enabled;
		lock (_lock) { enabled = _isEnabled; }
		if (!enabled) return false;
		try { FireTick(); }
		catch (Exception ex)
		{
			if (!Dispatcher.CurrentDispatcher.RaiseUnhandledException(ex))
				throw;
		}
		lock (_lock) { return _isEnabled; }
	}

	void FireTick() => Tick?.Invoke(this, EventArgs.Empty);
}