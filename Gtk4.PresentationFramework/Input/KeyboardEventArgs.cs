namespace System.Windows.Input;

/// <summary>Handler for <see cref="KeyboardFocusChangedEventArgs"/> events.</summary>
public delegate void KeyboardFocusChangedEventHandler(object sender, KeyboardFocusChangedEventArgs e);

/// <summary>Base class for input-device event data.</summary>
public class InputEventArgs : RoutedEventArgs
{
	/// <param name="inputDevice">The device that raised the event.</param>
	/// <param name="timestamp">The time of the event, in milliseconds.</param>
	public InputEventArgs(InputDevice inputDevice, int timestamp)
	{
		Device = inputDevice;
		Timestamp = timestamp;
	}

	internal InputEventArgs(RoutedEvent routedEvent, InputDevice inputDevice, int timestamp)
		: base(routedEvent)
	{
		Device = inputDevice ?? throw new ArgumentNullException(nameof(inputDevice));
		Timestamp = timestamp;
	}

	/// <summary>The device that raised the event.</summary>
	public InputDevice Device { get; }

	/// <summary>
	/// The time of the event, in milliseconds: the GDK event time on the GTK path, the tick count for programmatic raises.
	/// </summary>
	public int Timestamp { get; }
}

/// <summary>Base class for keyboard event data.</summary>
public class KeyboardEventArgs : InputEventArgs
{
	/// <param name="keyboard">The keyboard.</param>
	/// <param name="timestamp">The time of the event, in milliseconds.</param>
	public KeyboardEventArgs(KeyboardDevice keyboard, int timestamp)
		: base(keyboard, timestamp)
	{
	}

	internal KeyboardEventArgs(RoutedEvent routedEvent, KeyboardDevice keyboard, int timestamp)
		: base(routedEvent, keyboard ?? throw new ArgumentNullException(nameof(keyboard)), timestamp)
	{
	}

	/// <summary>The keyboard that raised the event.</summary>
	public KeyboardDevice KeyboardDevice => (KeyboardDevice)Device;
}

/// <summary>Data for the (Preview)Got/LostKeyboardFocus events.</summary>
public class KeyboardFocusChangedEventArgs : KeyboardEventArgs
{
	/// <param name="keyboard">The keyboard.</param>
	/// <param name="timestamp">The time of the event, in milliseconds.</param>
	/// <param name="oldFocus">The element that had focus.</param>
	/// <param name="newFocus">The element that gets focus.</param>
	public KeyboardFocusChangedEventArgs(
		KeyboardDevice keyboard, int timestamp, IInputElement? oldFocus, IInputElement? newFocus)
		: base(keyboard, timestamp)
	{
		OldFocus = oldFocus;
		NewFocus = newFocus;
	}

	/// <summary>The element that had keyboard focus.</summary>
	public IInputElement? OldFocus { get; }

	/// <summary>The element that gets keyboard focus.</summary>
	public IInputElement? NewFocus { get; }
}
