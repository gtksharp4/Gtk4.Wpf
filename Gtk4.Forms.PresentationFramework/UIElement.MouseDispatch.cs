using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// Managed mouse dispatch shared by the GTK controllers (<c>UIElement.GtkMouseInput.cs</c>) and
/// the unit tests: button-state tracking, capture re-targeting and the generic
/// (Preview)MouseDown/Up/MouseMove raise. No GTK required.
/// </summary>
public abstract partial class UIElement
{
	/// <summary>
	/// Raises PreviewMouseDown/MouseDown (or Up) for <paramref name="button"/> as if it was
	/// pressed/released over this element at <paramref name="position"/>. Input is re-targeted to
	/// <see cref="Mouse.Captured"/> while the mouse is captured. The button-specific events and
	/// <c>MouseDoubleClick</c> are produced by class handlers from these.
	/// </summary>
	internal void RaiseMouseButton(MouseButton button, Point position, int clickCount, bool pressed)
	{
		Mouse.SetButtonState(button, pressed ? MouseButtonState.Pressed : MouseButtonState.Released);
		Mouse.TrackPosition(this, position);

		var target = Mouse.GetInputTarget(this);
		var lb = Mouse.LeftButton;
		var rb = Mouse.RightButton;
		var mods = Keyboard.Modifiers;
		var preview = new MouseButtonEventArgs(pressed ? PreviewMouseDownEvent : PreviewMouseUpEvent,
			position, button, clickCount, lb, rb, mods) { PositionSource = this };
		target.RaiseEvent(preview);
		if (preview.Handled) return;

		var e = new MouseButtonEventArgs(pressed ? MouseDownEvent : MouseUpEvent,
			position, button, clickCount, lb, rb, mods) { PositionSource = this };
		target.RaiseEvent(e);
	}

	/// <summary>
	/// Raises MouseMove as if the pointer moved over this element to <paramref name="position"/>;
	/// re-targeted to <see cref="Mouse.Captured"/> while the mouse is captured.
	/// </summary>
	internal void RaiseMouseMove(Point position)
	{
		Mouse.TrackPosition(this, position);
		var target = Mouse.GetInputTarget(this);
		var e = new MouseEventArgs(MouseMoveEvent, position, Mouse.LeftButton, Mouse.RightButton,
			Keyboard.Modifiers) { PositionSource = this };
		target.RaiseEvent(e);
	}

	// ── Same-GDK-event de-duplication ────────────────────────────────────────────────

	static (int type, uint time, double x, double y) _lastDispatchedEvent;

	/// <summary>
	/// Returns <c>true</c> when the GDK event currently being handled by
	/// <paramref name="controller"/> was already dispatched by another element's controller
	/// (GTK propagates one event through every ancestor's controllers; the WPF route has already
	/// been run by the innermost one). The key is (type, time, surface position).
	/// </summary>
	static bool IsDuplicateDispatch(object controller)
	{
		if (controller is not Gtk.EventController c) return false;
		var ev = c.CurrentEvent;
		if (ev == null) return false;
		ev.GetPosition(out double x, out double y);
		var key = ((int)ev.EventType, ev.Time, x, y);
		if (key.Equals(_lastDispatchedEvent)) return true;
		_lastDispatchedEvent = key;
		return false;
	}
}
