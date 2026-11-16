using System;
using System.Threading;

namespace System.Windows.Threading
{
	/// <summary>
	/// Minimal <c>Dispatcher</c> for Phase 0.  By default both <see cref="Invoke"/> and
	/// <see cref="BeginInvoke"/> run the action inline on the calling thread.
	/// The GTK presentation layer will replace <see cref="InvokeAction"/> with a delegate
	/// that marshals work onto the GTK main thread, without any API change to consumers.
	/// </summary>
	/// <remarks>
	/// Real WPF's <c>Dispatcher</c> is thread-affine; this implementation is not (Phase 0).
	/// Every thread gets the same singleton via <see cref="CurrentDispatcher"/>.  Threading
	/// semantics will be tightened in Phase 1 when the GTK main loop is wired.
	/// </remarks>
	public class Dispatcher
	{
		/// <summary>
		/// Action invoked by <see cref="Invoke"/> and <see cref="BeginInvoke"/>.
		/// Replace this with a GTK-main-thread marshaller from the presentation layer:
		/// <code>
		/// Dispatcher.InvokeAction = action => GLib.Idle.Add(() => { action(); return false; });
		/// </code>
		/// </summary>
		public static Action<Action> InvokeAction { get; set; } = action => action();

		static readonly Dispatcher _instance = new Dispatcher();

		/// <summary>Returns the dispatcher for the current thread (Phase-0: always the singleton).</summary>
		public static Dispatcher CurrentDispatcher => _instance;

		/// <summary>
		/// Synchronously invokes <paramref name="action"/> (Phase 0: inline on calling thread,
		/// or through <see cref="InvokeAction"/> if replaced by the presentation layer).
		/// </summary>
		public void Invoke(Action action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));
			InvokeAction(action);
		}

		/// <summary>
		/// Schedules <paramref name="action"/> for asynchronous execution (Phase 0: runs inline).
		/// </summary>
		public void BeginInvoke(Action action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));
			InvokeAction(action);
		}
	}
}
