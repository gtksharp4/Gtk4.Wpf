using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using Gtk4.PresentationFramework.Tests;

/// <summary>
/// Owns the single thread on which GTK is initialised for the WPF-on-GTK smoke tests.
/// Mirrors GtkFixture from Gtk4.Forms.Platform.Gtk.UnitTests.
/// </summary>
sealed class WpfGtkFixture
{
	static readonly Lazy<WpfGtkFixture> _instance =
		new Lazy<WpfGtkFixture>(() => new WpfGtkFixture(), LazyThreadSafetyMode.ExecutionAndPublication);

	public static WpfGtkFixture Instance => _instance.Value;

	readonly BlockingCollection<Action> _queue;
	readonly Thread? _thread;
	int _gtkThreadId = -1;

	/// <summary>Why GTK could not be brought up, or null if it is running.</summary>
	public string? UnavailableReason { get; }

	WpfGtkFixture()
	{
		var reason = DisplayUnavailableReason();

		if (reason != null)
		{
			_queue = new BlockingCollection<Action>();
			UnavailableReason = reason;
			return;
		}

		if (WpfGtkThreadBridge.Queue != null)
		{
			// macOS: use the thread-0 queue provided by Program.RunOnThreadZero.
			_queue = WpfGtkThreadBridge.Queue;
			Marshal(() =>
			{
				_gtkThreadId = Environment.CurrentManagedThreadId;
				try { Gtk.Application.Init(); }
				catch (DllNotFoundException e) { reason = NoRuntime(e.Message); }
				catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException inner)
				{
					reason = NoRuntime(inner.Message);
				}
			});
			UnavailableReason = reason;
			return;
		}

		_queue = new BlockingCollection<Action>();
		var ready = new ManualResetEventSlim();

		_thread = new Thread(() =>
		{
			_gtkThreadId = Environment.CurrentManagedThreadId;
			try { Gtk.Application.Init(); }
			catch (DllNotFoundException e) { reason = NoRuntime(e.Message); }
			catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException inner)
			{
				reason = NoRuntime(inner.Message);
			}
			finally { ready.Set(); }

			if (reason != null) return;

			foreach (var work in _queue.GetConsumingEnumerable())
			{
				work();
				// Drain pending GTK events after each test body.
				for (int i = 0; i < 20 && Gtk.Application.EventsPending(); i++)
					Gtk.Application.RunIteration(false);
			}
		});

		_thread.IsBackground = true;
		_thread.Name = "WpfGtk";

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			_thread.SetApartmentState(ApartmentState.STA);

		_thread.Start();
		ready.Wait();
		UnavailableReason = reason;
	}

	/// <summary>
	/// Throws unless called on the GTK thread. Pumping GLib from another thread is undefined behaviour:
	/// on macOS thread 0 iterates the same context concurrently and the test host segfaults.
	/// </summary>
	public void AssertOnGtkThread()
	{
		if (_gtkThreadId != Environment.CurrentManagedThreadId)
			throw new InvalidOperationException(
				"GTK main loop used off the GTK thread: wrap the test body in Run(() => ...).");
	}

	/// <summary>Runs <paramref name="action"/> on the GTK thread, rethrowing anything it throws.</summary>
	public void Invoke(Action action)
	{
		ExceptionDispatchInfo? error = null;
		Marshal(() =>
		{
			try { action(); }
			catch (Exception e) { error = ExceptionDispatchInfo.Capture(e); }
		});
		error?.Throw();
	}

	public T Invoke<T>(Func<T> func)
	{
		T result = default!;
		Invoke(() => { result = func(); });
		return result;
	}

	void Marshal(Action work)
	{
		using var done = new ManualResetEventSlim();
		_queue.Add(() =>
		{
			try { work(); }
			finally { done.Set(); }
		});
		done.Wait();
	}

	static string? DisplayUnavailableReason()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
			RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return null;

		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")) ||
			!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
			return null;

		return "No DISPLAY. Run under xvfb-run -a dotnet test ... for headless GTK smoke tests.";
	}

	static string NoRuntime(string detail) =>
		"Native GTK 4 runtime not available: " + detail +
		". On Linux install libgtk-4-1; on Windows install a GTK 4 runtime on PATH.";
}
