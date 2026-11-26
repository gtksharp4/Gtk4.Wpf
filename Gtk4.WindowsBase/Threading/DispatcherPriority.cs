namespace System.Windows.Threading;

/// <summary>
/// Specifies the priority at which an operation is invoked on a <see cref="Dispatcher"/>.
/// </summary>
public enum DispatcherPriority
{
	/// <summary>No priority; the operation is not processed.</summary>
	Invalid = -1,
	/// <summary>The operation is not associated with processing of events.</summary>
	Inactive = 0,
	/// <summary>The operation is processed after all other non-idle operations.</summary>
	SystemIdle = 1,
	/// <summary>The operation is processed when the application is idle.</summary>
	ApplicationIdle = 2,
	/// <summary>The operation is processed after background operations.</summary>
	ContextIdle = 3,
	/// <summary>The operation is processed after all other operations.</summary>
	Background = 4,
	/// <summary>The operation is processed at the same priority as input.</summary>
	Input = 5,
	/// <summary>The operation is processed at the same priority as loaded events.</summary>
	Loaded = 6,
	/// <summary>The operation is processed at the same priority as render operations.</summary>
	Render = 7,
	/// <summary>The operation is processed at the same priority as data-binding operations.</summary>
	DataBind = 8,
	/// <summary>The operation is processed at normal priority.</summary>
	Normal = 9,
	/// <summary>The operation is processed before all other asynchronous operations.</summary>
	Send = 10,
}