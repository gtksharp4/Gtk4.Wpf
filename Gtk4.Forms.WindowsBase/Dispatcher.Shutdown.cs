using System;

namespace System.Windows.Threading;

public partial class Dispatcher
{
	volatile bool _shutdownStarted;
	volatile bool _shutdownFinished;

	/// <summary>Raised when the dispatcher starts shutting down, before its frames end.</summary>
	public event EventHandler? ShutdownStarted;

	/// <summary>Raised once the dispatcher finished shutting down and pending work was aborted.</summary>
	public event EventHandler? ShutdownFinished;

	/// <summary>Whether shutdown began.</summary>
	public bool HasShutdownStarted => _shutdownStarted;

	/// <summary>Whether shutdown completed; the dispatcher then accepts no new work.</summary>
	public bool HasShutdownFinished => _shutdownFinished;

	/// <summary>Synchronously starts shutting the dispatcher down: running frames end, pending work is aborted.</summary>
	public void InvokeShutdown() => Invoke(DispatcherPriority.Send, new Action(StartShutdown));

	/// <summary>Posts a shutdown request at <paramref name="priority"/>.</summary>
	public void BeginInvokeShutdown(DispatcherPriority priority) =>
		BeginInvoke(priority, new Action(StartShutdown));

	void StartShutdown()
	{
		if (_shutdownStarted)
			return;
		_shutdownStarted = true;
		ShutdownStarted?.Invoke(this, EventArgs.Empty);
		WakeFrames();
		// With no frame running nothing will unwind, so finish now.
		if (_frameDepth == 0)
			FinishShutdown();
	}

	void FinishShutdown()
	{
		if (_shutdownFinished)
			return;
		_shutdownFinished = true;
		ShutdownFinished?.Invoke(this, EventArgs.Empty);
		AbortQueued();
	}
}
