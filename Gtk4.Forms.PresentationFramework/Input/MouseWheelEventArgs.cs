namespace System.Windows.Input;

/// <summary>Handler delegate for <c>MouseWheel</c> / <c>PreviewMouseWheel</c>.</summary>
public delegate void MouseWheelEventHandler(object sender, MouseWheelEventArgs e);

/// <summary>Event args for the <c>MouseWheel</c> / <c>PreviewMouseWheel</c> events.</summary>
public class MouseWheelEventArgs : MouseEventArgs
{
	/// <summary>
	/// Amount the wheel turned: positive away from the user, negative towards the user;
	/// one notch is <see cref="Mouse.MouseWheelDeltaForOneLine"/> (120).
	/// </summary>
	public int Delta { get; }

	/// <param name="mouse">The mouse device.</param>
	/// <param name="timestamp">The time of the event, in milliseconds.</param>
	/// <param name="delta">Wheel delta.</param>
	public MouseWheelEventArgs(MouseDevice mouse, int timestamp, int delta)
		: base(mouse, timestamp)
	{
		Delta = delta;
	}

	/// <param name="routedEvent">The routed event being raised.</param>
	/// <param name="position">Pointer position relative to the source element.</param>
	/// <param name="delta">Wheel delta (120 per notch, positive away from the user).</param>
	/// <param name="leftButton">Left button state.</param>
	/// <param name="rightButton">Right button state.</param>
	/// <param name="modifiers">Active modifiers.</param>
	public MouseWheelEventArgs(
		RoutedEvent routedEvent,
		Point position,
		int delta,
		MouseButtonState leftButton  = MouseButtonState.Released,
		MouseButtonState rightButton = MouseButtonState.Released,
		ModifierKeys modifiers       = ModifierKeys.None)
		: this(routedEvent, position, delta, leftButton, rightButton, modifiers, Environment.TickCount)
	{
	}

	internal MouseWheelEventArgs(
		RoutedEvent routedEvent,
		Point position,
		int delta,
		MouseButtonState leftButton,
		MouseButtonState rightButton,
		ModifierKeys modifiers,
		int timestamp)
		: base(routedEvent, position, leftButton, rightButton, modifiers, timestamp)
	{
		Delta = delta;
	}
}
