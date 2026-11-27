using System;
using System.Collections.Generic;

namespace System.Windows.Media.Animation;

/// <summary>
/// Manages all active <see cref="AnimationClock"/> instances and ticks them on the
/// GTK thread via a single <c>GLib.Timeout</c> at ~60 fps.
///
/// <b>Thread contract:</b> <see cref="Register"/> MUST be called on the GLib main-loop
/// thread when the GLib event loop is running.  When GTK is not running (e.g. pure unit
/// tests that tick clocks directly via <see cref="AnimationClock.Tick"/>) the check and
/// the GLib timer registration are skipped, so the test-seam path is unaffected.
///
/// To enforce the contract in a live GLib context, call
/// <see cref="NotifyGtkInitialised"/> once after <c>Gtk.Application.Init()</c>;
/// from that point <see cref="Register"/> asserts <c>GLib.MainContext.Default.IsOwner</c>.
///
/// Unit tests that do not run a GLib main loop tick clocks directly via
/// <see cref="AnimationClock.Tick"/>, bypassing this scheduler entirely.
/// </summary>
internal static class AnimationScheduler
{
	const uint TickIntervalMs = 16; // ~60 fps

	static readonly List<Clock> _active = new List<Clock>();
	static bool _running;
	static DateTime _lastTick = DateTime.UtcNow;
	static readonly object _lock = new object();

	// Set to true once Gtk.Application.Init() has been called so that Register
	// knows whether to enforce the GTK-thread contract.
	static bool _gtkReady;

	// The thread that initialised GTK; it owns the UI before the main loop starts (OnStartup).
	static int _uiThreadId;

	/// <summary>
	/// Override hook used by unit tests to inject a custom "is on GLib main-loop thread"
	/// predicate.  When null and <see cref="_gtkReady"/> is false the check is skipped.
	/// </summary>
	internal static Func<bool>? IsOnGtkThreadOverride;

	/// <summary>
	/// Must be called once after <c>Gtk.Application.Init()</c> to tell the scheduler
	/// that the GLib event loop is now live and thread-safety checks should be enforced.
	/// </summary>
	internal static void NotifyGtkInitialised()
	{
		lock (_lock) { _gtkReady = true; _uiThreadId = Environment.CurrentManagedThreadId; }
	}

	/// <summary>Test hook: the thread-check state, so a test that drives <c>Application.InitGtk</c> can undo it.</summary>
	internal static (bool Ready, int UiThreadId) GtkStateForTests
	{
		get { lock (_lock) return (_gtkReady, _uiThreadId); }
		set { lock (_lock) { _gtkReady = value.Ready; _uiThreadId = value.UiThreadId; } }
	}

	/// <summary>
	/// Registers <paramref name="clock"/> for GLib-driven ticking.
	/// When GTK is running (after <see cref="NotifyGtkInitialised"/>), this MUST be
	/// called on the GLib main-loop thread; otherwise an
	/// <see cref="InvalidOperationException"/> is thrown.
	/// When GTK is not running (pure unit tests) the GLib timer is not started and
	/// the clock is added to the list only (ticked via <see cref="AnimationClock.Tick"/>).
	/// </summary>
	internal static void Register(Clock clock)
	{
		AssertGtkThread();
		lock (_lock)
		{
			if (!_active.Contains(clock)) _active.Add(clock);
			if (_gtkReady && !_running)
			{
				_running = true;
				_lastTick = DateTime.UtcNow;
				GLib.Timeout.Add(TickIntervalMs, OnTick);
			}
		}
	}

	/// <summary>
	/// Removes <paramref name="clock"/> from the scheduler. The clock's own
	/// <see cref="AnimationClock.Stop"/> still removes the animation value.
	/// </summary>
	internal static void Unregister(Clock clock)
	{
		lock (_lock)
			_active.Remove(clock);
	}

	/// <summary>
	/// Test-only entry point that calls <see cref="Register"/> (and thus the GTK-thread
	/// assertion).  Used to verify that the guard fires when called from a non-GTK thread.
	/// </summary>
	internal static void RegisterForTest(Clock clock) => Register(clock);

	/// <summary>
	/// Test-only entry point that calls <see cref="Unregister"/> without requiring the
	/// GTK thread (cleanup after a failed <see cref="RegisterForTest"/> call that threw).
	/// </summary>
	internal static void UnregisterForTest(Clock clock) => Unregister(clock);

	/// <summary>Test-only: ticks every registered clock by <paramref name="delta"/> without a GLib loop.</summary>
	internal static void TickAllForTest(TimeSpan delta)
	{
		Clock[] snapshot;
		lock (_lock) snapshot = _active.ToArray();
		foreach (var clock in snapshot) clock.Tick(delta);
	}

	// Called on the GLib main-loop thread every ~16 ms.
	static bool OnTick()
	{
		List<Clock> snapshot;
		lock (_lock)
		{
			if (_active.Count == 0)
			{
				_running = false;
				return false; // stop the timeout source
			}
			snapshot = new List<Clock>(_active);
		}

		var now = DateTime.UtcNow;
		var delta = now - _lastTick;
		_lastTick = now;

		var completed = new List<Clock>();
		foreach (var clock in snapshot)
		{
			clock.Tick(delta);
			if (clock.CurrentState == ClockState.Stopped ||
			    clock.CurrentState == ClockState.Filling)
				completed.Add(clock);
		}

		lock (_lock)
		{
			foreach (var c in completed)
				_active.Remove(c);

			if (_active.Count == 0)
			{
				_running = false;
				return false; // stop timer
			}
		}

		return true; // keep firing
	}

	/// <summary>
	/// Throws <see cref="InvalidOperationException"/> when the caller is not on the
	/// GLib main-loop thread and GTK has been initialised.  When
	/// <see cref="IsOnGtkThreadOverride"/> is set it is used instead of the GLib check
	/// (for unit tests).  When neither applies (GTK not yet initialised) the check is
	/// skipped to allow pure test-seam usage.
	/// </summary>
	static void AssertGtkThread()
	{
		if (IsOnGtkThreadOverride != null)
		{
			if (!IsOnGtkThreadOverride())
				throw new InvalidOperationException(
					"AnimationScheduler.Register must be called on the GLib main-loop thread. "
					+ "Marshal the call via GLib.Idle.Add or the application dispatcher.");
			return;
		}

		// Only enforce when GTK is actually running.
		if (!_gtkReady) return;

		if (Environment.CurrentManagedThreadId != _uiThreadId && !GLib.MainContext.Default.IsOwner)
			throw new InvalidOperationException(
				"AnimationScheduler.Register must be called on the GLib main-loop thread. "
				+ "Marshal the call via GLib.Idle.Add or the application dispatcher.");
	}
}