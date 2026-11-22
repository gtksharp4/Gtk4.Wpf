namespace System.Windows.Input;

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

	/// <param name="mouse">The mouse device.</param>
	/// <param name="timestamp">The time of the event, in milliseconds.</param>
	/// <param name="button">The button that changed.</param>
	/// <exception cref="System.ComponentModel.InvalidEnumArgumentException"><paramref name="button"/> is not a defined button.</exception>
	public MouseButtonEventArgs(MouseDevice mouse, int timestamp, MouseButton button)
		: base(mouse, timestamp)
	{
		if (button < MouseButton.Left || button > MouseButton.XButton2)
			throw new System.ComponentModel.InvalidEnumArgumentException(nameof(button), (int)button, typeof(MouseButton));
		ChangedButton = button;
		ClickCount = 1;
	}

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
		: this(routedEvent, position, changedButton, clickCount, leftButton, rightButton, modifiers,
			Environment.TickCount)
	{
	}

	internal MouseButtonEventArgs(
		RoutedEvent routedEvent,
		System.Windows.Point position,
		MouseButton changedButton,
		int clickCount,
		MouseButtonState leftButton,
		MouseButtonState rightButton,
		ModifierKeys modifiers,
		int timestamp)
		: base(routedEvent, position, leftButton, rightButton, modifiers, timestamp)
	{
		ChangedButton = changedButton;
		ClickCount    = clickCount;
	}
}
