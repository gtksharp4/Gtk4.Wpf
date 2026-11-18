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
public class MouseEventArgs : RoutedEventArgs
{
	/// <summary>State of the left mouse button.</summary>
	public MouseButtonState LeftButton { get; }

	/// <summary>State of the right mouse button.</summary>
	public MouseButtonState RightButton { get; }

	/// <summary>State of the middle mouse button (snapshot of <see cref="Mouse.MiddleButton"/>).</summary>
	public MouseButtonState MiddleButton { get; }

	/// <summary>State of the first extended button (snapshot of <see cref="Mouse.XButton1"/>).</summary>
	public MouseButtonState XButton1 { get; }

	/// <summary>State of the second extended button (snapshot of <see cref="Mouse.XButton2"/>).</summary>
	public MouseButtonState XButton2 { get; }

	/// <summary>Modifier keys held at the time of the event.</summary>
	public ModifierKeys Modifiers { get; }

	readonly System.Windows.Point _position;

	/// <summary>
	/// The element whose coordinate space <c>position</c> is expressed in (the element whose
	/// GTK controller produced the event). Used by <see cref="GetPosition"/> to translate.
	/// </summary>
	internal UIElement? PositionSource { get; set; }

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
		: base(routedEvent)
	{
		_position  = position;
		LeftButton  = leftButton;
		RightButton = rightButton;
		MiddleButton = Mouse.MiddleButton;
		XButton1     = Mouse.XButton1;
		XButton2     = Mouse.XButton2;
		Modifiers   = modifiers;
	}

	/// <summary>
	/// Returns the pointer position relative to <paramref name="relativeTo"/>, translated from
	/// the producing element's widget with <c>gtk_widget_compute_point</c>.
	/// </summary>
	/// <remarks>
	/// When <paramref name="relativeTo"/> is <c>null</c> the position is returned relative to the
	/// element that produced the event (WPF would return window-relative coordinates). Without
	/// realized widgets (headless) or when GTK cannot relate the two widgets, the raw position is
	/// returned.
	/// </remarks>
	public System.Windows.Point GetPosition(IInputElement? relativeTo) =>
		MousePositionMap.Translate(PositionSource, relativeTo, _position);
}

/// <summary>Event args for <c>MouseDown</c>/<c>MouseUp</c> events.</summary>
public class MouseButtonEventArgs : MouseEventArgs
{
	/// <summary>Which button changed.</summary>
	public MouseButton ChangedButton { get; }

	/// <summary>Number of rapid clicks (1 = single, 2 = double, …).</summary>
	public int ClickCount { get; }

	/// <summary>State of <see cref="ChangedButton"/>: Pressed for a down event, Released for up.</summary>
	public MouseButtonState ButtonState => ChangedButton switch
	{
		MouseButton.Left     => LeftButton,
		MouseButton.Right    => RightButton,
		MouseButton.Middle   => MiddleButton,
		MouseButton.XButton1 => XButton1,
		MouseButton.XButton2 => XButton2,
		_                    => MouseButtonState.Released,
	};

	/// <param name="routedEvent">The routed event being raised.</param>
	/// <param name="position">Pointer position relative to source element.</param>
	/// <param name="changedButton">The button that was pressed or released.</param>
	/// <param name="clickCount">Click count.</param>
	/// <param name="leftButton">Left button state.</param>
	/// <param name="rightButton">Right button state.</param>
	/// <param name="modifiers">Active modifiers.</param>
	public MouseButtonEventArgs(
		RoutedEvent routedEvent,
		System.Windows.Point position,
		MouseButton changedButton,
		int clickCount                = 1,
		MouseButtonState leftButton   = MouseButtonState.Released,
		MouseButtonState rightButton  = MouseButtonState.Released,
		ModifierKeys modifiers        = ModifierKeys.None)
		: base(routedEvent, position, leftButton, rightButton, modifiers)
	{
		ChangedButton = changedButton;
		ClickCount    = clickCount;
	}
}
