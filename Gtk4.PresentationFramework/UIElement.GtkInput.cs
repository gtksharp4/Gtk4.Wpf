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
		AttachScrollController(widget);
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

	internal Gtk.EventControllerKey? _keyController;

	void AttachKeyController(Gtk.Widget widget)
	{
		var key = _keyController = new Gtk.EventControllerKey();
		key.KeyPressed  += OnGtkKeyPressed;
		key.KeyReleased += OnGtkKeyReleased;
		widget.AddController(key);
	}

	void OnGtkKeyPressed(object o, Gtk.KeyPressedArgs args) =>
		args.RetVal = DispatchGtkKeyPressed(args.Keyval, args.State, GdkEventTime(o));

	/// <summary>The GTK key controller's press dispatch; returns whether the key was handled.</summary>
	internal bool DispatchGtkKeyPressed(uint keyval, Gdk.ModifierType state, int timestamp)
	{
		var key  = GdkKeyMap.FromKeyval(keyval);
		var mods = GdkKeyMap.FromGdkModifier(state);
		Keyboard.SetKeyDown(key);
		Keyboard.Modifiers = mods;

		// Generate text if the keyval is a printable character.
		string text = KeyvalToString(keyval);
		if (!string.IsNullOrEmpty(text))
		{
			var previewTI = new TextCompositionEventArgs(PreviewTextInputEvent, text, timestamp);
			RaiseEvent(previewTI);
			if (!previewTI.Handled)
			{
				var ti = new TextCompositionEventArgs(TextInputEvent, text, timestamp);
				RaiseEvent(ti);
			}
		}

		// Handled keys must not reach the widget class bindings (a Gtk.Scale would step a second time).
		return RaiseKeyDown(key, mods, timestamp);
	}

	void OnGtkKeyReleased(object o, Gtk.KeyReleasedArgs args) =>
		DispatchGtkKeyReleased(args.Keyval, args.State, GdkEventTime(o));

	internal void DispatchGtkKeyReleased(uint keyval, Gdk.ModifierType state, int timestamp)
	{
		var key  = GdkKeyMap.FromKeyval(keyval);
		var mods = GdkKeyMap.FromGdkModifier(state);
		Keyboard.SetKeyUp(key);
		Keyboard.Modifiers = mods;

		var preview = new KeyEventArgs(PreviewKeyUpEvent, key, KeyStates.None, mods, Key.None, timestamp);
		RaiseEvent(preview);
		if (!preview.Handled)
		{
			var e = new KeyEventArgs(KeyUpEvent, key, KeyStates.None, mods, Key.None, timestamp);
			RaiseEvent(e);
		}
	}

	/// <summary>Returns the printable string for a GDK keyval, or empty for non-printable keys.</summary>
	static string KeyvalToString(uint keyval) => GdkKeyText.FromKeyval(keyval) ?? string.Empty;
}
