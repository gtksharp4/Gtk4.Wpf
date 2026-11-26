using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Windows.Threading;

/// <summary>
/// Provides services for managing the queue of work items for a thread.
/// Bridges to the GLib main loop on the GTK thread.
/// </summary>
/// <remarks>
/// <para>
/// Each thread has its own <see cref="Dispatcher"/> accessed via <see cref="CurrentDispatcher"/>.
/// The presentation layer marks the GTK thread's dispatcher as the host: its work goes through the
/// static bridges below. Any other dispatcher keeps its own priority queue, pumped by
/// <see cref="Run"/> or <see cref="PushFrame"/> on its thread.
/// </para>
/// <para>
/// <b>Thread contract.</b> <see cref="InvokeAction"/> is set by the presentation layer once
/// <c>Gtk.Application.Init()</c> has run. Until that point (and in pure unit tests that never
/// start a GLib loop) every invocation runs inline on the calling thread.
/// </para>
/// <para>
/// <b>Limitation.</b> GTK is single-threaded, so widgets created on a secondary dispatcher thread are
/// unsupported; such a thread can run its own queue, but nothing renders its windows.
/// </para>
/// </remarks>
public partial class Dispatcher
{
	// ── Static GTK bridge ────────────────────────────────────────────────────────

	/// <summary>
	/// Delegate executed by <see cref="Invoke(Action)"/> to marshal work onto the GTK thread.
	/// The default implementation runs the action inline (suitable for pure unit tests).
	/// </summary>
	public static Action<Action> InvokeAction { get; set; } = action => action();

	static Action<Action> _beginInvokeAction = action => action();

	/// <summary>
	/// Legacy, priority-less bridge that <see cref="BeginInvoke(Action,DispatcherPriority)"/> uses
	/// only when <see cref="BeginInvokePriorityAction"/> is <c>null</c>. Assigning it clears
	/// <see cref="BeginInvokePriorityAction"/>, so the last bridge set wins. The default runs the
	/// action inline.
	/// </summary>
	public static Action<Action> BeginInvokeAction
	{
		get => _beginInvokeAction;
		set
		{
			_beginInvokeAction = value;
			BeginInvokePriorityAction = null;
		}
	}

	/// <summary>
	/// Priority-aware bridge. When non-null, <see cref="BeginInvoke(Action,DispatcherPriority)"/>
	/// uses it and passes the validated priority; setting <see cref="BeginInvokeAction"/> clears it.
	/// </summary>
	public static Action<Action, DispatcherPriority>? BeginInvokePriorityAction { get; set; }

	/// <summary>
	/// Predicate that returns <c>true</c> when the caller is on the GTK / dispatcher thread.
	/// The default considers every thread as the dispatcher thread.
	/// </summary>
	public static Func<bool> IsOnDispatcherThread { get; set; } = () => true;

	// ── Per-thread dispatchers ───────────────────────────────────────────────────

	[ThreadStatic]
	static Dispatcher? _current;

	static readonly ConditionalWeakTable<Thread, Dispatcher> _byThread =
		new ConditionalWeakTable<Thread, Dispatcher>();

	static volatile Dispatcher? _host;

	/// <summary>Returns the <see cref="Dispatcher"/> for the calling thread, creating it on first use.</summary>
	public static Dispatcher CurrentDispatcher
	{
		get
		{
			var dispatcher = _current;
			if (dispatcher == null)
			{
				dispatcher = new Dispatcher(Thread.CurrentThread);
				_byThread.Add(dispatcher._thread, dispatcher);
				_current = dispatcher;
			}
			return dispatcher;
		}
	}

	/// <summary>Returns the dispatcher of <paramref name="thread"/>, or <c>null</c> when it has none.</summary>
	public static Dispatcher? FromThread(Thread thread) =>
		thread != null && _byThread.TryGetValue(thread, out var dispatcher) ? dispatcher : null;

	/// <summary>Marks the calling thread's dispatcher as the GTK host, served by the static bridges.</summary>
	internal static void AttachHost() => _host = CurrentDispatcher;

	internal static void DetachHost() => _host = null;

	/// <summary>The GTK host's dispatcher (the UI thread), or null when no GTK loop is attached.</summary>
	internal static Dispatcher? Host => _host;

	// ── Instance ─────────────────────────────────────────────────────────────────

	readonly Thread _thread;

	Dispatcher(Thread thread)
	{
		_thread = thread;
	}

	/// <summary>The thread this dispatcher is associated with.</summary>
	public Thread Thread => _thread;

	bool IsHost => ReferenceEquals(this, _host);

	// A dispatcher owns its queue unless it serves the GTK host or no host exists and it never pumped,
	// the pure-test default where every call runs inline.
	bool UsesOwnQueue => !IsHost && (_host != null || _everRan);

	/// <summary>Returns <c>true</c> when the calling thread is the dispatcher's thread.</summary>
	public bool CheckAccess() =>
		UsesOwnQueue ? Thread.CurrentThread == _thread : IsOnDispatcherThread();

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
}
