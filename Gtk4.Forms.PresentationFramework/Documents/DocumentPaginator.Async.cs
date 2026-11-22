using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;

namespace System.Windows.Documents;

public abstract partial class DocumentPaginator
{
	readonly List<PendingCall> _pending = new List<PendingCall>();

	sealed class PendingCall
	{
		internal PendingCall(object state) => State = state;

		internal object State { get; }

		internal bool Cancelled { get; set; }
	}

	/// <summary>Gets a page without blocking; <see cref="GetPageCompleted"/> carries this paginator as state.</summary>
	public virtual void GetPageAsync(int pageNumber) => GetPageAsync(pageNumber, this);

	/// <summary>Gets a page without blocking; completion is raised from the dispatcher.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="userState"/> is null.</exception>
	public virtual void GetPageAsync(int pageNumber, object userState)
	{
		if (userState == null) throw new ArgumentNullException(nameof(userState));
		Post(userState, cancelled =>
		{
			DocumentPage? page = null;
			Exception? error = null;
			if (!cancelled)
			{
				try { page = GetPage(pageNumber); }
				catch (Exception ex) { error = ex; }
			}
			OnGetPageCompleted(new GetPageCompletedEventArgs(page, pageNumber, error, cancelled, userState));
		});
	}

	/// <summary>Computes the page count without blocking; the state is this paginator.</summary>
	public virtual void ComputePageCountAsync() => ComputePageCountAsync(this);

	/// <summary>Computes the page count without blocking; completion is raised from the dispatcher.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="userState"/> is null.</exception>
	public virtual void ComputePageCountAsync(object userState)
	{
		if (userState == null) throw new ArgumentNullException(nameof(userState));
		Post(userState, cancelled =>
		{
			Exception? error = null;
			if (!cancelled)
			{
				try { ComputePageCount(); }
				catch (Exception ex) { error = ex; }
			}
			OnComputePageCountCompleted(new AsyncCompletedEventArgs(error, cancelled, userState));
		});
	}

	/// <summary>Cancels the pending async calls started with <paramref name="userState"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="userState"/> is null.</exception>
	public virtual void CancelAsync(object userState)
	{
		if (userState == null) throw new ArgumentNullException(nameof(userState));
		lock (_pending)
		{
			foreach (var call in _pending)
				if (Equals(call.State, userState)) call.Cancelled = true;
		}
	}

	// Each call is tracked on its own: CancelAsync marks every outstanding call with that state, and a repeated
	// state never cancels the other calls that share it.
	internal void Post(object userState, Action<bool> work)
	{
		var call = new PendingCall(userState);
		lock (_pending) _pending.Add(call);
		Dispatcher.CurrentDispatcher.BeginInvoke(() =>
		{
			bool cancelled;
			lock (_pending)
			{
				cancelled = call.Cancelled;
				_pending.Remove(call);
			}
			work(cancelled);
		}, DispatcherPriority.Normal);
	}
}
