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
	internal void RaiseMouseButton(
		MouseButton button, Point position, int clickCount, bool pressed, int? timestamp = null)
	{
		int ts = timestamp ?? Environment.TickCount;
		Mouse.SetButtonState(button, pressed ? MouseButtonState.Pressed : MouseButtonState.Released);
		Mouse.TrackPosition(this, position);

		var target = Mouse.GetInputTarget(this);
		var lb = Mouse.LeftButton;
		var rb = Mouse.RightButton;
		var mods = Keyboard.Modifiers;
		var preview = new MouseButtonEventArgs(pressed ? PreviewMouseDownEvent : PreviewMouseUpEvent,
			position, button, clickCount, lb, rb, mods, ts) { PositionSource = this };
		target.RaiseEvent(preview);
		if (preview.Handled) return;

		var e = new MouseButtonEventArgs(pressed ? MouseDownEvent : MouseUpEvent,
			position, button, clickCount, lb, rb, mods, ts) { PositionSource = this };
		target.RaiseEvent(e);
	}

	/// <summary>
	/// Raises PreviewMouseMove (tunnel) then MouseMove (bubble) as if the pointer moved over this
	/// element to <paramref name="position"/>; re-targeted to <see cref="Mouse.Captured"/> while
	/// the mouse is captured. As in WPF the pair shares one Handled flow: when the preview is
	/// handled, MouseMove is still raised but already handled, so only handledEventsToo handlers run.
	/// </summary>
	internal void RaiseMouseMove(Point position, int? timestamp = null)
	{
		int ts = timestamp ?? Environment.TickCount;
		Mouse.TrackPosition(this, position);
		var target = Mouse.GetInputTarget(this);
		var mods = Keyboard.Modifiers;
		var preview = new MouseEventArgs(PreviewMouseMoveEvent, position, Mouse.LeftButton, Mouse.RightButton,
			mods, ts) { PositionSource = this };
		target.RaiseEvent(preview);

		var e = new MouseEventArgs(MouseMoveEvent, position, Mouse.LeftButton, Mouse.RightButton,
			mods, ts) { PositionSource = this, Handled = preview.Handled };
		target.RaiseEvent(e);
	}

	// ── GTK controller → managed dispatch ───────────────────────────────────────────

	/// <summary>
	/// The GDK event a GTK controller is currently handling, as a <see cref="MouseEventKey"/>;
	/// <see langword="null"/> when there is none.
	/// </summary>
	static MouseEventKey? GetEventKey(object controller)
	{
		if (controller is not Gtk.EventController c) return null;
		var ev = c.CurrentEvent;
		if (ev == null) return null;
		ev.GetPosition(out double x, out double y);
		return new MouseEventKey((int)ev.EventType, ev.Time, x, y);
	}

	/// <summary>
	/// <see langword="true"/> when the GDK event behind <paramref name="controller"/> was
	/// already dispatched by another (inner) element's controller — see <see cref="MouseDispatchDedupe"/>.
	/// </summary>
	/// <summary>The GDK time of the event a controller is handling; the tick count when there is none.</summary>
	static int GdkEventTime(object controller) =>
		controller is Gtk.EventController c && c.CurrentEvent is { } ev ? unchecked((int)ev.Time) : Environment.TickCount;

	static bool IsDuplicateDispatch(object controller) =>
		GetEventKey(controller) is MouseEventKey key && MouseDispatchDedupe.IsDuplicate(key);

	/// <summary>
	/// The GTK click controller's dispatch decision: skips a GDK event already dispatched by an
	/// inner element (<paramref name="key"/>), drops GDK buttons WPF has no
	/// <see cref="MouseButton"/> for, and otherwise raises the button route. Returns whether it raised.
	/// </summary>
	internal bool DispatchGtkButton(MouseEventKey? key, uint gdkButton, Point position, int clickCount, bool pressed)
	{
		if (key.HasValue && MouseDispatchDedupe.IsDuplicate(key.Value)) return false;
		if (!MouseButtonMap.TryFromGdk(gdkButton, out var button)) return false;
		RaiseMouseButton(button, position, clickCount, pressed, key.HasValue ? unchecked((int)key.Value.Time) : null);
		if (!pressed && button == MouseButton.Right) OnContextMenuRequested(position);
		return true;
	}

	/// <summary>Called after a right-button release; <see cref="FrameworkElement"/> raises ContextMenuOpening.</summary>
	internal virtual void OnContextMenuRequested(Point position) { }

	/// <summary>The GTK motion controller's dispatch decision (single delivery per GDK event).</summary>
	internal bool DispatchGtkMotion(MouseEventKey? key, Point position)
	{
		if (key.HasValue && MouseDispatchDedupe.IsDuplicate(key.Value)) return false;
		RaiseMouseMove(position, key.HasValue ? unchecked((int)key.Value.Time) : null);
		return true;
	}
}
