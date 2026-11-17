using System;

namespace System.Windows.Input;

/// <summary>Keyboard key-state flags.</summary>
[Flags]
public enum KeyStates
{
	/// <summary>Key is not pressed.</summary>
	None    = 0,
	/// <summary>Key is currently pressed.</summary>
	Down    = 1,
	/// <summary>Key is toggled (e.g. CapsLock).</summary>
	Toggled = 2,
}

/// <summary>Event args for <c>KeyDown</c>/<c>KeyUp</c> and their Preview counterparts.</summary>
public class KeyEventArgs : RoutedEventArgs
{
	/// <summary>The key involved.</summary>
	public Key Key { get; }

	/// <summary>Pressed, released, or toggled.</summary>
	public KeyStates KeyStates { get; }

	/// <summary>
	/// For system key events (Alt+key); <see cref="Key.None"/> when not a system key.
	/// </summary>
	public Key SystemKey { get; }

	/// <summary>Modifier keys held at the time of the event.</summary>
	public ModifierKeys Modifiers { get; }

	/// <param name="routedEvent">The routed event being raised.</param>
	/// <param name="key">Key pressed or released.</param>
	/// <param name="keyStates">State of the key.</param>
	/// <param name="modifiers">Active modifiers.</param>
	/// <param name="systemKey">System key, if any.</param>
	public KeyEventArgs(
		RoutedEvent routedEvent,
		Key key,
		KeyStates keyStates,
		ModifierKeys modifiers = ModifierKeys.None,
		Key systemKey = Key.None)
		: base(routedEvent)
	{
		Key = key;
		KeyStates = keyStates;
		Modifiers = modifiers;
		SystemKey = systemKey;
	}

	/// <summary>Returns <c>true</c> when <see cref="Key"/> is currently pressed down.</summary>
	public bool IsDown => (KeyStates & KeyStates.Down) != 0;

	/// <summary>Returns <c>true</c> when <see cref="Key"/> is not pressed.</summary>
	public bool IsUp => !IsDown;
}