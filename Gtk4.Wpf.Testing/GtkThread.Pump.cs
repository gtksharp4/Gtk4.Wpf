using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace Gtk4.Wpf.Testing;

public static partial class GtkThread
{
	/// <summary>
	/// Iterates the GTK main loop on the GTK thread until <paramref name="condition"/> holds or
	/// <paramref name="timeout"/> elapses. Called from another thread it hops to the GTK thread first, so the
	/// loop is never pumped anywhere else. The condition runs on the GTK thread.
	/// </summary>
	public static bool PumpUntil(Func<bool> condition, TimeSpan timeout)
	{
		if (condition == null) throw new ArgumentNullException(nameof(condition));
		return Invoke(() => PumpHere(condition, timeout));
	}

	/// <summary>Iterates the main loop for about <paramref name="duration"/> on the GTK thread.</summary>
	public static void Pump(TimeSpan duration) => PumpUntil(() => false, duration);

	/// <summary>
	/// Returns once every pending main-loop source, including idle work, has run: an idle-priority marker is
	/// posted and the loop iterates until it fires. A source that is always ready (a zero-interval timer, a
	/// self-reposting callback) keeps the marker from firing and the drain from ending, so both are bounded by
	/// <paramref name="timeout"/>; the marker not firing throws <see cref="WaitTimeoutException"/>.
	/// </summary>
	public static void WaitForIdle(TimeSpan? timeout = null) => Invoke(() =>
	{
		var limit = timeout ?? TimeSpan.FromSeconds(5);
		bool idle = false;
		System.Windows.Application.PostToMainLoop(() => idle = true, DispatcherPriority.SystemIdle);
		if (!PumpHere(() => idle, limit, out var stats))
			throw new WaitTimeoutException(
				$"The application did not become idle within {limit.TotalSeconds:0.#} s: a source such as a "
				+ "zero-interval timer is always ready (" + stats + ")." + Environment.NewLine + "Automation tree:"
				+ Environment.NewLine + TreeDump.Of(System.Windows.Automation.AutomationElement.RootElement));
		// Its own clock: the wait above may have excused a frozen iteration that this bound would otherwise count.
		var clock = Stopwatch.StartNew();
		for (int i = 0; i < MaxDrain && clock.Elapsed < limit && GLib.MainContext.Pending(); i++)
			GLib.MainContext.Iteration(false);
	});

	const int MaxDrain = 1000;

	// An iteration this long means the process itself was frozen (the hosted macOS runner freezes for tens of seconds
	// when a window is shown); that time does not count against a wait, up to MaxExcusedStall in all.
	static readonly TimeSpan Stall = TimeSpan.FromSeconds(1);
	static readonly TimeSpan MaxExcusedStall = TimeSpan.FromSeconds(60);

	static bool PumpHere(Func<bool> condition, TimeSpan timeout) => PumpHere(condition, timeout, out _);

	// stats says how the time went, which tells a loop that is always busy from one whose iterations were frozen.
	static bool PumpHere(Func<bool> condition, TimeSpan timeout, out string stats)
	{
		AssertOnGtkThread();
		var clock = Stopwatch.StartNew();
		int iterations = 0;
		TimeSpan longest = TimeSpan.Zero, excused = TimeSpan.Zero;
		// A periodic source keeps RunIteration(true) from blocking past the deadline. It sits below every app
		// priority (SystemIdle is Low) so that, when iterations run late, it never starves idle work or redraws.
		uint wake = GLib.Timeout.Add(10, new GLib.TimeoutHandler(() => true), (int)GLib.Priority.Low + 1);
		try
		{
			while (!condition() && clock.Elapsed - excused < timeout)
			{
				var start = clock.Elapsed;
				Gtk.Application.RunIteration(true);
				iterations++;
				var took = clock.Elapsed - start;
				if (took > longest) longest = took;
				if (took >= Stall)
				{
					excused += took;
					if (excused > MaxExcusedStall) excused = MaxExcusedStall;
				}
			}
		}
		finally { GLib.Source.Remove(wake); }
		stats = $"{iterations} iterations in {clock.Elapsed.TotalSeconds:0.#} s, the longest {longest.TotalMilliseconds:0} ms";
		return condition();
	}
}
