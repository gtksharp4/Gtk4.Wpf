using System;

namespace System.Windows.Input;

/// <summary>
/// Binds an <see cref="ICommand"/> to handlers on a specific element.
/// Attach to <see cref="System.Windows.UIElement.CommandBindings"/> so
/// <see cref="CommandManager"/> can route <c>Execute</c>/<c>CanExecute</c> calls.
/// </summary>
public sealed class CommandBinding
{
	/// <summary>The command this binding handles.</summary>
	public ICommand Command { get; }

	/// <summary>Raised when the command is executed on or bubbles to the owner element.</summary>
	public event ExecutedRoutedEventHandler? Executed;

	/// <summary>Raised to query whether the command can execute.</summary>
	public event CanExecuteRoutedEventHandler? CanExecute;

	/// <param name="command">The command to handle.</param>
	public CommandBinding(ICommand command)
	{
		Command = command ?? throw new ArgumentNullException(nameof(command));
	}

	/// <param name="command">The command to handle.</param>
	/// <param name="executed">Handler invoked on execution.</param>
	public CommandBinding(ICommand command, ExecutedRoutedEventHandler executed)
		: this(command)
	{
		Executed += executed;
	}

	/// <param name="command">The command to handle.</param>
	/// <param name="executed">Handler invoked on execution.</param>
	/// <param name="canExecute">Handler invoked to query can-execute state.</param>
	public CommandBinding(
		ICommand command,
		ExecutedRoutedEventHandler executed,
		CanExecuteRoutedEventHandler canExecute)
		: this(command, executed)
	{
		CanExecute += canExecute;
	}

	// ── Internal invocation ───────────────────────────────────────────────────

	/// <summary>
	/// Invokes <see cref="Executed"/> if subscribed and returns whether it was handled.
	/// </summary>
	internal bool InvokeExecuted(object sender, ExecutedRoutedEventArgs args)
	{
		if (Executed == null) return false;
		Executed(sender, args);
		return true;
	}

	/// <summary>
	/// Invokes <see cref="CanExecute"/> if subscribed and returns the result.
	/// When no handler is subscribed the command is assumed executable.
	/// </summary>
	internal bool InvokeCanExecute(object sender, CanExecuteRoutedEventArgs args)
	{
		if (CanExecute == null)
		{
			args.CanExecute = true;
			return true;
		}
		CanExecute(sender, args);
		return true;
	}
}