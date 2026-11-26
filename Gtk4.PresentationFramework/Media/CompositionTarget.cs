using System.Diagnostics;
using System.Windows.Threading;

namespace System.Windows.Media;

/// <summary>
/// Frame callbacks. <see cref="Rendering"/> fires about every 16 ms on the GLib main loop while it has at least
/// one handler (the timer starts on the first add and stops on the last remove). The time base is a process-wide
/// stopwatch, not the animation clock.
/// </summary>
public static class CompositionTarget
{
	const uint FrameIntervalMs = 16;
	static readonly Stopwatch _clock = Stopwatch.StartNew();
	static EventHandler? _rendering;
	static uint _timer;

	/// <summary>Occurs once per frame while subscribed.</summary>
	public static event EventHandler? Rendering
	{
		add
		{
			_rendering += value;
			if (_rendering != null && _timer == 0)
				_timer = GLib.Timeout.Add(FrameIntervalMs, OnFrame);
		}
		remove
		{
			_rendering -= value;
			if (_rendering == null && _timer != 0)
			{
				GLib.Source.Remove(_timer);
				_timer = 0;
			}
		}
	}

	// Gap: WPF raises Rendering from MediaContext at its own point in the frame (after layout, before the
	// composition commit); here it is a GLib timer, so ordering against layout and animation is not guaranteed.
	static bool OnFrame()
	{
		var handlers = _rendering;
		if (handlers == null)
		{
			_timer = 0;
			return false;
		}
		var args = new RenderingEventArgs(_clock.Elapsed);
		foreach (var handler in handlers.GetInvocationList())
		{
			try { ((EventHandler)handler)(null, args); }
			catch (Exception ex) { if (!Dispatcher.CurrentDispatcher.RaiseUnhandledException(ex)) throw; }
		}
		return true;
	}
}
