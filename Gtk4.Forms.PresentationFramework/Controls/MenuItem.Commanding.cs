using System;
using System.Windows.Input;

namespace System.Windows.Controls;

public partial class MenuItem
{
	// ── Command DPs ────────────────────────────────────────────────────────────

	/// <summary>The command invoked when this item is activated.</summary>
	public static readonly DependencyProperty CommandProperty =
		DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(MenuItem),
			new PropertyMetadata(null, OnCommandChanged));

	/// <summary>Parameter forwarded to <see cref="ICommand.Execute"/>.</summary>
	public static readonly DependencyProperty CommandParameterProperty =
		DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(MenuItem),
			new PropertyMetadata(null, (d, e) => ((MenuItem)d).RefreshEnabled()));

	/// <summary>Optional explicit routing target.</summary>
	public static readonly DependencyProperty CommandTargetProperty =
		DependencyProperty.Register(nameof(CommandTarget), typeof(IInputElement), typeof(MenuItem),
			new PropertyMetadata(null));

	static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var mi = (MenuItem)d;
		mi.CommandHelper.OnCommandChanged(e.OldValue as ICommand, e.NewValue as ICommand);
	}

	/// <inheritdoc cref="ICommandSource.Command"/>
	public ICommand? Command
	{
		get => (ICommand?)GetValue(CommandProperty);
		set => SetValue(CommandProperty, value);
	}

	/// <inheritdoc cref="ICommandSource.CommandParameter"/>
	public object? CommandParameter
	{
		get => GetValue(CommandParameterProperty);
		set => SetValue(CommandParameterProperty, value);
	}

	/// <inheritdoc cref="ICommandSource.CommandTarget"/>
	public IInputElement? CommandTarget
	{
		get => (IInputElement?)GetValue(CommandTargetProperty);
		set => SetValue(CommandTargetProperty, value);
	}

	// ── Command helper ────────────────────────────────────────────────────────

	CommandSourceHelper? _commandHelper;
	CommandSourceHelper CommandHelper => _commandHelper ??= new CommandSourceHelper(this, RefreshEnabled);

	/// <inheritdoc/>
	protected override bool IsEnabledCore =>
		base.IsEnabledCore && (Command == null || CommandHelper.QueryCanExecute());

	void RefreshEnabled() => CoerceValue(IsEnabledProperty);

	bool _disposed;

	/// <summary>Disposes: unsubscribes from command's CanExecuteChanged.</summary>
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		_commandHelper?.Dispose();
		_commandHelper = null;
	}
}