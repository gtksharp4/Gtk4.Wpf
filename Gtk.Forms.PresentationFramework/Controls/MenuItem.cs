using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace System.Windows.Controls
{
	/// <summary>
	/// A menu-item control. Backed by a <see cref="Gtk.Button"/> for leaf items and by a
	/// <see cref="Gtk.MenuButton"/> + <see cref="Gtk.Popover"/> for items with sub-items
	/// (<c>Gtk.PopoverMenu</c> integration is a future phase).
	/// Implements <see cref="ICommandSource"/>; activating fires <see cref="Click"/> and
	/// invokes <see cref="Command"/>.
	/// </summary>
	/// <remarks>
	/// GTK-widget choice: leaf items use <c>Gtk.Button</c>; parent items use
	/// <c>Gtk.MenuButton</c> that opens a <c>Gtk.Popover</c> containing child buttons.
	/// A full <c>Gtk.PopoverMenu</c> menu-model integration is deferred.
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
			var text = e.NewValue?.ToString() ?? string.Empty;
			if (mi._gtkButton != null) mi._gtkButton.Label = text;
			if (mi._gtkMenuButton != null) mi._gtkMenuButton.Label = text;
		}

		/// <summary>Gets or sets the item header (typically a string).</summary>
		public object? Header
		{
			get => GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}

		// ── IsCheckable / IsChecked DPs ───────────────────────────────────────────

		/// <summary><c>true</c> when this item acts as a toggle.</summary>
		public static readonly DependencyProperty IsCheckableProperty =
			DependencyProperty.Register(nameof(IsCheckable), typeof(bool), typeof(MenuItem),
				new PropertyMetadata(false));

		/// <summary>Gets or sets whether this item can be checked.</summary>
		public bool IsCheckable
		{
			get => (bool?)GetValue(IsCheckableProperty) == true;
			set => SetValue(IsCheckableProperty, value);
		}

		/// <summary>Checked state; relevant when <see cref="IsCheckable"/> is <c>true</c>.</summary>
		public static readonly DependencyProperty IsCheckedProperty =
			DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(MenuItem),
				new PropertyMetadata(false, OnIsCheckedChanged));

		static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var mi = (MenuItem)d;
			if (mi._updating) return;
			// Reflect on the GTK check button when present (future: Gtk.CheckButton in popover).
			mi.Checked?.Invoke(mi, EventArgs.Empty);
		}

		/// <summary>Gets or sets the checked state.</summary>
		public bool IsChecked
		{
			get => (bool?)GetValue(IsCheckedProperty) == true;
			set => SetValue(IsCheckedProperty, value);
		}

		/// <summary>Raised when <see cref="IsChecked"/> becomes <c>true</c>.</summary>
		public event EventHandler? Checked;

		// ── InputGestureText DP ───────────────────────────────────────────────────

		/// <summary>Display text for the keyboard shortcut (e.g. "Ctrl+S").</summary>
		public static readonly DependencyProperty InputGestureTextProperty =
			DependencyProperty.Register(nameof(InputGestureText), typeof(string), typeof(MenuItem),
				new PropertyMetadata(null));

		/// <summary>Gets or sets the input gesture text shown at the right of the item.</summary>
		public string? InputGestureText
		{
			get => (string?)GetValue(InputGestureTextProperty);
			set => SetValue(InputGestureTextProperty, value);
		}

		// ── Items collection (sub-items) ──────────────────────────────────────────

		/// <summary>Child menu items. When non-empty the item shows a sub-menu popover.</summary>
		public IList<MenuItem> Items { get; } = new List<MenuItem>();

		// ── Command DPs ────────────────────────────────────────────────────────────

		/// <summary>The command invoked when this item is activated.</summary>
		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(MenuItem),
				new PropertyMetadata(null, OnCommandChanged));

		/// <summary>Parameter forwarded to <see cref="ICommand.Execute"/>.</summary>
		public static readonly DependencyProperty CommandParameterProperty =
			DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(MenuItem),
				new PropertyMetadata(null));

		/// <summary>Optional explicit routing target.</summary>
		public static readonly DependencyProperty CommandTargetProperty =
			DependencyProperty.Register(nameof(CommandTarget), typeof(UIElement), typeof(MenuItem),
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

		Gtk.Button? _gtkButton;
		Gtk.MenuButton? _gtkMenuButton;
		bool _updating;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			var label = Header?.ToString() ?? string.Empty;
			if (Items.Count > 0)
			{
				_gtkMenuButton = new Gtk.MenuButton { Label = label };
				var popover = new Gtk.Popover();
				var box = new Gtk.Box(Gtk.Orientation.Vertical, 2);
				foreach (var child in Items)
					box.Add(child.Widget);
				popover.Child = box;
				_gtkMenuButton.Popover = popover;
				return _gtkMenuButton;
			}

			_gtkButton = new Gtk.Button { Label = label };
			_gtkButton.Clicked += OnGtkClicked;
			return _gtkButton;
		}

		void OnGtkClicked(object? sender, EventArgs e) => Activate();

		/// <summary>Activates the item: fires <see cref="Click"/> and toggles check if applicable.</summary>
		internal void InvokeClick_ForTest() => Activate();

		void Activate()
		{
			if (IsCheckable)
			{
				_updating = true;
				try { SetValue(IsCheckedProperty, !IsChecked); }
				finally { _updating = false; }
				Checked?.Invoke(this, EventArgs.Empty);
			}
			Click?.Invoke(this, EventArgs.Empty);
			if (CommandHelper.QueryCanExecute())
				CommandHelper.InvokeCommand();
		}

		// ── Command helper ────────────────────────────────────────────────────────

		CommandSourceHelper? _commandHelper;
		CommandSourceHelper CommandHelper => _commandHelper ??= new CommandSourceHelper(this, RefreshEnabled);

		void RefreshEnabled()
		{
			var canRun = CommandHelper.QueryCanExecute();
			if (_gtkButton != null) _gtkButton.Sensitive = canRun;
			if (_gtkMenuButton != null) _gtkMenuButton.Sensitive = canRun;
		}

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
}
