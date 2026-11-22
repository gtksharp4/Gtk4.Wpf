using System;

namespace System.Windows.Input;

/// <summary>State of a mouse button.</summary>
public enum MouseButtonState
{
	/// <summary>Button is not pressed.</summary>
	Released = 0,
	/// <summary>Button is pressed.</summary>
	Pressed  = 1,
}

/// <summary>Identifies a mouse button.</summary>
public enum MouseButton
{
	Left   = 0,
	Middle = 1,
	Right  = 2,
	XButton1 = 3,
	XButton2 = 4,
}

/// <summary>Event args for mouse-move and enter/leave events.</summary>
/// <remarks>
/// WPF's <c>MouseEventArgs(MouseDevice, int, StylusDevice)</c> overload is omitted: there is no
/// <c>StylusDevice</c> type in this layer.
/// </remarks>
public class MouseEventArgs : InputEventArgs
{
	// Args raised by the GTK path snapshot the state; args built with the WPF constructor read the device live.
	readonly bool _snapshot;
	readonly MouseButtonState _left, _right, _middle, _xButton1, _xButton2;
	readonly ModifierKeys _modifiers;
	readonly System.Windows.Point _position;

	/// <summary>The mouse device that raised the event.</summary>
	public MouseDevice MouseDevice => (MouseDevice)Device;

	/// <summary>State of the left mouse button.</summary>
	public MouseButtonState LeftButton => _snapshot ? _left : MouseDevice.LeftButton;

	/// <summary>State of the right mouse button.</summary>
	public MouseButtonState RightButton => _snapshot ? _right : MouseDevice.RightButton;

	/// <summary>State of the middle mouse button.</summary>
	public MouseButtonState MiddleButton => _snapshot ? _middle : MouseDevice.MiddleButton;

	/// <summary>State of the first extended button.</summary>
	public MouseButtonState XButton1 => _snapshot ? _xButton1 : MouseDevice.XButton1;

	/// <summary>State of the second extended button.</summary>
	public MouseButtonState XButton2 => _snapshot ? _xButton2 : MouseDevice.XButton2;

	/// <summary>Modifier keys held at the time of the event.</summary>
	public ModifierKeys Modifiers => _snapshot ? _modifiers : Keyboard.Modifiers;

	/// <summary>
	/// The element whose coordinate space <c>position</c> is expressed in (the element whose
	/// GTK controller produced the event). Used by <see cref="GetPosition"/> to translate.
	/// </summary>
	internal UIElement? PositionSource { get; set; }

	/// <param name="mouse">The mouse device.</param>
	/// <param name="timestamp">The time of the event, in milliseconds.</param>
	/// <exception cref="ArgumentNullException"><paramref name="mouse"/> is null.</exception>
	public MouseEventArgs(MouseDevice mouse, int timestamp)
		: base(mouse ?? throw new ArgumentNullException(nameof(mouse)), timestamp)
	{
	}

	/// <param name="routedEvent">The routed event being raised.</param>
	/// <param name="position">Pointer position relative to source element.</param>
	/// <param name="leftButton">Left button state.</param>
	/// <param name="rightButton">Right button state.</param>
	/// <param name="modifiers">Active modifiers.</param>
	public MouseEventArgs(
		RoutedEvent routedEvent,
		System.Windows.Point position,
		MouseButtonState leftButton   = MouseButtonState.Released,
		MouseButtonState rightButton  = MouseButtonState.Released,
		ModifierKeys modifiers        = ModifierKeys.None)
		: this(routedEvent, position, leftButton, rightButton, modifiers, Environment.TickCount)
	{
	}

	internal MouseEventArgs(
		RoutedEvent routedEvent,
		System.Windows.Point position,
		MouseButtonState leftButton,
		MouseButtonState rightButton,
		ModifierKeys modifiers,
		int timestamp)
		: base(routedEvent, Mouse.PrimaryDevice, timestamp)
	{
		_snapshot = true;
		_position = position;
		_left = leftButton;
		_right = rightButton;
		_middle = Mouse.MiddleButton;
		_xButton1 = Mouse.XButton1;
		_xButton2 = Mouse.XButton2;
		_modifiers = modifiers;
	}

	/// <summary>
	/// Returns the pointer position relative to <paramref name="relativeTo"/>, translated from
	/// the producing element's widget with <c>gtk_widget_compute_point</c>.
	/// </summary>
	/// <remarks>
	/// When <paramref name="relativeTo"/> is <c>null</c> the position is returned relative to the
	/// element that produced the event (WPF would return window-relative coordinates). Without
	/// realized widgets (headless) or when GTK cannot relate the two widgets, the raw position is
	/// returned. Args built with the WPF constructor delegate to <see cref="MouseDevice.GetPosition"/>.
	/// </remarks>
	public System.Windows.Point GetPosition(IInputElement? relativeTo) =>
		_snapshot ? MousePositionMap.Translate(PositionSource, relativeTo, _position)
			: MouseDevice.GetPosition(relativeTo);
}
