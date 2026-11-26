using System;

namespace System.Windows.Input;

/// <summary>Delegate for the <see cref="CommandBinding.Executed"/> event.</summary>
public delegate void ExecutedRoutedEventHandler(object sender, ExecutedRoutedEventArgs e);

/// <summary>Delegate for the <see cref="CommandBinding.CanExecute"/> event.</summary>
public delegate void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e);

/// <summary>
/// Event-args delivered to a <see cref="CommandBinding"/> <c>Executed</c> handler.
/// </summary>
public sealed class ExecutedRoutedEventArgs : System.Windows.RoutedEventArgs
{
	/// <summary>The command that was executed.</summary>
	public ICommand Command { get; }

	/// <summary>The parameter passed to <see cref="ICommand.Execute"/>.</summary>
	public object? Parameter { get; }

	internal ExecutedRoutedEventArgs(
		System.Windows.RoutedEvent routedEvent,
		ICommand command,
		object? parameter)
		: base(routedEvent)
	{
		Command = command;
		Parameter = parameter;
	}
}

/// <summary>
/// Event-args delivered to a <see cref="CommandBinding"/> <c>CanExecute</c> handler.
/// </summary>
public sealed class CanExecuteRoutedEventArgs : System.Windows.RoutedEventArgs
{
	/// <summary>The command being queried.</summary>
	public ICommand Command { get; }

	/// <summary>The parameter passed to <see cref="ICommand.CanExecute"/>.</summary>
	public object? Parameter { get; }

	/// <summary>
	/// Set to <c>true</c> by the handler to indicate the command can execute.
	/// </summary>
	public bool CanExecute { get; set; }

	internal CanExecuteRoutedEventArgs(
		System.Windows.RoutedEvent routedEvent,
		ICommand command,
		object? parameter)
		: base(routedEvent)
	{
		Command = command;
		Parameter = parameter;
	}
}