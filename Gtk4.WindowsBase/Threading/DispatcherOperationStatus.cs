namespace System.Windows.Threading;

/// <summary>Describes the state of a <see cref="DispatcherOperation"/>.</summary>
public enum DispatcherOperationStatus
{
	/// <summary>The operation is waiting in the queue.</summary>
	Pending,

	/// <summary>The operation was removed before it ran.</summary>
	Aborted,

	/// <summary>The operation ran to completion.</summary>
	Completed,

	/// <summary>The operation is running.</summary>
	Executing,
}
