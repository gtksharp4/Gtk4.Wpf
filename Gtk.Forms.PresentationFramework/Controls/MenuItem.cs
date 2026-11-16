using System;
using System.Windows.Input;

namespace System.Windows.Controls
{
	/// <summary>
	/// A menu-item control backed by a <see cref="Gtk.Button"/> inside a horizontal
	/// <see cref="Gtk.Box"/> (real <c>Gtk.PopoverMenu</c> integration is a later phase).
	/// Implements <see cref="ICommandSource"/>: activating the item invokes
	/// <see cref="Command"/> when <see cref="ICommand.CanExecute"/> is true.
	/// </summary>
	/// <remarks>
	/// GTK-widget choice: <c>Gtk.Button</c> with its label set to <see cref="Header"/>.
	/// A <c>Gtk.PopoverMenu</c>-based implementation with keyboard accelerators is deferred;
	/// this gives a functional, testable item that works in a horizontal/vertical layout.
	/// </remarks>
	public class MenuItem : FrameworkElement, ICommandSource, IDisposable
	{
		// ── Header DP ─────────────────────────────────────────────────────────────

		/// <summary>Display text shown as the menu item label.</summary>
		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register(
				nameof(Header),
				typeof(object),
				typeof(MenuItem),
				new PropertyMetadata(null, OnHeaderChanged));

		static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var mi = (MenuItem)d;
			if (mi._gtkButton != null)
				mi._gtkButton.Label = e.NewValue?.ToString() ?? string.Empty;
		}

		/// <summary>Gets or sets the item header (typically a string).</summary>
		public object? Header
		{
			get => GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}

		// ── Command DP ────────────────────────────────────────────────────────────

		/// <summary>The command invoked when this item is activated.</summary>
		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.Register(
				nameof(Command),
				typeof(ICommand),
				typeof(MenuItem),
				new PropertyMetadata(null, OnCommandChanged));

		/// <summary>Parameter forwarded to <see cref="ICommand.Execute"/>.</summary>
		public static readonly DependencyProperty CommandParameterProperty =
			DependencyProperty.Register(
				nameof(CommandParameter),
				typeof(object),
				typeof(MenuItem),
				new PropertyMetadata(null));

		/// <summary>Optional explicit routing target.</summary>
		public static readonly DependencyProperty CommandTargetProperty =
			DependencyProperty.Register(
				nameof(CommandTarget),
				typeof(UIElement),
				typeof(MenuItem),
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
		public UIElement? CommandTarget
		{
			get => (UIElement?)GetValue(CommandTargetProperty);
			set => SetValue(CommandTargetProperty, value);
		}

		// ── Click event ───────────────────────────────────────────────────────────

		/// <summary>Raised when the item is activated (clicked).</summary>
		public event EventHandler? Click;

		// ── GTK backing ───────────────────────────────────────────────────────────

		Gtk.Button _gtkButton = null!;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_gtkButton = new Gtk.Button
			{
				Label = Header?.ToString() ?? string.Empty
			};
			_gtkButton.Clicked += OnGtkClicked;
			return _gtkButton;
		}

		void OnGtkClicked(object? sender, EventArgs e) => Activate();

		/// <summary>
		/// Activates the menu item: fires <see cref="Click"/> and invokes the command if able.
		/// Exposed internally for unit tests.
		/// </summary>
		internal void InvokeClick_ForTest() => Activate();

		void Activate()
		{
			Click?.Invoke(this, EventArgs.Empty);
			if (CommandHelper.QueryCanExecute())
				CommandHelper.InvokeCommand();
		}

		// ── Command helper ────────────────────────────────────────────────────────

		CommandSourceHelper? _commandHelper;

		CommandSourceHelper CommandHelper =>
			_commandHelper ??= new CommandSourceHelper(this, RefreshEnabled);

		void RefreshEnabled()
		{
			var canRun = CommandHelper.QueryCanExecute();
			if (_gtkButton != null)
				_gtkButton.Sensitive = canRun;
		}

		bool _disposed;

		/// <summary>Disposes the menu item: unsubscribes from the command's CanExecuteChanged (E3).</summary>
		public void Dispose()
		{
			if (_disposed) return;
			_disposed = true;
			_commandHelper?.Dispose();
			_commandHelper = null;
		}
	}
}
