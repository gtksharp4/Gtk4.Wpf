using System.Windows.Input;

namespace System.Windows
{
	/// <summary>
	/// Focus DPs, keyboard/mouse/text routed-event declarations, and the programmatic
	/// raise seam used by unit tests (no GTK required).
	/// GTK controller wiring lives in <c>UIElement.GtkInput.cs</c>.
	/// </summary>
	public abstract partial class UIElement : IInputElement
	{
		// ── Focusable DP ─────────────────────────────────────────────────────────

		/// <summary>Focusable DP (default <c>true</c>).</summary>
		public static readonly DependencyProperty FocusableProperty =
			DependencyProperty.Register(nameof(Focusable), typeof(bool), typeof(UIElement),
				new PropertyMetadata(true));

		/// <summary>Gets or sets whether this element can receive keyboard focus.</summary>
		public bool Focusable
		{
			get => (bool)(GetValue(FocusableProperty) ?? true);
			set => SetValue(FocusableProperty, value);
		}

		// ── IsFocused (read-only) DP ─────────────────────────────────────────────

		static readonly DependencyPropertyKey _isFocusedKey =
			DependencyProperty.RegisterReadOnly(nameof(IsFocused), typeof(bool), typeof(UIElement),
				new PropertyMetadata(false));

		/// <summary>Read-only DP — true while keyboard focus is on this element.</summary>
		public static readonly DependencyProperty IsFocusedProperty = _isFocusedKey.DependencyProperty;

		/// <summary>Returns <c>true</c> when this element holds keyboard focus.</summary>
		public bool IsFocused => (bool)(GetValue(IsFocusedProperty) ?? false);

		/// <summary>
		/// Returns <c>true</c> when this element holds keyboard focus (alias for <see cref="IsFocused"/>).
		/// </summary>
		public bool IsKeyboardFocused => IsFocused;

		/// <summary>
		/// Requests keyboard focus. Calls <see cref="Gtk.Widget.GrabFocus"/> when the
		/// backing widget has been created; otherwise records the request to apply on first creation.
		/// </summary>
		public bool Focus()
		{
			if (!Focusable) return false;
			SetFocusedInternal(true);
			if (IsWidgetCreated)
				Widget.GrabFocus();
			else
				_focusRequestedBeforeCreation = true;
			return true;
		}

		bool _focusRequestedBeforeCreation;

		// ── Focus routed events ──────────────────────────────────────────────────

		/// <summary>Raised when keyboard focus enters this element.</summary>
		public static readonly RoutedEvent GotFocusEvent =
			EventManager.RegisterRoutedEvent(
				"GotFocus", RoutingStrategy.Bubble,
				typeof(RoutedEventHandler), typeof(UIElement));

		/// <summary>Raised when keyboard focus leaves this element.</summary>
		public static readonly RoutedEvent LostFocusEvent =
			EventManager.RegisterRoutedEvent(
				"LostFocus", RoutingStrategy.Bubble,
				typeof(RoutedEventHandler), typeof(UIElement));

		/// <summary>Occurs when keyboard focus enters this element.</summary>
		public event RoutedEventHandler? GotFocus
		{
			add    => AddHandler(GotFocusEvent, value!);
			remove => RemoveHandler(GotFocusEvent, value!);
		}

		/// <summary>Occurs when keyboard focus leaves this element.</summary>
		public event RoutedEventHandler? LostFocus
		{
			add    => AddHandler(LostFocusEvent, value!);
			remove => RemoveHandler(LostFocusEvent, value!);
		}

		// ── Keyboard routed events ───────────────────────────────────────────────

		/// <summary>Tunnel PreviewKeyDown.</summary>
		public static readonly RoutedEvent PreviewKeyDownEvent =
			EventManager.RegisterRoutedEvent("PreviewKeyDown", RoutingStrategy.Tunnel,
				typeof(System.Windows.Input.KeyEventHandler), typeof(UIElement));

		/// <summary>Bubble KeyDown.</summary>
		public static readonly RoutedEvent KeyDownEvent =
			EventManager.RegisterRoutedEvent("KeyDown", RoutingStrategy.Bubble,
				typeof(System.Windows.Input.KeyEventHandler), typeof(UIElement));

