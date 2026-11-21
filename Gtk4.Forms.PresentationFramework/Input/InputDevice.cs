namespace System.Windows.Input;

/// <summary>Base class for input devices.</summary>
public abstract class InputDevice
{
	/// <summary>The element that receives this device's input.</summary>
	public abstract IInputElement? Target { get; }
}

/// <summary>The keyboard device; one primary instance exists, see <see cref="Keyboard.PrimaryDevice"/>.</summary>
public abstract class KeyboardDevice : InputDevice
{
	internal KeyboardDevice() { }

	/// <inheritdoc/>
	public override IInputElement? Target => Keyboard.FocusedElement;

	/// <summary>The element that has keyboard focus.</summary>
	public IInputElement? FocusedElement => Keyboard.FocusedElement;

	/// <summary>The modifier keys currently held.</summary>
	public ModifierKeys Modifiers => Keyboard.Modifiers;

	/// <summary>Whether <paramref name="key"/> is down.</summary>
	public bool IsKeyDown(Key key) => Keyboard.IsKeyDown(key);

	/// <summary>Whether <paramref name="key"/> is up.</summary>
	public bool IsKeyUp(Key key) => !Keyboard.IsKeyDown(key);

	/// <summary>Moves keyboard focus to <paramref name="element"/>.</summary>
	public IInputElement? Focus(IInputElement element) => Keyboard.Focus(element);

	/// <summary>Clears keyboard focus.</summary>
	public void ClearFocus() => Keyboard.ClearFocus();
}

/// <summary>The one keyboard device, fed by the GTK key and focus controllers.</summary>
internal sealed class GtkKeyboardDevice : KeyboardDevice
{
}
