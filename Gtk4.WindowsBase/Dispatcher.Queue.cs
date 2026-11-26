using System;
using System.Collections.Generic;
using System.Threading;

namespace System.Windows.Threading;

public partial class Dispatcher
{
	readonly object _queueLock = new object();
	readonly List<DispatcherOperation> _queue = new List<DispatcherOperation>();
	bool _everRan;

	// Hands an operation to the host bridge or to this dispatcher's own queue. Inactive work is held back.
	internal void Schedule(DispatcherOperation operation)
	{
		if (operation.Priority == DispatcherPriority.Inactive || operation.Status != DispatcherOperationStatus.Pending)
			return;
		if (UsesOwnQueue)
		{
			lock (_queueLock)
			{
				if (!operation.Scheduled)
				{
					operation.Scheduled = true;
					_queue.Add(operation);
				}
				Monitor.PulseAll(_queueLock);
			}
			return;
		}
		lock (_queueLock)
		{
			if (operation.Scheduled)
				return;
			operation.Scheduled = true;
		}
		var bridge = BeginInvokePriorityAction;
		if (bridge != null) bridge(operation.Execute, operation.Priority);
		else BeginInvokeAction(operation.Execute);
	}

	internal void Unqueue(DispatcherOperation operation)
	{
		lock (_queueLock)
		{
			_queue.Remove(operation);
			Monitor.PulseAll(_queueLock);
		}
	}

	// Highest priority first, first in first out within a priority. Caller holds the lock.
	DispatcherOperation? TakeNext()
	{
		DispatcherOperation? best = null;
		foreach (var operation in _queue)
		{
			if (operation.Priority > DispatcherPriority.Inactive && (best == null || operation.Priority > best.Priority))
				best = operation;
		}
		if (best != null)
			_queue.Remove(best);
		return best;
	}

	void AbortQueued()
	{
		DispatcherOperation[] pending;
		lock (_queueLock)
		{
			pending = _queue.ToArray();
			_queue.Clear();
		}
		foreach (var operation in pending)
			operation.Abort();
	}

	// Runs queued operations until the frame stops. An unhandled exception leaves the loop, as in WPF.
	void RunQueue(DispatcherFrame frame)
	{
		while (true)
		{
			DispatcherOperation? next = null;
			lock (_queueLock)
			{
				while (frame.Continue)
				{
					next = TakeNext();
					if (next != null)
						break;
					Monitor.Wait(_queueLock);
				}
			}
			if (next == null)
				return;
			next.Execute();
		}
	}

	internal void WakeFrames()
	{
		lock (_queueLock) Monitor.PulseAll(_queueLock);
		DispatcherFrame[] frames;
		lock (_queueLock) frames = _frames.ToArray();
		foreach (var frame in frames)
			frame.NotifyContinueChanged();
	}
}
