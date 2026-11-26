using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;

namespace Gtk4.Wpf.Testing;

/// <summary>
/// Owns the one thread GTK runs on (STA on Windows, thread 0 when a macOS runner adopts it). Work reaches it
/// through the GLib main context, so <see cref="System.Windows.Threading.Dispatcher"/> calls from the test
/// thread and <see cref="Invoke(Action)"/> use the same path.
/// </summary>
public static partial class GtkThread
{
	static readonly object Gate = new object();
	static bool _started;
	static volatile bool _stop;
	static int _threadId = -1;
	static string? _unavailable;
	static ManualResetEventSlim? _ready;

	/// <summary>Why GTK could not start, or <c>null</c> when it is running. Starts GTK on first use.</summary>
	public static string? UnavailableReason
	{
		get
		{
			EnsureStarted();
			return _unavailable;
		}
	}

	/// <summary>True on the GTK thread.</summary>
	public static bool IsOnGtkThread => _threadId == Environment.CurrentManagedThreadId;

	/// <summary>Throws unless called on the GTK thread: iterating GLib from another thread is undefined.</summary>
	public static void AssertOnGtkThread()
	{
		if (!IsOnGtkThread)
			throw new InvalidOperationException("The GTK main loop was used off the GTK thread. Use GtkThread.Invoke.");
	}

	/// <summary>Runs <paramref name="action"/> on the GTK thread and rethrows what it throws.</summary>
	public static void Invoke(Action action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));
		if (UnavailableReason != null) throw new InvalidOperationException(_unavailable);
		if (IsOnGtkThread)
		{
			action();
			return;
		}
		ExceptionDispatchInfo? error = null;
		using var done = new ManualResetEventSlim();
		System.Windows.Application.PostToMainLoop(() =>
		{
			try { action(); }
			catch (Exception e) { error = ExceptionDispatchInfo.Capture(e); }
			finally { done.Set(); }
		}, DispatcherPriority.Send);
		done.Wait();
		error?.Throw();
	}

	/// <summary>Runs <paramref name="func"/> on the GTK thread and returns its value.</summary>
	public static T Invoke<T>(Func<T> func)
	{
		if (func == null) throw new ArgumentNullException(nameof(func));
		T result = default!;
		Invoke(() => { result = func(); });
		return result;
	}

	/// <summary>
	/// Takes over the calling thread as the GTK thread, runs <paramref name="tests"/> on a worker and iterates the
	/// main loop until it returns. macOS needs this: GTK's Quartz backend only works on thread 0.
	/// </summary>
	public static int RunOnCurrentThread(Func<int> tests)
	{
		lock (Gate)
		{
			if (_started) throw new InvalidOperationException("GTK is already running.");
			_started = true;
			_ready = new ManualResetEventSlim();
		}
		int exit = 0;
		var worker = new Thread(() =>
		{
			try { exit = tests(); }
			catch (Exception e)
			{
				Console.Error.WriteLine("The test runner crashed: " + e);
				exit = 1;
			}
			finally
			{
				_stop = true;
				_ready.Wait();
				if (_unavailable == null) Wake();
			}
		}) { IsBackground = true, Name = "tests" };
		worker.Start();
		StartLoop();
		worker.Join();
		return exit;
	}

	static void EnsureStarted()
	{
		lock (Gate)
		{
			if (!_started)
			{
				_started = true;
				_ready = new ManualResetEventSlim();
				_unavailable = DisplayUnavailableReason();
				if (_unavailable != null)
				{
					_ready.Set();
					return;
				}
				var thread = new Thread(StartLoop) { IsBackground = true, Name = "WpfUiGtk" };
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
			}
		}
		_ready!.Wait();
	}

	static void StartLoop()
	{
		_threadId = Environment.CurrentManagedThreadId;
		RunLoop(System.Windows.Application.InitGtk, () => Gtk.Application.RunIteration(true),
			reason =>
			{
				_unavailable = reason;
				_ready!.Set();
			});
	}

	/// <summary>
	/// Initialises GTK and iterates until stopped. Returns why GTK is missing without iterating: touching the
	/// unloadable native library again from this background thread would kill the test host.
	/// </summary>
	internal static string? RunLoop(Action init, Action iterate, Action<string?> ready)
	{
		string? reason = null;
		try { init(); }
		catch (DllNotFoundException e) { reason = NoRuntime(e.Message); }
		catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException inner)
		{
			reason = NoRuntime(inner.Message);
		}
		finally
		{
			ready(reason);
		}
		if (reason != null) return reason;
		while (!_stop)
			iterate();
		return null;
	}

	static void Wake() => System.Windows.Application.PostToMainLoop(() => { }, DispatcherPriority.Send);

	static string? DisplayUnavailableReason()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return null;
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"))
			|| !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
			return null;
		return "No DISPLAY. Run under xvfb-run -a dotnet test ... for GTK UI tests.";
	}

	static string NoRuntime(string detail) =>
		"Native GTK 4 runtime not available: " + detail
		+ ". On Linux install libgtk-4-1; on Windows install a GTK 4 runtime on PATH.";
}
