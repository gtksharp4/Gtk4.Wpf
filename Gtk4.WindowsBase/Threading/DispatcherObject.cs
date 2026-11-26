using System;

namespace System.Windows.Threading;

/// <summary>
/// Base class for objects that are associated with a <see cref="Dispatcher"/>.
/// Provides <see cref="CheckAccess"/> and <see cref="VerifyAccess"/> to enforce
/// thread-affinity contracts.
/// </summary>
public abstract class DispatcherObject
{
	/// <summary>
	/// The <see cref="Dispatcher"/> this object is associated with.
	/// Captured at construction time from <see cref="Dispatcher.CurrentDispatcher"/>.
	/// </summary>
	public Dispatcher Dispatcher { get; } = Dispatcher.CurrentDispatcher;

	/// <summary>
	/// Returns <c>true</c> when the calling thread has access to this object.
	/// Delegates to <see cref="Dispatcher.CheckAccess"/>.
	/// </summary>
	public bool CheckAccess() => Dispatcher.CheckAccess();

	/// <summary>
	/// Throws <see cref="InvalidOperationException"/> if the calling thread does not have
	/// access to this object.
	/// </summary>
	public void VerifyAccess() => Dispatcher.VerifyAccess();
}