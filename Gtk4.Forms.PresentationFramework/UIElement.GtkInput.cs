using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// Wires GTK 4 input controllers to the WPF routed-event system.
/// Focus + key controllers live here; mouse/motion controllers are in
/// <c>UIElement.GtkMouseInput.cs</c>.
/// Controllers are attached once when the backing widget is first created.
/// </summary>
public abstract partial class UIElement
{
	/// <summary>
	/// Implements the partial hook from <c>UIElement.cs OnWidgetCreated</c>.
	/// Attaches all GTK input controllers and handles a pending focus request.
	/// </summary>
	partial void AttachInputControllersOnCreated(Gtk.Widget widget)
	{
		// Key/focus controllers attach to FocusWidget (may differ from widget for
		// multiline TextBox where focus lives on the inner TextView, not ScrolledWindow).
		var focusTarget = FocusWidget;
		AttachFocusController(focusTarget);
		AttachKeyController(focusTarget);
		AttachClickController(widget);
		AttachMotionController(widget);
		AttachDragDropOnCreated(widget);

		// Honour a Focus() call that arrived before the widget existed.
		if (_focusRequestedBeforeCreation)
		{
			_focusRequestedBeforeCreation = false;
			focusTarget.GrabFocus();
		}
	}

	// ── Focus controller ─────────────────────────────────────────────────────

	void AttachFocusController(Gtk.Widget widget)
	{
		var focus = new Gtk.EventControllerFocus();
		focus.Enter += OnGtkFocusEnter;
		focus.Leave += OnGtkFocusLeave;
		widget.AddController(focus);
	}

	void OnGtkFocusEnter(object? sender, EventArgs e) => SetFocusedInternal(true);
	void OnGtkFocusLeave(object? sender, EventArgs e) => SetFocusedInternal(false);

	// ── Key controller ───────────────────────────────────────────────────────

	void AttachKeyController(Gtk.Widget widget)
	{
		var key = new Gtk.EventControllerKey();
		key.KeyPressed  += OnGtkKeyPressed;
		key.KeyReleased += OnGtkKeyReleased;
		widget.AddController(key);
	}

	void OnGtkKeyPressed(object o, Gtk.KeyPressedArgs args)
	{
		var key  = GdkKeyMap.FromKeyval(args.Keyval);
		var mods = GdkKeyMap.FromGdkModifier(args.State);
		Keyboard.SetKeyDown(key);
		Keyboard.Modifiers = mods;

		// Generate text if the keyval is a printable character.
		string text = KeyvalToString(args.Keyval);
		if (!string.IsNullOrEmpty(text))
		{
			var previewTI = new TextCompositionEventArgs(PreviewTextInputEvent, text);
			RaiseEvent(previewTI);
			if (!previewTI.Handled)
			{
				var ti = new TextCompositionEventArgs(TextInputEvent, text);
				RaiseEvent(ti);
			}
		}

		RaiseKeyDown(key, mods);
	}

	void OnGtkKeyReleased(object o, Gtk.KeyReleasedArgs args)
	{
		var key  = GdkKeyMap.FromKeyval(args.Keyval);
		var mods = GdkKeyMap.FromGdkModifier(args.State);
		Keyboard.SetKeyUp(key);
		Keyboard.Modifiers = mods;

		var preview = new KeyEventArgs(PreviewKeyUpEvent, key, KeyStates.None, mods);
		RaiseEvent(preview);
		if (!preview.Handled)
		{
			var e = new KeyEventArgs(KeyUpEvent, key, KeyStates.None, mods);
			RaiseEvent(e);
		}
	}

	/// <summary>Returns the printable string for a GDK keyval, or empty for non-printable keys.</summary>
	static string KeyvalToString(uint keyval)
	{
		// Skip GDK special keys (>= 0xff00 or < 0x0020)
		if (keyval >= 0xff00 || keyval < 0x0020) return string.Empty;
		try { return char.ConvertFromUtf32((int)keyval); }
		catch { return string.Empty; }
	}
}