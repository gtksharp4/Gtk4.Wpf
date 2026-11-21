namespace System.Windows.Threading;

public partial class Dispatcher
{
	/// <summary>
	/// Raised when an exception escapes a callback posted through this dispatcher. Handlers run in
	/// subscription order, all of them even after one sets <see cref="DispatcherUnhandledExceptionEventArgs.Handled"/>
	/// (which keeps the app running). <c>Application.DispatcherUnhandledException</c> forwards here.
	/// </summary>
	public event DispatcherUnhandledExceptionEventHandler? UnhandledException;

	// Marks an exception already reported so GLib's own exception hook does not raise it twice.
	internal const string ReportedKey = "Gtk4.Forms.DispatcherReported";

	/// <summary>
	/// Raises <see cref="UnhandledException"/> and returns whether a handler set Handled.
	/// An unhandled exception is tagged so the GLib fallback hook skips it.
	/// </summary>
	internal bool RaiseUnhandledException(Exception exception)
	{
		var args = new DispatcherUnhandledExceptionEventArgs(this, exception);
		UnhandledException?.Invoke(this, args);
		if (!args.Handled)
			exception.Data[ReportedKey] = true;
		return args.Handled;
	}
}
