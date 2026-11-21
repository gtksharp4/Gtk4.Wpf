using System;
using System.Threading.Tasks;

namespace System.Windows.Threading;

/// <summary>A <see cref="DispatcherOperation"/> whose delegate returns a typed value.</summary>
/// <typeparam name="TResult">The type the delegate returns.</typeparam>
public class DispatcherOperation<TResult> : DispatcherOperation
{
	Task<TResult>? _typedTask;

	internal DispatcherOperation(Dispatcher dispatcher, Func<TResult> method, DispatcherPriority priority)
		: base(dispatcher, method, priority, null)
	{
	}

	/// <summary>A task that yields the typed result.</summary>
	public new Task<TResult> Task =>
		_typedTask ??= Completion.ContinueWith(t => (TResult)t.GetAwaiter().GetResult()!, TaskContinuationOptions.ExecuteSynchronously);

	/// <summary>The value the delegate returned once the operation completed.</summary>
	public new TResult Result
	{
		get
		{
			Wait();
			return Task.Result;
		}
	}

	/// <summary>Gets an awaiter that completes with the typed result.</summary>
	public new System.Runtime.CompilerServices.TaskAwaiter<TResult> GetAwaiter() => Task.GetAwaiter();
}
