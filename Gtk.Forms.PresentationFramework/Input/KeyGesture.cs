using System;

namespace System.Windows.Input;

/// <summary>
/// A keyboard-based input gesture: a <see cref="Key"/> with optional <see cref="ModifierKeys"/>.
/// GTK key-event integration is a later phase; dispatch today is via
/// <see cref="UIElement.InvokeInputBinding"/>.
/// </summary>
public class KeyGesture : InputGesture
{
	/// <summary>The primary key.</summary>
	public Key Key { get; }

	/// <summary>Required modifier keys (default <see cref="ModifierKeys.None"/>).</summary>
	public ModifierKeys Modifiers { get; }

	/// <summary>Optional display name shown in menu accelerator text.</summary>
	public string DisplayString { get; }

	/// <param name="key">The key.</param>
	public KeyGesture(Key key)
		: this(key, ModifierKeys.None, string.Empty) { }

	/// <param name="key">The key.</param>
	/// <param name="modifiers">Required modifiers.</param>
	public KeyGesture(Key key, ModifierKeys modifiers)
		: this(key, modifiers, string.Empty) { }

	/// <param name="key">The key.</param>
	/// <param name="modifiers">Required modifiers.</param>
	/// <param name="displayString">UI display string.</param>
	public KeyGesture(Key key, ModifierKeys modifiers, string displayString)
	{
		Key = key;
		Modifiers = modifiers;
		DisplayString = displayString ?? string.Empty;
	}

	/// <inheritdoc/>
	/// <remarks>
	/// Runtime stub — GTK key event args are not yet bridged.  Pass a
	/// <see cref="KeyGestureEventArgs"/> to exercise this path programmatically.
	/// </remarks>
	public override bool Matches(object? targetElement, EventArgs inputEventArgs)
	{
		if (inputEventArgs is KeyGestureEventArgs kea)
			return kea.Key == Key && kea.Modifiers == Modifiers;
		return false;
	}
}

/// <summary>
/// Programmatic event-args used to test / invoke <see cref="KeyGesture.Matches"/>
/// without a live GTK key event.
/// </summary>
public sealed class KeyGestureEventArgs : EventArgs
{
	/// <summary>The key pressed.</summary>
	public Key Key { get; }

	/// <summary>Modifier keys held at the time of the event.</summary>
	public ModifierKeys Modifiers { get; }

	/// <param name="key">Key pressed.</param>
	/// <param name="modifiers">Modifiers held.</param>
	public KeyGestureEventArgs(Key key, ModifierKeys modifiers = ModifierKeys.None)
	{
		Key = key;
		Modifiers = modifiers;
	}
}