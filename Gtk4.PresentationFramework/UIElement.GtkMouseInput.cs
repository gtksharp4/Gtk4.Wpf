using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// GTK click (GestureClick) and motion (EventControllerMotion) controllers.
/// Focus and key controllers live in <c>UIElement.GtkInput.cs</c>; the managed raise path is in
/// <c>UIElement.MouseDispatch.cs</c>.
/// </summary>
public abstract partial class UIElement
{
	// ── Click (mouse button) controller ─────────────────────────────────────

	void AttachClickController(Gtk.Widget widget)
	{
		var click = new Gtk.GestureClick();
		// GestureClick defaults to the primary button only; 0 listens to every button so right,
		// middle and X buttons reach MouseDown/MouseUp (WPF distinguishes them via ChangedButton).
		click.Button = 0;
		click.Pressed  += OnGtkPressed;
		click.Released += OnGtkReleased;
		widget.AddController(click);
	}

	// GTK propagates one unclaimed press/release through every ancestor's GestureClick: the
	// innermost element dispatches it, every later controller skips it (DispatchGtkButton), with or
	// without capture — so capturing inside MouseDown never re-delivers that MouseDown.
	void OnGtkPressed(object o, Gtk.PressedArgs args) =>
		DispatchGtkButton(GetEventKey(o), GetClickButton(o), new Point(args.X, args.Y), args.NPress, pressed: true);

	// WPF's MouseUp reports the click count of the press it ends (NPress here).
	void OnGtkReleased(object o, Gtk.ReleasedArgs args) =>
		DispatchGtkButton(GetEventKey(o), GetClickButton(o), new Point(args.X, args.Y), args.NPress, pressed: false);

	/// <summary>Reads the button that triggered the current GestureClick sequence.</summary>
	static uint GetClickButton(object sender) =>
		sender is Gtk.GestureClick gc && gc.CurrentButton != 0 ? gc.CurrentButton : 1u;

	// ── Motion controller ────────────────────────────────────────────────────

	void AttachMotionController(Gtk.Widget widget)
	{
		var motion = new Gtk.EventControllerMotion();
		motion.Motion += OnGtkMotion;
		motion.Enter  += OnGtkMotionEnter;
		motion.Leave  += OnGtkMotionLeave;
		// is-pointer flips without enter/leave when the pointer moves between this widget and a
		// child; contains-pointer likewise. Keep IsMouseOver/IsMouseDirectlyOver in sync.
		motion.AddNotification("is-pointer", OnGtkMotionPointerNotify);
		motion.AddNotification("contains-pointer", OnGtkMotionPointerNotify);
		widget.AddController(motion);
	}

	void OnGtkMotionPointerNotify(object o, GLib.NotifyArgs args) => SyncMouseOver(o);

	/// <summary>
	/// Maps <c>contains-pointer</c> → <see cref="IsMouseOver"/> and <c>is-pointer</c> →
	/// <see cref="IsMouseDirectlyOver"/>.
	/// </summary>
	void SyncMouseOver(object controller)
	{
		if (controller is Gtk.EventControllerMotion m)
			UpdateMouseOver(m.ContainsPointer(), m.IsPointer);
	}

	void OnGtkMotion(object o, Gtk.MotionArgs args) =>
		DispatchGtkMotion(GetEventKey(o), new Point(args.X, args.Y));

	void OnGtkMotionEnter(object o, Gtk.EnterArgs args)
	{
		var pos = new Point(args.X, args.Y);
		Mouse.TrackPosition(this, pos);
		SyncMouseOver(o);
		var e = new MouseEventArgs(MouseEnterEvent, pos, Mouse.LeftButton, Mouse.RightButton)
		{
			PositionSource = this,
		};
		RaiseEvent(e);
	}

	void OnGtkMotionLeave(object? sender, EventArgs args)
	{
		// Leave means the pointer left this widget's whole area (moving into a child only
		// flips is-pointer), so both flags clear.
		UpdateMouseOver(false, false);
		var e = new MouseEventArgs(MouseLeaveEvent, Mouse._lastPosition,
			Mouse.LeftButton, Mouse.RightButton) { PositionSource = Mouse._lastPositionSource };
		RaiseEvent(e);
	}
}
