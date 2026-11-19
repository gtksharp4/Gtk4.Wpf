using System;

namespace System.Windows.Threading;

/// <summary>Arguments for <see cref="Dispatcher.UnhandledException"/>.</summary>
public sealed class DispatcherUnhandledExceptionEventArgs : DispatcherEventArgs
{
	internal DispatcherUnhandledExceptionEventArgs(Dispatcher dispatcher, Exception exception)
		: base(dispatcher)
	{
		Exception = exception;
	}

	/// <summary>The exception that escaped a dispatcher callback.</summary>
	public Exception Exception { get; }

	/// <summary>Set to <see langword="true"/> to swallow the exception and keep running.</summary>
	public bool Handled { get; set; }
}

/// <summary>Handles <see cref="Dispatcher.UnhandledException"/>.</summary>
public delegate void DispatcherUnhandledExceptionEventHandler(
	object sender, DispatcherUnhandledExceptionEventArgs e);
