using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Xamarin.Forms.Platform.GTK.UnitTests;

/// <summary>
/// Owns the single thread that GTK + Forms are initialised on, and marshals every test body onto
/// it. One instance for the whole assembly, started lazily on first use.
/// </summary>
/// <remarks>
/// GTK may only be touched from the thread that called <c>gtk_init</c>, and xUnit makes no promise
/// about which thread a test body runs on - disabling parallelism (see AssemblyTestConfig.cs) is
/// necessary but NOT sufficient, because sequential tests can still land on different pool threads.
///
/// <para>On Windows the deeper reason is apartment state: GTK4's Win32 GDK backend calls
/// <c>OleInitialize</c> during <c>gtk_init</c>, which REQUIRES the calling thread to be STA. The
/// xUnit runner thread is MTA, so initialising GTK on it aborts the process with
/// <c>Gdk-ERROR: OleInitialize failed</c> ("incompatible apartment model"). So the fixture starts
/// its own thread, sets it STA on Windows, initialises GTK + Forms there, and runs every test body
/// on it through <see cref="Invoke"/>. <c>Forms.Init()</c> running here is what pins
/// <c>Forms.MainThread</c> to this thread.</para>
///
/// <para>If the native runtime is missing, <c>Application.Init()</c> throws
/// <see cref="DllNotFoundException"/>; the fixture captures that as <see cref="UnavailableReason"/>
/// so callers can SKIP rather than hang - exactly the behaviour the old latched bootstrap had.</para>
/// </remarks>
sealed class GtkFixture
{
	static readonly Lazy<GtkFixture> s_instance =
		new Lazy<GtkFixture>(() => new GtkFixture(), LazyThreadSafetyMode.ExecutionAndPublication);

	public static GtkFixture Instance => s_instance.Value;

	readonly BlockingCollection<Action> _queue;
	readonly Thread _thread;

	/// <summary>Why GTK could not be brought up here, or null if it is running.</summary>
	public string UnavailableReason { get; }

	GtkFixture()
	{
		var reason = GtkTestHost.DisplayUnavailableReason();

		if (reason != null)
		{
			_queue = new BlockingCollection<Action>();
			UnavailableReason = reason;
			return;
		}

		if (GtkThreadBridge.Queue != null)
		{
			// A thread-0 runner (macOS, where GTK 4's Quartz backend must run on the process's
			// first thread) owns the GTK thread. Initialise GTK + Forms on it through the shared
			// queue - the runner's RunLoop services this and every later test body - and start no
			// background thread of our own.
			_queue = GtkThreadBridge.Queue;

			Marshal(() =>
			{
				try
				{
					Gtk.Application.Init();
					Xamarin.Forms.Forms.Init();
				}
				catch (DllNotFoundException e)
				{
					reason = GtkTestHost.NoNativeRuntime(e.Message);
				}
				catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException inner)
				{
					reason = GtkTestHost.NoNativeRuntime(inner.Message);
				}
			});

			UnavailableReason = reason;
			return;
		}

		_queue = new BlockingCollection<Action>();

		var ready = new ManualResetEventSlim();

		_thread = new Thread(() =>
		{
			try
			{
				Gtk.Application.Init();
				Xamarin.Forms.Forms.Init();
			}
			catch (DllNotFoundException e)
			{
				reason = GtkTestHost.NoNativeRuntime(e.Message);
			}
			catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException inner)
			{
				reason = GtkTestHost.NoNativeRuntime(inner.Message);
			}
			finally
			{
				ready.Set();
			}

			if (reason != null)
				return;

			// Service test bodies until the process tears the background thread down.
			foreach (var work in _queue.GetConsumingEnumerable())
			{
				work();
				GtkTestHost.PumpPending();
			}
		});

		_thread.IsBackground = true;
		_thread.Name = "Gtk";

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			_thread.SetApartmentState(ApartmentState.STA);

		_thread.Start();
		ready.Wait();

		UnavailableReason = reason;
	}

	/// <summary>Runs <paramref name="action"/> on the GTK thread, rethrowing anything it throws.</summary>
	public void Invoke(Action action)
	{
		ExceptionDispatchInfo error = null;

		Marshal(() =>
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				error = ExceptionDispatchInfo.Capture(e);
			}
		});

		error?.Throw();
	}

	public T Invoke<T>(Func<T> func)
	{
		T result = default;
		Invoke(() => { result = func(); });
		return result;
	}

	/// <summary>Enqueues work on the GTK thread and blocks until it has run.</summary>
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
}