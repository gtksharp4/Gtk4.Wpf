using System;
using System.Windows.Threading;

namespace System.Windows;

public partial class Application
{
	static bool _glibHooked;

	/// <summary>
	/// Raised for an exception that escapes a dispatcher callback, after
	/// <see cref="Dispatcher.UnhandledException"/>. Handled = true keeps the app running;
	/// otherwise the process terminates.
	/// </summary>
	public event DispatcherUnhandledExceptionEventHandler? DispatcherUnhandledException;

	void HookDispatcher() => Dispatcher.ApplicationUnhandledException += OnDispatcherUnhandled;

	void UnhookDispatcher() => Dispatcher.ApplicationUnhandledException -= OnDispatcherUnhandled;

	void OnDispatcherUnhandled(object sender, DispatcherUnhandledExceptionEventArgs e) =>
		DispatcherUnhandledException?.Invoke(this, e);

	/// <summary>
	/// Routes exceptions that escape GTK signal callbacks (which never pass through
	/// <c>Dispatcher.BeginInvoke</c>) into the dispatcher. Hooked once per process.
	/// </summary>
	internal static void EnsureGlibExceptionHook()
	{
		if (_glibHooked)
			return;
		_glibHooked = true;
		GLib.ExceptionManager.UnhandledException += OnGlibUnhandled;
	}

	internal static void OnGlibUnhandled(GLib.UnhandledExceptionArgs args)
	{
		if (!(args.ExceptionObject is Exception ex))
			return;
		// Already reported by Dispatcher.BeginInvoke / DispatcherTimer and rethrown: terminate.
		if (ex.Data.Contains(Dispatcher.ReportedKey))
		{
			args.ExitApplication = true;
			return;
		}
		args.ExitApplication = !Dispatcher.CurrentDispatcher.RaiseUnhandledException(ex);
	}
}
