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

	/// <summary>Modifier keys held at the time of the event.</summary>
	public ModifierKeys Modifiers { get; }

	readonly System.Windows.Point _position;

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
		Modifiers   = modifiers;
	}

	/// <summary>
	/// Returns the pointer position relative to <paramref name="relativeTo"/>.
	/// When <paramref name="relativeTo"/> is <c>null</c> returns the raw position.
	/// </summary>
	public System.Windows.Point GetPosition(System.Windows.UIElement? relativeTo) => _position;
}

/// <summary>Event args for <c>MouseDown</c>/<c>MouseUp</c> events.</summary>
public class MouseButtonEventArgs : MouseEventArgs
{
	/// <summary>Which button changed.</summary>
	public MouseButton ChangedButton { get; }

	/// <summary>Number of rapid clicks (1 = single, 2 = double, …).</summary>
	public int ClickCount { get; }

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