using System;

namespace System.Windows.Threading;

/// <summary>Base arguments for events raised by a <see cref="Dispatcher"/>.</summary>
public class DispatcherEventArgs : EventArgs
{
	internal DispatcherEventArgs(Dispatcher dispatcher)
	{
		Dispatcher = dispatcher;
	}

	/// <summary>The dispatcher that raised the event.</summary>
	public Dispatcher Dispatcher { get; }
}
