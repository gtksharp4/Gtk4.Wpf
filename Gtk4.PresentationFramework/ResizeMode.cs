namespace System.Windows;

/// <summary>Specifies whether a <see cref="Window"/> can be resized by the user.</summary>
public enum ResizeMode
{
	/// <summary>
	/// The window cannot be resized. The minimize and maximize buttons are hidden.
	/// GTK4 limitation: <c>Gtk.Window.Resizable = false</c> suppresses resize handles but
	/// cannot hide individual window-manager buttons; the header-bar ShowTitleButtons flag
	/// removes all three buttons together (min/max/close). NoResize therefore hides all
	/// window-manager buttons via <c>HeaderBar.ShowTitleButtons = false</c>.
	/// </summary>
	NoResize,
	/// <summary>
	/// The window cannot be resized. Only the minimize button is shown.
	/// GTK4 limitation: GTK4 does not expose per-button visibility on the native title bar.
	/// This mode maps to <c>Resizable = false</c> with normal title buttons visible (same
	/// as <see cref="CanResize"/> minus the resize affordance). The maximize button remains
	/// visible but ineffective because GTK4 honours <c>Resizable</c> in its own decoration.
	/// </summary>
	CanMinimize,
	/// <summary>The window can be resized; resize handles and all title buttons are shown.</summary>
	CanResize,
	/// <summary>
	/// The window can be resized. A resize grip is shown in the lower-right corner.
	/// GTK4 limitation: GTK4 handles resize grips natively and no explicit grip widget is
	/// available. This mode maps identically to <see cref="CanResize"/>.
	/// </summary>
	CanResizeWithGrip,
}
