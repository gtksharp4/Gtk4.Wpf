using System.Windows.Input;

namespace System.Windows
{
	/// <summary>
	/// Wires GTK 4 input controllers to the WPF routed-event system.
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
			AttachFocusController(widget);
			AttachKeyController(widget);
			AttachClickController(widget);
			AttachMotionController(widget);

			// Honour a Focus() call that arrived before the widget existed.
			if (_focusRequestedBeforeCreation)
			{
				_focusRequestedBeforeCreation = false;
				widget.GrabFocus();
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

		// ── Click (mouse button) controller ─────────────────────────────────────

		void AttachClickController(Gtk.Widget widget)
		{
			var click = new Gtk.GestureClick();
			click.Pressed  += OnGtkPressed;
			click.Released += OnGtkReleased;
			widget.AddController(click);
		}

		void OnGtkPressed(object o, Gtk.PressedArgs args)
		{
			uint button = GetClickButton(o);
			var btn = GdkButtonToMouseButton(button);
			var pos = new System.Windows.Point(args.X, args.Y);
			UpdateMouseButtonState(btn, pressed: true);
			Mouse._lastPosition = pos;

			var lb = Mouse.LeftButton;
			var rb = Mouse.RightButton;
			var preview = new MouseButtonEventArgs(PreviewMouseDownEvent, pos, btn, args.NPress, lb, rb);
			RaiseEvent(preview);
			if (!preview.Handled)
			{
				var e = new MouseButtonEventArgs(MouseDownEvent, pos, btn, args.NPress, lb, rb);
				RaiseEvent(e);
			}
		}

		void OnGtkReleased(object o, Gtk.ReleasedArgs args)
		{
			uint button = GetClickButton(o);
			var btn = GdkButtonToMouseButton(button);
			var pos = new System.Windows.Point(args.X, args.Y);
			UpdateMouseButtonState(btn, pressed: false);
			Mouse._lastPosition = pos;

			var lb = Mouse.LeftButton;
			var rb = Mouse.RightButton;
			var preview = new MouseButtonEventArgs(PreviewMouseUpEvent, pos, btn, 1, lb, rb);
			RaiseEvent(preview);
			if (!preview.Handled)
			{
				var e = new MouseButtonEventArgs(MouseUpEvent, pos, btn, 1, lb, rb);
				RaiseEvent(e);
			}
		}

		/// <summary>Reads the button number from the GestureClick sender.</summary>
		static uint GetClickButton(object sender) =>
			sender is Gtk.GestureClick gc ? (uint)gc.Button : 1u;

		static void UpdateMouseButtonState(MouseButton btn, bool pressed)
		{
			var state = pressed ? MouseButtonState.Pressed : MouseButtonState.Released;
			if (btn == MouseButton.Left)  Mouse.LeftButton  = state;
			if (btn == MouseButton.Right) Mouse.RightButton = state;
		}

		static MouseButton GdkButtonToMouseButton(uint gdkButton) => gdkButton switch
		{
			1u => MouseButton.Left,
			2u => MouseButton.Middle,
			3u => MouseButton.Right,
			8u => MouseButton.XButton1,
			9u => MouseButton.XButton2,
			_  => MouseButton.Left,
		};

		// ── Motion controller ────────────────────────────────────────────────────

		void AttachMotionController(Gtk.Widget widget)
		{
			var motion = new Gtk.EventControllerMotion();
			motion.Motion += OnGtkMotion;
			motion.Enter  += OnGtkMotionEnter;
			motion.Leave  += OnGtkMotionLeave;
			widget.AddController(motion);
		}

		void OnGtkMotion(object o, Gtk.MotionArgs args)
		{
			var pos = new System.Windows.Point(args.X, args.Y);
			Mouse._lastPosition = pos;
			var e = new MouseEventArgs(MouseMoveEvent, pos, Mouse.LeftButton, Mouse.RightButton);
			RaiseEvent(e);
		}

		void OnGtkMotionEnter(object o, Gtk.EnterArgs args)
		{
			var pos = new System.Windows.Point(args.X, args.Y);
			Mouse._lastPosition = pos;
			var e = new MouseEventArgs(MouseEnterEvent, pos, Mouse.LeftButton, Mouse.RightButton);
			RaiseEvent(e);
		}

		void OnGtkMotionLeave(object? sender, EventArgs args)
		{
			var e = new MouseEventArgs(MouseLeaveEvent, Mouse._lastPosition,
				Mouse.LeftButton, Mouse.RightButton);
			RaiseEvent(e);
		}
	}
}
