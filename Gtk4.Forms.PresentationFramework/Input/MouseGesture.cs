using System;

namespace System.Windows.Input;

/// <summary>Identifies a mouse action.</summary>
public enum MouseAction
{
	None           = 0,
	LeftClick      = 1,
	RightClick     = 2,
	MiddleClick    = 3,
	WheelClick     = 4,
	LeftDoubleClick  = 5,
	RightDoubleClick = 6,
	MiddleDoubleClick = 7,
}

/// <summary>
/// A mouse-based input gesture: a <see cref="MouseAction"/> with optional modifiers.
/// GTK integration is a later phase; dispatch today is via
/// <see cref="UIElement.InvokeInputBinding"/>.
/// </summary>
public class MouseGesture : InputGesture
{
	/// <summary>The mouse action.</summary>
	public MouseAction MouseAction { get; }

	/// <summary>Required modifier keys.</summary>
	public ModifierKeys Modifiers { get; }

	/// <param name="mouseAction">The action.</param>
	public MouseGesture(MouseAction mouseAction)
		: this(mouseAction, ModifierKeys.None) { }

	/// <param name="mouseAction">The action.</param>
	/// <param name="modifiers">Required modifiers.</param>
	public MouseGesture(MouseAction mouseAction, ModifierKeys modifiers)
	{
		MouseAction = mouseAction;
		Modifiers = modifiers;
	}

	/// <inheritdoc/>
	public override bool Matches(object? targetElement, EventArgs inputEventArgs)
	{
		if (inputEventArgs is MouseGestureEventArgs mea)
			return mea.MouseAction == MouseAction && mea.Modifiers == Modifiers;
		return false;
	}
}

/// <summary>
/// Programmatic event-args for <see cref="MouseGesture.Matches"/> without a live GTK event.
/// </summary>
public sealed class MouseGestureEventArgs : EventArgs
{
	/// <summary>The mouse action.</summary>
	public MouseAction MouseAction { get; }

	/// <summary>Modifiers held.</summary>
	public ModifierKeys Modifiers { get; }

	/// <param name="mouseAction">Action.</param>
	/// <param name="modifiers">Modifiers.</param>
	public MouseGestureEventArgs(MouseAction mouseAction, ModifierKeys modifiers = ModifierKeys.None)
	{
		MouseAction = mouseAction;
		Modifiers = modifiers;
	}
}