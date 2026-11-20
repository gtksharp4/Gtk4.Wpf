using System;
using System.Windows.Threading;

namespace System.Windows;

public partial class Application
{
	static bool _glibHooked;

	/// <summary>
	/// Raised for an exception that escapes a dispatcher callback. Like WPF it forwards to
	/// <see cref="Dispatcher.UnhandledException"/>: handlers get the dispatcher as sender, run in
	/// subscription order, all run after one sets Handled, and are active from subscription.
	/// </summary>
	public event DispatcherUnhandledExceptionEventHandler? DispatcherUnhandledException
	{
		add => Dispatcher.UnhandledException += value;
		remove => Dispatcher.UnhandledException -= value;
	}

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
		// GLib's callback marshaller wraps the original in TargetInvocationException.
		while (ex is System.Reflection.TargetInvocationException { InnerException: { } inner })
			ex = inner;
		// Already reported by Dispatcher.BeginInvoke / DispatcherTimer and rethrown: terminate.
		var handled = !ex.Data.Contains(Dispatcher.ReportedKey)
			&& Dispatcher.CurrentDispatcher.RaiseUnhandledException(ex);
		args.ExitApplication = !handled;
		// GLib exits without printing, so say why the process is going away.
		if (!handled)
			Console.Error.WriteLine("Unhandled exception in a GTK callback. " + ex);
	}
}
