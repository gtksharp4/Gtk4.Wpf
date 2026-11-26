using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>Pumps the GTK main loop on the (current) GTK thread with a wall-clock bound.</summary>
static class GtkPump
{
	/// <summary>Iterates the main loop until <paramref name="condition"/> holds or the timeout elapses.</summary>
	public static bool Until(Func<bool> condition, int timeoutMs = 4000)
	{
		WpfGtkFixture.Instance.AssertOnGtkThread();
		var sw = Stopwatch.StartNew();
		// A periodic wake-up source so RunIteration(true) never blocks past the deadline, below every app priority
		// (SystemIdle is Low) so that, when iterations run late, it never starves idle work or redraws.
		uint wake = GLib.Timeout.Add(10, new GLib.TimeoutHandler(() => true), (int)GLib.Priority.Low + 1);
		try
		{
			while (!condition() && sw.ElapsedMilliseconds < timeoutMs)
			{
				long start = sw.ElapsedMilliseconds;
				Gtk.Application.RunIteration(true);
				ReportStall(sw.ElapsedMilliseconds - start);
			}
		}
		finally { GLib.Source.Remove(wake); }
		return condition();
	}

	// The wake-up source bounds an iteration to ~10 ms, so a long one means the loop itself was blocked; the
	// caller frame names the test (the macOS runner shows multi-second stalls).
	static void ReportStall(long ms)
	{
		if (ms < 1000) return;
		var caller = new StackTrace().GetFrames()
			.Select(f => f.GetMethod())
			.FirstOrDefault(m => m?.DeclaringType != null && m.DeclaringType != typeof(GtkPump)
				&& m.DeclaringType.Assembly == typeof(GtkPump).Assembly);
		Console.Error.WriteLine($"GtkPump STALL: one main-loop iteration took {ms} ms, in "
			+ $"{caller?.DeclaringType?.FullName}.{caller?.Name}");
	}

	/// <summary>Iterates the main loop for roughly <paramref name="ms"/> milliseconds.</summary>
	public static void For(int ms) => Until(() => false, ms);
}

/// <summary>
/// Routes <see cref="Dispatcher.BeginInvoke(Action, DispatcherPriority)"/> through the same GLib
/// bridge <c>Application.Run</c> installs, for the duration of a GTK smoke test, so deferred
/// lifecycle work runs from the real main loop; restores the previous bridge on dispose.
/// </summary>
sealed class GtkDispatcherScope : IDisposable
{
	readonly Action<Action> _previous = Dispatcher.BeginInvokeAction;
	readonly Action<Action, DispatcherPriority>? _previousPriority = Dispatcher.BeginInvokePriorityAction;

	public GtkDispatcherScope()
	{
		Dispatcher.BeginInvokeAction = a => System.Windows.Application.PostToMainLoop(a, DispatcherPriority.Normal);
		Dispatcher.BeginInvokePriorityAction = System.Windows.Application.PostToMainLoop;
	}

	public void Dispose()
	{
		Dispatcher.BeginInvokeAction = _previous;
		Dispatcher.BeginInvokePriorityAction = _previousPriority;
	}
}
