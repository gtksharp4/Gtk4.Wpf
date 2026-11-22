using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// Focus DPs, focus helpers, and the programmatic key-raise seam (no GTK required).
/// Routed-event declarations live in <c>UIElement.InputEvents.cs</c>.
/// GTK controller wiring lives in <c>UIElement.GtkInput.cs</c>.
/// </summary>
public abstract partial class UIElement : IInputElement
{
	// ── Focusable DP ─────────────────────────────────────────────────────────

	/// <summary>Focusable DP (default <c>true</c>).</summary>
	public static readonly DependencyProperty FocusableProperty =
		DependencyProperty.Register(nameof(Focusable), typeof(bool), typeof(UIElement),
			new PropertyMetadata(true, OnFocusabilityChanged));

	static void OnFocusabilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		// Re-evaluate widget.CanFocus when Focusable changes (see UIElement.TabNavigation.cs).
		if (d is UIElement el && el.IsWidgetCreated)
			el.ApplyCanFocus(el.FocusWidget);
	}

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
	/// Requests keyboard focus. Calls <see cref="Gtk.Widget.GrabFocus"/> when the
	/// backing widget has been created; otherwise records the request to apply on first creation.
	/// Returns <c>false</c> when not focusable, disabled, or itself or an ancestor is not
	/// <see cref="Visibility.Visible"/> (the Visibility chain; see <see cref="IsVisibilityChainVisible"/>).
	/// </summary>
	public bool Focus()
	{
		if (!Focusable || !IsEnabled || !IsVisibilityChainVisible) return false;
		if (!SetFocusedInternal(true)) return false;
		if (IsWidgetCreated)
			FocusWidget.GrabFocus();
		else
			_focusRequestedBeforeCreation = true;
		return true;
	}

	bool _focusRequestedBeforeCreation;

	/// <summary>
	/// The widget that should receive GTK focus / input controllers.
	/// For most controls this is <see cref="Widget"/>; <see cref="Controls.TextBox"/>
	/// overrides it to return the inner <see cref="Gtk.TextView"/> for multiline mode.
	/// </summary>
	internal virtual Gtk.Widget FocusWidget => Widget;

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

	internal bool RaiseKeyDown(Key key, ModifierKeys modifiers, int? timestamp = null)
	{
		int ts = timestamp ?? Environment.TickCount;
		var preview = new KeyEventArgs(PreviewKeyDownEvent, key, KeyStates.Down, modifiers, Key.None, ts);
		RaiseEvent(preview);
		if (preview.Handled) return true;

		var e = new KeyEventArgs(KeyDownEvent, key, KeyStates.Down, modifiers, Key.None, ts);
		RaiseEvent(e);
		if (e.Handled) return true;

		// Dispatch any matching InputBindings.
		return InvokeInputBinding(new KeyGestureEventArgs(key, modifiers));
	}
}