		/// <summary>Tunnel PreviewKeyUp.</summary>
		public static readonly RoutedEvent PreviewKeyUpEvent =
			EventManager.RegisterRoutedEvent("PreviewKeyUp", RoutingStrategy.Tunnel,
				typeof(System.Windows.Input.KeyEventHandler), typeof(UIElement));

		/// <summary>Bubble KeyUp.</summary>
		public static readonly RoutedEvent KeyUpEvent =
			EventManager.RegisterRoutedEvent("KeyUp", RoutingStrategy.Bubble,
				typeof(System.Windows.Input.KeyEventHandler), typeof(UIElement));

		/// <summary>Occurs when a key is pressed.</summary>
		public event System.Windows.Input.KeyEventHandler? KeyDown
		{
			add    => AddHandler(KeyDownEvent, value!);
			remove => RemoveHandler(KeyDownEvent, value!);
		}

		/// <summary>Occurs when a key is released.</summary>
		public event System.Windows.Input.KeyEventHandler? KeyUp
		{
			add    => AddHandler(KeyUpEvent, value!);
			remove => RemoveHandler(KeyUpEvent, value!);
		}

		// ── Mouse routed events ──────────────────────────────────────────────────

		/// <summary>Tunnel PreviewMouseDown.</summary>
		public static readonly RoutedEvent PreviewMouseDownEvent =
			EventManager.RegisterRoutedEvent("PreviewMouseDown", RoutingStrategy.Tunnel,
				typeof(System.Windows.Input.MouseButtonEventHandler), typeof(UIElement));

		/// <summary>Bubble MouseDown.</summary>
		public static readonly RoutedEvent MouseDownEvent =
			EventManager.RegisterRoutedEvent("MouseDown", RoutingStrategy.Bubble,
				typeof(System.Windows.Input.MouseButtonEventHandler), typeof(UIElement));

		/// <summary>Tunnel PreviewMouseUp.</summary>
		public static readonly RoutedEvent PreviewMouseUpEvent =
			EventManager.RegisterRoutedEvent("PreviewMouseUp", RoutingStrategy.Tunnel,
				typeof(System.Windows.Input.MouseButtonEventHandler), typeof(UIElement));

		/// <summary>Bubble MouseUp.</summary>
		public static readonly RoutedEvent MouseUpEvent =
			EventManager.RegisterRoutedEvent("MouseUp", RoutingStrategy.Bubble,
				typeof(System.Windows.Input.MouseButtonEventHandler), typeof(UIElement));

		/// <summary>Bubble MouseMove.</summary>
		public static readonly RoutedEvent MouseMoveEvent =
			EventManager.RegisterRoutedEvent("MouseMove", RoutingStrategy.Bubble,
				typeof(System.Windows.Input.MouseEventHandler), typeof(UIElement));

		/// <summary>Direct MouseEnter.</summary>
		public static readonly RoutedEvent MouseEnterEvent =
			EventManager.RegisterRoutedEvent("MouseEnter", RoutingStrategy.Direct,
				typeof(System.Windows.Input.MouseEventHandler), typeof(UIElement));

		/// <summary>Direct MouseLeave.</summary>
		public static readonly RoutedEvent MouseLeaveEvent =
			EventManager.RegisterRoutedEvent("MouseLeave", RoutingStrategy.Direct,
				typeof(System.Windows.Input.MouseEventHandler), typeof(UIElement));

		/// <summary>Occurs when a mouse button is pressed over this element.</summary>
		public event System.Windows.Input.MouseButtonEventHandler? MouseDown
		{
			add    => AddHandler(MouseDownEvent, value!);
			remove => RemoveHandler(MouseDownEvent, value!);
		}

		/// <summary>Occurs when a mouse button is released over this element.</summary>
		public event System.Windows.Input.MouseButtonEventHandler? MouseUp
		{
			add    => AddHandler(MouseUpEvent, value!);
			remove => RemoveHandler(MouseUpEvent, value!);
		}

		/// <summary>Occurs when the pointer moves over this element.</summary>
		public event System.Windows.Input.MouseEventHandler? MouseMove
		{
			add    => AddHandler(MouseMoveEvent, value!);
			remove => RemoveHandler(MouseMoveEvent, value!);
		}

