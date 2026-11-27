using System;
using System.Threading;

namespace System.Windows;

public partial class Application
{
	GLib.MainLoop? _loop;
	bool _running;
	bool _runCalled;
	bool _runCompleted;

	/// <summary>Test seam: replaces <c>GLib.MainLoop.Run</c> so the lifecycle runs without blocking.</summary>
	internal Action<Application>? RunLoopOverride { get; set; }

	/// <summary>
	/// Initialises GTK, raises <see cref="Startup"/>, shows the StartupUri window and runs the
	/// main loop until shutdown. Returns <see cref="ExitEventArgs.ApplicationExitCode"/>.
	/// A <see cref="MainWindow"/> assigned in code is not shown; use <see cref="Run(Window)"/>.
	/// </summary>
	public int Run() => RunWithGtk(null);

	/// <summary>Adds the window, makes it <see cref="MainWindow"/> if none is set, then runs and shows it.</summary>
	public int Run(Window window)
	{
		PrepareMainWindow(window);
		return RunWithGtk(window);
	}

	// WPF adds the window to Windows if missing and sets MainWindow only when it is unset.
	internal void PrepareMainWindow(Window window)
	{
		if (window == null) throw new ArgumentNullException(nameof(window));
		if (!_windows.Contains(window)) _windows.Add(window);
		MainWindow ??= window;
	}

	int RunWithGtk(Window? windowToShow)
	{
		InitGtk();
		int code = RunCore(StartupEventArgs.ProcessArgs(), windowToShow);
		// Generated Main is void, so the process exit code is the only channel besides Run's result.
		if (code != 0)
			Environment.ExitCode = code;
		return code;
	}

	static bool _gtkInitialised;

	/// <summary>
	/// Initialises GTK for a window created before <see cref="Run()"/> (WPF allows that). A no-op when GTK is
	/// already up, through <see cref="InitGtk"/> or because a host initialised it directly.
	/// </summary>
	internal static void EnsureGtkInitialised()
	{
		if (_gtkInitialised || Gdk.Display.Default != null)
			return;
		InitGtk();
	}

	/// <summary>Idempotent: wires GTK and the dispatcher once per process.</summary>
	internal static void InitGtk()
	{
		if (_gtkInitialised)
			return;
		_gtkInitialised = true;
		Gtk.Application.Init();
		// Notify the animation scheduler that the GLib event loop is now live.
		System.Windows.Media.Animation.AnimationScheduler.NotifyGtkInitialised();
		// Wire Dispatcher delegates to the GLib main loop.
		DispatcherGlibHost.Attach();
		Threading.Dispatcher.IsOnDispatcherThread = () => GLib.MainContext.Default.IsOwner;
		Threading.Dispatcher.BeginInvokeAction = action => PostToMainLoop(action, Threading.DispatcherPriority.Normal);
		Threading.Dispatcher.BeginInvokePriorityAction = PostToMainLoop;
		Threading.Dispatcher.InvokeAction = action =>
		{
			if (GLib.MainContext.Default.IsOwner) { action(); return; }
			using var done = new System.Threading.ManualResetEventSlim();
			PostToMainLoop(() => { try { action(); } finally { done.Set(); } }, Threading.DispatcherPriority.Send);
			done.Wait();
		};
		// Wire DispatcherTimer delegates to GLib.
		Threading.DispatcherTimer.ScheduleAction = (ms, cb) =>
			GLib.Timeout.Add(ms, new GLib.TimeoutHandler(() => cb()));
		Threading.DispatcherTimer.RemoveSourceAction = id => GLib.Source.Remove(id);
		EnsureGlibExceptionHook();
	}

	/// <summary>The lifecycle without GTK init, so tests can drive it on the fixture thread.</summary>
	internal int RunCore(string[] args, Window? windowToShow = null)
	{
		if (Dispatcher.Thread != Thread.CurrentThread)
			throw new InvalidOperationException(
				"The calling thread cannot access this object because a different thread owns it.");
		if (_runCalled)
			throw new InvalidOperationException("Cannot call Run more than once.");
		_runCalled = true;
		_running = true;
		try
		{
			// Startup runs inside the dispatcher in WPF, so its exceptions reach the unhandled-exception event.
			try { DoStartup(args); }
			catch (Exception ex) { if (!Dispatcher.RaiseUnhandledException(ex)) throw; }
			if (_pendingShutdown && !_shutdownStarted)
				DoShutdown(_exitCode);
			if (!_shutdownStarted)
			{
				try { windowToShow?.Show(); }
				catch (Exception ex) { if (!Dispatcher.RaiseUnhandledException(ex)) throw; }
				RunLoop();
			}
			if (!_shutdownStarted)
				DoShutdown(_exitCode);
			return _exitCode;
		}
		finally
		{
			_running = false;
			_runCompleted = true;
		}
	}

	/// <summary>Raises startup, then creates and shows the StartupUri window.</summary>
	internal void DoStartup(string[] args)
	{
		OnStartup(new StartupEventArgs(args));
		if (_shutdownStarted)
			return;
		// Shutdown() is asynchronous (posted), so the StartupUri window is still created; this check only
		// matters when the dispatcher runs posted work inline. A window created by a handler stays MainWindow.
		if (StartupUri != null)
		{
			var window = CreateWindowFromStartupUri(StartupUri);
			if (window != null)
			{
				MainWindow ??= window;
				window.Show();
			}
		}
	}

	void RunLoop()
	{
		if (RunLoopOverride != null)
		{
			RunLoopOverride(this);
			return;
		}
		_loop = new GLib.MainLoop();
		_loop.Run();
	}
}
