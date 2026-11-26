namespace System.Windows.Input;

/// <summary>
/// Abstract base for all input gestures (key, mouse, etc.).
/// </summary>
public abstract class InputGesture
{
	/// <summary>
	/// Returns whether this gesture matches the given input event arguments.
	/// Runtime-side check; GTK key-event wiring is a later phase — callers may
	/// invoke <see cref="UIElement.InvokeInputBinding"/> programmatically for now.
	/// </summary>
	public abstract bool Matches(object? targetElement, System.EventArgs inputEventArgs);
}