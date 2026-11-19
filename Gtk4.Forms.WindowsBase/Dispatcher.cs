using System;
using System.Threading;

namespace System.Windows.Threading;

/// <summary>
/// Provides services for managing the queue of work items for a thread.
/// Bridges to the GLib main loop on the GTK thread.
/// </summary>
/// <remarks>
/// <para>
/// Each thread has its own <see cref="Dispatcher"/> accessed via <see cref="CurrentDispatcher"/>.
/// In practice the GTK back-end uses a single dispatcher that is thread-affine to the GTK thread.
/// </para>
/// <para>
/// <b>Thread contract.</b> <see cref="InvokeAction"/> is set by the presentation layer once
/// <c>Gtk.Application.Init()</c> has run. Until that point (and in pure unit tests that never
/// start a GLib loop) every invocation runs the action inline on the calling thread.
/// </para>
/// </remarks>
public class Dispatcher
{
	// ── Static GTK bridge ────────────────────────────────────────────────────────

	/// <summary>
	/// Delegate executed by <see cref="Invoke(Action)"/> to marshal work onto the GTK thread.
	/// Replace with a GTK-aware implementation once the GLib loop is running:
	/// <code>
	/// Dispatcher.InvokeAction = action => { /* post to GTK thread and wait */ };
	/// </code>
	/// The default implementation runs the action inline (suitable for pure unit tests).
	/// </summary>
	public static Action<Action> InvokeAction { get; set; } = action => action();

	/// <summary>
	/// Delegate executed by <see cref="BeginInvoke(Action,DispatcherPriority)"/> to post
	/// work asynchronously onto the GTK main loop.
	/// Replace with <c>GLib.Idle.Add</c> (or equivalent) once the GLib loop is running.
	/// The default implementation runs the action inline.
	/// </summary>
	public static Action<Action> BeginInvokeAction { get; set; } = action => action();

	/// <summary>
	/// Predicate that returns <c>true</c> when the caller is on the GTK / dispatcher thread.
	/// Replace with a GLib-thread check once the GLib loop is running.
	/// The default considers every thread as the dispatcher thread.
	/// </summary>
	public static Func<bool> IsOnDispatcherThread { get; set; } = () => true;

	// ── Per-thread singleton ─────────────────────────────────────────────────────

	[ThreadStatic]
	static Dispatcher? _current;

	static readonly Dispatcher _main = new Dispatcher(Thread.CurrentThread);

	/// <summary>
	/// Returns the <see cref="Dispatcher"/> for the calling thread.
	/// If no dispatcher has been created for this thread yet, one is created and returned.
	/// </summary>
	public static Dispatcher CurrentDispatcher
	{
		get
		{
			if (_current == null)
				_current = new Dispatcher(Thread.CurrentThread);
			return _current;
		}
	}

	// ── Instance ─────────────────────────────────────────────────────────────────

	readonly Thread _thread;

	Dispatcher(Thread thread)
	{
		_thread = thread;
	}

	/// <summary>
	/// The thread this dispatcher is associated with.
	/// </summary>
	public Thread Thread => _thread;

	/// <summary>
	/// Returns <c>true</c> when the calling thread is the dispatcher's thread.
	/// Uses the static <see cref="IsOnDispatcherThread"/> predicate.
	/// </summary>
	public bool CheckAccess() => IsOnDispatcherThread();

	/// <summary>
	/// Throws <see cref="InvalidOperationException"/> if the calling thread is not the
	/// dispatcher thread.
	/// </summary>
	public void VerifyAccess()
	{
		if (!CheckAccess())
			throw new InvalidOperationException(
				"The calling thread cannot access this object because a different thread owns it.");
	}

	/// <summary>
	/// Synchronously invokes <paramref name="action"/> on the dispatcher thread.
	/// If already on the dispatcher thread, runs inline; otherwise marshals and waits.
	/// </summary>
	public void Invoke(Action action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));
		InvokeAction(action);
	}

	/// <summary>
	/// Synchronously invokes <paramref name="func"/> on the dispatcher thread and returns
	/// its result.
	/// </summary>
	public T Invoke<T>(Func<T> func)
	{
		if (func == null) throw new ArgumentNullException(nameof(func));
		T result = default!;
		InvokeAction(() => { result = func(); });
		return result;
	}

	/// <summary>
	/// Asynchronously posts <paramref name="action"/> to the dispatcher queue at the
	/// given <paramref name="priority"/>.
	/// </summary>
	public void BeginInvoke(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));
		BeginInvokeAction(() =>
		{
			try { action(); }
			catch (Exception ex)
			{
				if (!RaiseUnhandledException(ex))
					throw;
			}
		});
	}

	// ── Unhandled exceptions ─────────────────────────────────────────────────────

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