		/// <summary>Occurs when the pointer enters this element.</summary>
		public event System.Windows.Input.MouseEventHandler? MouseEnter
		{
			add    => AddHandler(MouseEnterEvent, value!);
			remove => RemoveHandler(MouseEnterEvent, value!);
		}

		/// <summary>Occurs when the pointer leaves this element.</summary>
		public event System.Windows.Input.MouseEventHandler? MouseLeave
		{
			add    => AddHandler(MouseLeaveEvent, value!);
			remove => RemoveHandler(MouseLeaveEvent, value!);
		}

		// ── Text-input routed events ─────────────────────────────────────────────

		/// <summary>Tunnel PreviewTextInput.</summary>
		public static readonly RoutedEvent PreviewTextInputEvent =
			EventManager.RegisterRoutedEvent("PreviewTextInput", RoutingStrategy.Tunnel,
				typeof(System.Windows.Input.TextCompositionEventHandler), typeof(UIElement));

		/// <summary>Bubble TextInput.</summary>
		public static readonly RoutedEvent TextInputEvent =
			EventManager.RegisterRoutedEvent("TextInput", RoutingStrategy.Bubble,
				typeof(System.Windows.Input.TextCompositionEventHandler), typeof(UIElement));

		/// <summary>Occurs when text is generated by a key stroke or IME.</summary>
		public event System.Windows.Input.TextCompositionEventHandler? TextInput
		{
			add    => AddHandler(TextInputEvent, value!);
			remove => RemoveHandler(TextInputEvent, value!);
		}

		// ── Programmatic raise seam (unit-testable without GTK) ─────────────────

		/// <summary>
		/// Raises <c>PreviewKeyDown</c> (tunnel) then <c>KeyDown</c> (bubble) for
		/// <paramref name="key"/> + <paramref name="modifiers"/> as if the key was pressed on
		/// this element.  Also dispatches matching <see cref="InputBindings"/>.
		/// No GTK required — usable from pure unit tests.
		/// </summary>
		internal void RaiseKeyDownForTest(Key key, ModifierKeys modifiers = ModifierKeys.None)
		{
			Keyboard.Modifiers = modifiers;
			Keyboard.SetKeyDown(key);
			RaiseKeyDown(key, modifiers);
		}

		/// <summary>
		/// Raises <c>PreviewKeyUp</c> (tunnel) then <c>KeyUp</c> (bubble).
		/// No GTK required.
		/// </summary>
		internal void RaiseKeyUpForTest(Key key, ModifierKeys modifiers = ModifierKeys.None)
		{
			Keyboard.SetKeyUp(key);
			var preview = new KeyEventArgs(PreviewKeyUpEvent, key, KeyStates.None, modifiers);
			RaiseEvent(preview);
			if (!preview.Handled)
			{
				var e = new KeyEventArgs(KeyUpEvent, key, KeyStates.None, modifiers);
				RaiseEvent(e);
			}
		}

		// ── Internal helper used by GTK wiring too ───────────────────────────────

		internal void RaiseKeyDown(Key key, ModifierKeys modifiers)
		{
			var preview = new KeyEventArgs(PreviewKeyDownEvent, key, KeyStates.Down, modifiers);
			RaiseEvent(preview);
			if (preview.Handled) return;

			var e = new KeyEventArgs(KeyDownEvent, key, KeyStates.Down, modifiers);
			RaiseEvent(e);
			if (e.Handled) return;

			// Dispatch any matching InputBindings.
			InvokeInputBinding(new KeyGestureEventArgs(key, modifiers));
		}

		// ── Managed focus helpers (called by GTK focus controller) ───────────────

		internal void SetFocusedInternal(bool focused)
		{
			bool wasFocused = IsFocused;
			SetValue(_isFocusedKey, focused);

			if (focused && !wasFocused)
			{
				Keyboard.SetFocused(this);
				FocusManager.SetFocusedElement(this, this);
				RaiseEvent(new RoutedEventArgs(GotFocusEvent));
			}
			else if (!focused && wasFocused)
			{
				Keyboard.ClearFocused(this);
				RaiseEvent(new RoutedEventArgs(LostFocusEvent));
			}
		}
	}
}
