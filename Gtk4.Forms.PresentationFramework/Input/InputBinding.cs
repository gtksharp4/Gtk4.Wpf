using System;

namespace System.Windows.Input;

/// <summary>
/// Associates an <see cref="InputGesture"/> with an <see cref="ICommand"/>.
/// When the gesture fires the command is executed (if <see cref="ICommand.CanExecute"/>).
/// </summary>
public class InputBinding
{
	/// <summary>The gesture that triggers this binding.</summary>
	public InputGesture Gesture { get; set; }

	/// <summary>The command to execute when the gesture matches.</summary>
	public ICommand? Command { get; set; }

	/// <summary>Optional parameter passed to <see cref="ICommand.Execute"/>.</summary>
	public object? CommandParameter { get; set; }

	/// <summary>Optional explicit command target (may be used by routing).</summary>
	public System.Windows.UIElement? CommandTarget { get; set; }

	/// <param name="command">The command.</param>
	/// <param name="gesture">The gesture that triggers it.</param>
	public InputBinding(ICommand command, InputGesture gesture)
	{
		Command = command ?? throw new ArgumentNullException(nameof(command));
		Gesture = gesture ?? throw new ArgumentNullException(nameof(gesture));
	}
}

/// <summary>Associates a <see cref="KeyGesture"/> with an <see cref="ICommand"/>.</summary>
public class KeyBinding : InputBinding
{
	/// <summary>
	/// Returns the gesture cast as <see cref="KeyGesture"/>.
	/// </summary>
	public KeyGesture KeyGesture => (KeyGesture)Gesture;

	/// <param name="command">The command.</param>
	/// <param name="gesture">The key gesture.</param>
	public KeyBinding(ICommand command, KeyGesture gesture)
		: base(command, gesture) { }

	/// <param name="command">The command.</param>
	/// <param name="key">The key.</param>
	/// <param name="modifiers">Modifiers.</param>
	public KeyBinding(ICommand command, Key key, ModifierKeys modifiers)
		: base(command, new KeyGesture(key, modifiers)) { }
}

/// <summary>Associates a <see cref="MouseGesture"/> with an <see cref="ICommand"/>.</summary>
public class MouseBinding : InputBinding
{
	/// <summary>Returns the gesture cast as <see cref="MouseGesture"/>.</summary>
	public MouseGesture MouseGesture => (MouseGesture)Gesture;

	/// <param name="command">The command.</param>
	/// <param name="gesture">The mouse gesture.</param>
	public MouseBinding(ICommand command, MouseGesture gesture)
		: base(command, gesture) { }
}