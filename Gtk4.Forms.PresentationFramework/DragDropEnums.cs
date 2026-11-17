namespace System.Windows;

/// <summary>
/// Specifies the drag-and-drop operation effects that are allowed by the drop target.
/// Mirrors the WPF <c>DragDropEffects</c> enumeration.
/// </summary>
[Flags]
public enum DragDropEffects
{
	/// <summary>The drag source does not allow the drop.</summary>
	None   = 0,
	/// <summary>Data is copied to the drop target.</summary>
	Copy   = 1,
	/// <summary>Data is moved to the drop target.</summary>
	Move   = 2,
	/// <summary>Data from the source is linked to the drop target.</summary>
	Link   = 4,
	/// <summary>Scrolling is about to start or is currently occurring.</summary>
	Scroll = unchecked((int)0x80000000),
	/// <summary>Data may be copied, moved, linked, or the target may be scrolled.</summary>
	All    = Copy | Move | Link | Scroll,
}

/// <summary>
/// Specifies the keyboard and mouse button modifier keys that are pressed at the
/// start of a drag-and-drop operation.
/// </summary>
[Flags]
public enum DragDropKeyStates
{
	/// <summary>No modifiers.</summary>
	None         = 0,
	/// <summary>Left mouse button is pressed.</summary>
	LeftMouseButton  = 1,
	/// <summary>Right mouse button is pressed.</summary>
	RightMouseButton = 2,
	/// <summary>Shift key is pressed.</summary>
	ShiftKey     = 4,
	/// <summary>Ctrl key is pressed.</summary>
	ControlKey   = 8,
	/// <summary>Middle mouse button is pressed.</summary>
	MiddleMouseButton = 16,
	/// <summary>Alt key is pressed.</summary>
	AltKey       = 32,
}
