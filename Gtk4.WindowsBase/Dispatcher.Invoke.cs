using System;
using System.ComponentModel;

namespace System.Windows.Threading;

public partial class Dispatcher
{
	// ── Synchronous ──────────────────────────────────────────────────────────────

	/// <summary>
	/// Synchronously invokes <paramref name="action"/> on the dispatcher thread.
	/// If already on the dispatcher thread, runs inline; otherwise marshals and waits.
	/// </summary>
	public void Invoke(Action action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));
		if (UsesOwnQueue) InvokeCore(DispatcherPriority.Send, action, null);
		else InvokeAction(action);
	}

	/// <summary>Synchronously invokes <paramref name="action"/> at <paramref name="priority"/>.</summary>
	public void Invoke(Action action, DispatcherPriority priority)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));
		InvokeCore(priority, action, null);
	}

	/// <summary>Synchronously invokes <paramref name="func"/> on the dispatcher thread and returns its result.</summary>
	public T Invoke<T>(Func<T> func)
	{
		if (func == null) throw new ArgumentNullException(nameof(func));
		if (UsesOwnQueue) return Cast<T>(InvokeCore(DispatcherPriority.Send, func, null));
		T result = default!;
		InvokeAction(() => { result = func(); });
		return result;
	}

	/// <summary>Synchronously invokes <paramref name="func"/> at <paramref name="priority"/>.</summary>
	public T Invoke<T>(Func<T> func, DispatcherPriority priority)
	{
		if (func == null) throw new ArgumentNullException(nameof(func));
		return Cast<T>(InvokeCore(priority, func, null));
	}

	/// <summary>
	/// Invokes <paramref name="method"/> with <paramref name="args"/> at Send priority and returns its result.
	/// </summary>
	public object? Invoke(Delegate method, params object?[]? args) =>
		InvokeCore(DispatcherPriority.Send, method, args);

	/// <summary>Invokes <paramref name="method"/> at <paramref name="priority"/> and returns its result.</summary>
	public object? Invoke(DispatcherPriority priority, Delegate method) =>
		InvokeCore(priority, method, null);

	/// <summary>Invokes <paramref name="method"/> with <paramref name="arg"/> at <paramref name="priority"/>.</summary>
	public object? Invoke(DispatcherPriority priority, Delegate method, object? arg) =>
		InvokeCore(priority, method, new[] { arg });

	/// <summary>Invokes <paramref name="method"/> with <paramref name="arg"/> then <paramref name="args"/>.</summary>
	public object? Invoke(DispatcherPriority priority, Delegate method, object? arg, params object?[]? args) =>
		InvokeCore(priority, method, Combine(arg, args));

	// ── Asynchronous ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Asynchronously posts <paramref name="action"/> to the dispatcher queue at the
	/// given <paramref name="priority"/>.
	/// </summary>
	/// <exception cref="InvalidEnumArgumentException">
	/// <paramref name="priority"/> is <see cref="DispatcherPriority.Invalid"/> or out of range.</exception>
	/// <remarks>An <see cref="DispatcherPriority.Inactive"/> operation stays pending until its priority is raised.</remarks>
	public DispatcherOperation BeginInvoke(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));
		return BeginInvokeCore(priority, action, null);
	}

	/// <summary>Posts <paramref name="method"/> with <paramref name="args"/> at Normal priority.</summary>
	public DispatcherOperation BeginInvoke(Delegate method, params object?[]? args) =>
		BeginInvokeCore(DispatcherPriority.Normal, method, args);

	/// <summary>Posts <paramref name="method"/> at <paramref name="priority"/>.</summary>
	public DispatcherOperation BeginInvoke(DispatcherPriority priority, Delegate method) =>
		BeginInvokeCore(priority, method, null);

	/// <summary>Posts <paramref name="method"/> with <paramref name="arg"/> at <paramref name="priority"/>.</summary>
	public DispatcherOperation BeginInvoke(DispatcherPriority priority, Delegate method, object? arg) =>
		BeginInvokeCore(priority, method, new[] { arg });

	/// <summary>Posts <paramref name="method"/> with <paramref name="arg"/> then <paramref name="args"/>.</summary>
	public DispatcherOperation BeginInvoke(
		DispatcherPriority priority, Delegate method, object? arg, params object?[]? args) =>
		BeginInvokeCore(priority, method, Combine(arg, args));

	/// <summary>Posts <paramref name="callback"/> at <paramref name="priority"/>.</summary>
	public DispatcherOperation InvokeAsync(Action callback, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		if (callback == null) throw new ArgumentNullException(nameof(callback));
		return BeginInvokeCore(priority, callback, null);
	}

	/// <summary>Posts <paramref name="callback"/> and returns an operation that yields its result.</summary>
	public DispatcherOperation<TResult> InvokeAsync<TResult>(
		Func<TResult> callback, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		if (callback == null) throw new ArgumentNullException(nameof(callback));
		ValidatePriority(priority, nameof(priority), true);
		var operation = new DispatcherOperation<TResult>(this, callback, priority);
		return (DispatcherOperation<TResult>)Post(operation);
	}

	// ── Core ─────────────────────────────────────────────────────────────────────

	internal static void ValidatePriority(DispatcherPriority priority, string name, bool allowInactive)
	{
		if (priority < DispatcherPriority.Inactive || priority > DispatcherPriority.Send
			|| (!allowInactive && priority == DispatcherPriority.Inactive))
			throw new InvalidEnumArgumentException(name, (int)priority, typeof(DispatcherPriority));
	}

	static T Cast<T>(object? value) => value is T typed ? typed : default!;

	static object?[] Combine(object? arg, object?[]? args)
	{
		if (args == null || args.Length == 0)
			return new[] { arg };
		var all = new object?[args.Length + 1];
		all[0] = arg;
		Array.Copy(args, 0, all, 1, args.Length);
		return all;
	}

	DispatcherOperation BeginInvokeCore(DispatcherPriority priority, Delegate method, object?[]? args)
	{
		if (method == null) throw new ArgumentNullException(nameof(method));
		ValidatePriority(priority, nameof(priority), true);
		return Post(new DispatcherOperation(this, method, priority, args));
	}

	DispatcherOperation Post(DispatcherOperation operation)
	{
		if (HasShutdownFinished)
			operation.Abort();
		else
			Schedule(operation);
		return operation;
	}

	object? InvokeCore(DispatcherPriority priority, Delegate method, object?[]? args)
	{
		if (method == null) throw new ArgumentNullException(nameof(method));
		ValidatePriority(priority, nameof(priority), false);
		var operation = new DispatcherOperation(this, method, priority, args);
		if (priority == DispatcherPriority.Send && CheckAccess() && !HasShutdownFinished)
			operation.Execute();
		else
		{
			Post(operation);
			operation.Wait();
		}
		return operation.Result;
	}
}
