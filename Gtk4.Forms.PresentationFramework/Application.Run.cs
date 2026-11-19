using System;

namespace System.Windows;

public partial class Application
{
	GLib.MainLoop? _loop;
	bool _running;

	/// <summary>Test seam: replaces <c>GLib.MainLoop.Run</c> so the lifecycle runs without blocking.</summary>
	internal Action<Application>? RunLoopOverride { get; set; }

	/// <summary>
	/// Initialises GTK, raises <see cref="Startup"/>, shows the StartupUri window and runs the
	/// main loop until shutdown. Returns <see cref="ExitEventArgs.ApplicationExitCode"/>.
	/// A <see cref="MainWindow"/> assigned in code is not shown; use <see cref="Run(Window)"/>.
	/// </summary>
	public int Run() => RunWithGtk(null);

	/// <summary>Sets <see cref="MainWindow"/>, then runs and shows it after startup.</summary>
	public int Run(Window window)
	{
		MainWindow = window ?? throw new ArgumentNullException(nameof(window));
		return RunWithGtk(window);
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

	static void InitGtk()
	{
		Gtk.Application.Init();
		// Notify the animation scheduler that the GLib event loop is now live.
		System.Windows.Media.Animation.AnimationScheduler.NotifyGtkInitialised();
		// Wire Dispatcher delegates to the GLib main loop.
		Threading.Dispatcher.IsOnDispatcherThread = () => GLib.MainContext.Default.IsOwner;
		Threading.Dispatcher.BeginInvokeAction = action => GLib.Idle.Add(() => { action(); return false; });
		Threading.Dispatcher.InvokeAction = action =>
		{
			if (GLib.MainContext.Default.IsOwner) { action(); return; }
			using var done = new System.Threading.ManualResetEventSlim();
			GLib.Idle.Add(() => { try { action(); } finally { done.Set(); } return false; });
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
		_running = true;
		HookDispatcher();
		try
		{
			DoStartup(args);
			if (_pendingShutdown && !_shutdownStarted)
				DoShutdown(_exitCode);
			if (!_shutdownStarted)
			{
				windowToShow?.Show();
				RunLoop();
			}
			if (!_shutdownStarted)
				DoShutdown(_exitCode);
			return _exitCode;
		}
		finally
		{
			_running = false;
			UnhookDispatcher();
		}
	}

	/// <summary>Raises startup, then creates and shows the StartupUri window.</summary>
	internal void DoStartup(string[] args)
	{
		OnStartup(new StartupEventArgs(args));
		if (_shutdownStarted)
			return;
		// StartupUri is resolved after Startup so a handler can still set MainWindow itself.
		if (MainWindow == null && StartupUri != null)
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
