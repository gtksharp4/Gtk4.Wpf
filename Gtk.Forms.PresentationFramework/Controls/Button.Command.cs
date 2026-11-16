using System;
using System.Windows.Input;

namespace System.Windows.Controls
{
	public partial class Button
	{
		// ── Command DP ────────────────────────────────────────────────────────────

		/// <summary>The command invoked when the button is clicked.</summary>
		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.Register(
				nameof(Command),
				typeof(ICommand),
				typeof(Button),
				new PropertyMetadata(null, OnCommandChanged));

		/// <summary>Parameter forwarded to <see cref="ICommand.Execute"/>.</summary>
		public static readonly DependencyProperty CommandParameterProperty =
			DependencyProperty.Register(
				nameof(CommandParameter),
				typeof(object),
				typeof(Button),
				new PropertyMetadata(null, OnCommandParameterChanged));

		/// <summary>
		/// Optional explicit routing target for <see cref="RoutedCommand"/>.
		/// When <c>null</c> the button itself is the target.
		/// </summary>
		public static readonly DependencyProperty CommandTargetProperty =
			DependencyProperty.Register(
				nameof(CommandTarget),
				typeof(UIElement),
				typeof(Button),
				new PropertyMetadata(null));

		static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var btn = (Button)d;
			btn.CommandHelper.OnCommandChanged(e.OldValue as ICommand, e.NewValue as ICommand);
		}

		static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// Refresh enabled state when parameter changes (command may have parameter-sensitive CanExecute).
			((Button)d).RefreshCommandEnabled();
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
		public UIElement? CommandTarget
		{
			get => (UIElement?)GetValue(CommandTargetProperty);
			set => SetValue(CommandTargetProperty, value);
		}

		// ── Command helper (lazy — created once, lives for the button's lifetime) ──

		CommandSourceHelper? _commandHelper;

		CommandSourceHelper CommandHelper =>
			_commandHelper ??= new CommandSourceHelper(this, RefreshCommandEnabled);

		void RefreshCommandEnabled()
		{
			var canRun = CommandHelper.QueryCanExecute();
			if (_button != null)
				_button.Sensitive = canRun;
		}

		// ── Wired from Button.OnGtkClicked (partial method hook) ─────────────────

		/// <summary>
		/// Invokes the command after the Click routed-event fires.
		/// Called by <see cref="OnGtkClicked"/> and exposed internally for unit tests.
		/// </summary>
		internal void ExecuteCommandIfAble_ForTest() => ExecuteCommandIfAble();

		void ExecuteCommandIfAble()
		{
			if (CommandHelper.QueryCanExecute())
				CommandHelper.InvokeCommand();
		}

		/// <summary>
		/// Deterministic teardown: unsubscribes from <see cref="ICommand.CanExecuteChanged"/>.
		/// </summary>
		void DisposeCommandSource()
		{
			_commandHelper?.Dispose();
			_commandHelper = null;
		}

		bool _disposed;

		/// <summary>
		/// Disposes the button: unsubscribes from the command's CanExecuteChanged (E3).
		/// </summary>
		public void Dispose()
		{
			if (_disposed) return;
			_disposed = true;
			DisposeCommandSource();
		}
	}
}
