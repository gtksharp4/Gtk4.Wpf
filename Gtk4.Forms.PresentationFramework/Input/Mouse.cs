namespace System.Windows.Input;

/// <summary>
/// Provides mouse state services. State is updated by GTK gesture controllers.
/// </summary>
public static class Mouse
{
	/// <summary>Current state of the left mouse button.</summary>
	public static MouseButtonState LeftButton { get; internal set; } = MouseButtonState.Released;

	/// <summary>Current state of the right mouse button.</summary>
	public static MouseButtonState RightButton { get; internal set; } = MouseButtonState.Released;

	/// <summary>
	/// Returns the mouse position relative to <paramref name="relativeTo"/>.
	/// Returns <see cref="System.Windows.Point"/> origin when tracking is unavailable.
	/// </summary>
	public static System.Windows.Point GetPosition(System.Windows.UIElement? relativeTo)
	{
		// GTK does not give a global cursor position without a GDK device query on
		// the GTK thread.  Return the last tracked position from the motion controller.
		return _lastPosition;
	}

	internal static System.Windows.Point _lastPosition;

	/// <summary>
	/// Application-wide cursor override. When non-<c>null</c> this cursor takes
	/// precedence over any element-level <c>FrameworkElement.Cursor</c>.
	/// <para>
	/// <b>GTK 4 limitation:</b> GTK does not expose a global cursor override API.
	/// Setting this property stores the value so application code and consumers can
	/// read it; actual per-widget enforcement requires the caller to propagate the
	/// value to the relevant widget(s) or walk the visual tree. No automatic
	/// propagation is performed by this implementation.
	/// </para>
	/// </summary>
	public static System.Windows.Input.Cursor? OverrideCursor { get; set; }
}