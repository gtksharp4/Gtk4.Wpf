using System;
using System.Collections.Generic;

namespace System.Windows;

public partial class Application
{
	readonly List<Window> _windows = new List<Window>();
	bool _shutdownStarted;
	bool _pendingShutdown;
	bool _shutdownRequested;
	int _exitCode;

	/// <summary>Raised once while the application shuts down; a handler may change the exit code.</summary>
	public event ExitEventHandler? Exit;

	/// <summary>When the application shuts down automatically. Default <see cref="ShutdownMode.OnLastWindowClose"/>.</summary>
	public ShutdownMode ShutdownMode { get; set; } = ShutdownMode.OnLastWindowClose;

	/// <summary>A snapshot of the windows created on this application's thread that are still open.</summary>
	public WindowCollection Windows => new WindowCollection(_windows);

	/// <summary>Raises <see cref="Exit"/>.</summary>
	protected virtual void OnExit(ExitEventArgs e) => Exit?.Invoke(this, e);

	/// <summary>Shuts the application down with exit code 0.</summary>
	public void Shutdown() => Shutdown(0);

	/// <summary>
	/// Requests shutdown like WPF: while running it is asynchronous, so windows close, <see cref="Exit"/>
	/// is raised and the main loop ends later on the dispatcher; the first request wins. Before
	/// <c>Run</c> it only records the request, which <c>Run</c> honours right after <see cref="Startup"/>.
	/// </summary>
	/// <exception cref="InvalidOperationException">Called from a thread other than the application's.</exception>
	public void Shutdown(int exitCode)
	{
		if (Dispatcher.Thread != System.Threading.Thread.CurrentThread)
			throw new InvalidOperationException(
				"The calling thread cannot access this object because a different thread owns it.");
		if (_shutdownRequested || _shutdownStarted)
			return;
		if (!_running)
		{
			_pendingShutdown = true;
			_exitCode = exitCode;
			return;
		}
		_shutdownRequested = true;
		_exitCode = exitCode;
		Dispatcher.BeginInvoke(new Action(() =>
		{
			if (!_shutdownStarted) DoShutdown(_exitCode);
		}));
	}

	void DoShutdown(int exitCode)
	{
		_shutdownStarted = true;
		_exitCode = exitCode;
		// Windows closing re-enter Shutdown/OnWindowClosed; _shutdownStarted makes both inert.
		foreach (var window in _windows.ToArray())
			window.CloseForShutdown();
		var args = new ExitEventArgs(exitCode);
		try { OnExit(args); }
		finally { _exitCode = args.ApplicationExitCode; }
		if (_loop != null && _loop.IsRunning)
			_loop.Quit();
	}

	// ── Window registration (called from Window) ─────────────────────────────

	internal void RegisterWindow(Window window)
	{
		if (_runCompleted || _shutdownStarted) return;
		if (Dispatcher.Thread != System.Threading.Thread.CurrentThread)
			return;
		_windows.Add(window);
		MainWindow ??= window;
	}

	/// <summary>Removes a closed window, then applies the <see cref="ShutdownMode"/> policy.</summary>
	internal void OnWindowClosed(Window window)
	{
		if (!_windows.Remove(window))
			return;
		DeactivateWindow(window);
		bool closingIsMain = ReferenceEquals(window, MainWindow);
		if (closingIsMain)
			MainWindow = null;
		if (!_shutdownStarted && !_shutdownRequested && _running
			&& ShutdownPolicy.ShouldShutdown(ShutdownMode, closingIsMain, _windows.Count))
			Shutdown();
	}
}
