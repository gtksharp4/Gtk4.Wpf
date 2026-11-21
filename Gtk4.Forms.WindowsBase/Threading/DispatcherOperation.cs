using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Windows.Threading;

/// <summary>Represents work posted to a <see cref="Dispatcher"/> by <c>BeginInvoke</c> or <c>InvokeAsync</c>.</summary>
public class DispatcherOperation
{
	readonly object _sync = new object();
	readonly Delegate _method;
	readonly object?[]? _args;
	readonly TaskCompletionSource<object?> _completion = new TaskCompletionSource<object?>();
	ManualResetEventSlim? _done;
	DispatcherPriority _priority;
	DispatcherOperationStatus _status = DispatcherOperationStatus.Pending;
	object? _result;

	internal DispatcherOperation(Dispatcher dispatcher, Delegate method, DispatcherPriority priority, object?[]? args)
	{
		Dispatcher = dispatcher;
		_method = method;
		_priority = priority;
		_args = args;
	}

	/// <summary>The dispatcher this operation was posted to.</summary>
	public Dispatcher Dispatcher { get; }

	// Set by the dispatcher once the operation sits in its queue or was handed to the host bridge.
	internal bool Scheduled { get; set; }

	/// <summary>The priority of the operation. Raising an Inactive operation queues it.</summary>
	/// <exception cref="System.ComponentModel.InvalidEnumArgumentException">The value is not a valid priority.</exception>
	public DispatcherPriority Priority
	{
		get => _priority;
		set
		{
			Dispatcher.ValidatePriority(value, nameof(value), true);
			lock (_sync)
			{
				if (_status != DispatcherOperationStatus.Pending)
					return;
				_priority = value;
			}
			Dispatcher.Schedule(this);
		}
	}

	/// <summary>The state of the operation.</summary>
	public DispatcherOperationStatus Status
	{
		get { lock (_sync) return _status; }
	}

	/// <summary>The return value of the delegate once the operation completed, otherwise <c>null</c>.</summary>
	public object? Result => _result;

	/// <summary>A task that completes with the operation, or is cancelled when it is aborted.</summary>
	public Task Task => _completion.Task;

	internal Task<object?> Completion => _completion.Task;

	/// <summary>Raised when the operation is aborted.</summary>
	public event EventHandler? Aborted;

	/// <summary>Raised when the operation has run to completion.</summary>
	public event EventHandler? Completed;

	/// <summary>Gets an awaiter that completes with <see cref="Task"/>.</summary>
	public TaskAwaiter GetAwaiter() => Task.GetAwaiter();

	/// <summary>Removes the operation from the queue. Returns <c>false</c> unless it was still pending.</summary>
	public bool Abort()
	{
		lock (_sync)
		{
			if (_status != DispatcherOperationStatus.Pending)
				return false;
			_status = DispatcherOperationStatus.Aborted;
		}
		Dispatcher.Unqueue(this);
		try { Aborted?.Invoke(this, EventArgs.Empty); }
		finally
		{
			_completion.TrySetCanceled();
			SignalDone();
		}
		return true;
	}

	/// <summary>Waits for the operation to finish; on the dispatcher thread this runs a nested frame.</summary>
	public DispatcherOperationStatus Wait() => Wait(Timeout.InfiniteTimeSpan);

	/// <summary>Waits up to <paramref name="timeout"/> and returns the status reached.</summary>
	public DispatcherOperationStatus Wait(TimeSpan timeout)
	{
		var status = Status;
		if (status != DispatcherOperationStatus.Pending && status != DispatcherOperationStatus.Executing)
			return status;
		if (Dispatcher.CheckAccess())
		{
			if (status == DispatcherOperationStatus.Executing)
				throw new InvalidOperationException(
					"A thread cannot wait on operations already running on the same thread.");
			Dispatcher.WaitInFrame(this, timeout);
		}
		else
			EnsureDone().Wait(timeout);
		return Status;
	}

	/// <summary>Runs the delegate once. An exception no handler marks as handled is rethrown.</summary>
	internal void Execute()
	{
		lock (_sync)
		{
			if (_status != DispatcherOperationStatus.Pending)
				return;
			_status = DispatcherOperationStatus.Executing;
		}
		Exception? failure = null;
		try
		{
			_result = DispatcherInvoke.Call(_method, _args);
		}
		catch (Exception ex)
		{
			if (Dispatcher.RaiseUnhandledException(ex))
				return;
			failure = ex;
			throw;
		}
		finally
		{
			Finish(failure);
		}
	}

	void Finish(Exception? failure)
	{
		lock (_sync) _status = DispatcherOperationStatus.Completed;
		try { Completed?.Invoke(this, EventArgs.Empty); }
		finally
		{
			if (failure != null) _completion.TrySetException(failure);
			else _completion.TrySetResult(_result);
			SignalDone();
		}
	}

	ManualResetEventSlim EnsureDone()
	{
		lock (_sync)
		{
			if (_done == null)
				_done = new ManualResetEventSlim(_status != DispatcherOperationStatus.Pending
					&& _status != DispatcherOperationStatus.Executing);
			return _done;
		}
	}

	void SignalDone()
	{
		lock (_sync) _done?.Set();
	}
}
