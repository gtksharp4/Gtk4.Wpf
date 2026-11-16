using System.Windows.Input;

namespace System.Windows
{
	/// <summary>
	/// GTK click (GestureClick) and motion (EventControllerMotion) controllers.
	/// Focus and key controllers live in <c>UIElement.GtkInput.cs</c>.
	/// </summary>
	public abstract partial class UIElement
	{
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
