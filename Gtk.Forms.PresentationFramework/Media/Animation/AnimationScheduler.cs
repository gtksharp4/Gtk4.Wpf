using System;
using System.Collections.Generic;

namespace System.Windows.Media.Animation
{
	/// <summary>
	/// Manages all active <see cref="AnimationClock"/> instances and ticks them on the
	/// GTK thread via a single <c>GLib.Timeout</c> at ~60 fps.
	///
	/// All GTK/GLib interactions remain on the GLib main-loop thread; the ticker is not
	/// started until the first animation is registered.
	///
	/// Unit tests that do not run a GLib main loop tick clocks directly via
	/// <see cref="AnimationClock.Tick"/>, bypassing this scheduler entirely.
	/// </summary>
	internal static class AnimationScheduler
	{
		const uint TickIntervalMs = 16; // ~60 fps

		static readonly List<AnimationClock> _active = new List<AnimationClock>();
		static bool _running;
		static DateTime _lastTick = DateTime.UtcNow;
		static readonly object _lock = new object();

		/// <summary>
		/// Registers <paramref name="clock"/> for GLib-driven ticking.
		/// Safe to call before the first GLib iteration — the timer is started lazily.
		/// </summary>
		internal static void Register(AnimationClock clock)
		{
			lock (_lock)
			{
				_active.Add(clock);
				if (!_running)
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
		internal static void Unregister(AnimationClock clock)
		{
			lock (_lock)
				_active.Remove(clock);
		}

		// Called on the GLib main-loop thread every ~16 ms.
		static bool OnTick()
		{
			List<AnimationClock> snapshot;
			lock (_lock)
			{
				if (_active.Count == 0)
				{
					_running = false;
					return false; // stop the timeout source
				}
				snapshot = new List<AnimationClock>(_active);
			}

			var now = DateTime.UtcNow;
			var delta = now - _lastTick;
			_lastTick = now;

			var completed = new List<AnimationClock>();
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
	}
}
