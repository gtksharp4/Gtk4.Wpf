using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Threading;

namespace System.Windows.Automation;

/// <summary>
/// Runs client calls on the GTK thread through the <see cref="Dispatcher"/> and returns once the dispatcher
/// is idle, so a read after an action sees the action's result. A nested call runs inline.
/// </summary>
static class AutomationThread
{
	const int SettleTimeoutMs = 2000;
	const int MaxPumped = 1000;

	[ThreadStatic]
	static int _depth;

	internal static T Call<T>(Func<T> func)
	{
		if (func == null) throw new ArgumentNullException(nameof(func));
		bool nested = _depth > 0;
		T result = default!;
		ExceptionDispatchInfo? error = null;
		Dispatcher.CurrentDispatcher.Invoke(() =>
		{
			_depth++;
			try { result = func(); }
			catch (Exception e) { error = ExceptionDispatchInfo.Capture(e); }
			finally { _depth--; }
		});
		if (!nested) Settle();
		error?.Throw();
		return result;
	}

	internal static void Call(Action action) =>
		Call(() =>
		{
			action();
			return true;
		});

	/// <summary>Runs <paramref name="action"/> with nested calls and settling suppressed (event delivery).</summary>
	internal static void Deliver(Action action)
	{
		_depth++;
		try { action(); }
		finally { _depth--; }
	}

	static void Settle()
	{
		var dispatcher = Dispatcher.CurrentDispatcher;
		if (dispatcher.CheckAccess())
		{
			var clock = Stopwatch.StartNew();
			for (int i = 0; i < MaxPumped && clock.ElapsedMilliseconds < SettleTimeoutMs
				&& GLib.MainContext.Pending(); i++)
				GLib.MainContext.Iteration(false);
			return;
		}
		using var done = new ManualResetEventSlim();
		dispatcher.BeginInvoke(done.Set, DispatcherPriority.ApplicationIdle);
		done.Wait(SettleTimeoutMs);
	}
}
