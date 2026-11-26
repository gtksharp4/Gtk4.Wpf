using System;

namespace System.Windows.Input;

/// <summary>
/// Shared helper for <see cref="ICommandSource"/> implementors.
/// Manages the subscribe/unsubscribe lifecycle for <see cref="ICommand.CanExecuteChanged"/>
/// and <see cref="CommandManager.RequerySuggested"/> so each control does not duplicate it.
/// </summary>
internal sealed class CommandSourceHelper
{
	readonly ICommandSource _owner;
	readonly Action _refreshEnabled;

	ICommand? _currentCommand;

	/// <param name="owner">The owning command source.</param>
	/// <param name="refreshEnabled">
	/// Callback that re-evaluates <c>IsEnabled</c> (or equivalent) on the owner.
	/// </param>
	internal CommandSourceHelper(ICommandSource owner, Action refreshEnabled)
	{
		_owner = owner;
		_refreshEnabled = refreshEnabled;
	}

	/// <summary>
	/// Call when <see cref="ICommandSource.Command"/> is replaced. Unsubscribes from
	/// the old command's <see cref="ICommand.CanExecuteChanged"/> and subscribes to
	/// the new one, then refreshes the enabled state immediately.
	/// </summary>
	internal void OnCommandChanged(ICommand? oldCommand, ICommand? newCommand)
	{
		if (ReferenceEquals(oldCommand, newCommand)) return;

		if (oldCommand != null)
			oldCommand.CanExecuteChanged -= OnCanExecuteChanged;

		_currentCommand = newCommand;

		if (newCommand != null)
			newCommand.CanExecuteChanged += OnCanExecuteChanged;

		_refreshEnabled();
	}

	/// <summary>
	/// Returns whether the current command reports it can execute.
	/// When no command is set, returns <c>true</c> (element stays enabled).
	/// </summary>
	internal bool QueryCanExecute()
	{
		var cmd = _owner.Command;
		if (cmd == null) return true;
		var target = _owner.CommandTarget ?? (_owner as System.Windows.IInputElement);
		if (cmd is RoutedCommand rc)
			return rc.CanExecute(_owner.CommandParameter, target);
		return cmd.CanExecute(_owner.CommandParameter);
	}

	/// <summary>
	/// Invokes the current command if it can execute.
	/// </summary>
	internal void InvokeCommand()
	{
		var cmd = _owner.Command;
		if (cmd == null) return;
		var target = _owner.CommandTarget ?? (_owner as System.Windows.IInputElement);
		if (cmd is RoutedCommand rc)
			rc.Execute(_owner.CommandParameter, target);
		else
			cmd.Execute(_owner.CommandParameter);
	}

	/// <summary>Deterministic teardown — unsubscribes from the current command.</summary>
	internal void Dispose()
	{
		if (_currentCommand != null)
		{
			_currentCommand.CanExecuteChanged -= OnCanExecuteChanged;
			_currentCommand = null;
		}
	}

	void OnCanExecuteChanged(object? sender, EventArgs e) => _refreshEnabled();
}