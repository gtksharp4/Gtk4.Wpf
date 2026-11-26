namespace System.Windows.Input;

/// <summary>
/// Provides mouse state services. State is updated by GTK gesture controllers.
/// Capture lives in <c>Mouse.Capture.cs</c>.
/// </summary>
public static partial class Mouse
{
	/// <summary>The wheel delta of one notch (one "line"): 120.</summary>
	public const int MouseWheelDeltaForOneLine = 120;

	/// <summary>Current state of the left mouse button.</summary>
	public static MouseButtonState LeftButton { get; internal set; } = MouseButtonState.Released;

	/// <summary>Current state of the right mouse button.</summary>
	public static MouseButtonState RightButton { get; internal set; } = MouseButtonState.Released;

	/// <summary>Current state of the middle mouse button.</summary>
	public static MouseButtonState MiddleButton { get; internal set; } = MouseButtonState.Released;

	/// <summary>Current state of the first extended mouse button (GDK button 8 on X11/Wayland, 4 on Win32/macOS).</summary>
	public static MouseButtonState XButton1 { get; internal set; } = MouseButtonState.Released;

	/// <summary>Current state of the second extended mouse button (GDK button 9 on X11/Wayland, 5 on Win32/macOS).</summary>
	public static MouseButtonState XButton2 { get; internal set; } = MouseButtonState.Released;

	/// <summary>
	/// The element the pointer is directly over: the innermost element whose GTK motion
	/// controller reports <c>is-pointer</c>, or <c>null</c>.
	/// </summary>
	public static IInputElement? DirectlyOver { get; internal set; }

	/// <summary>
	/// Returns the last tracked mouse position relative to <paramref name="relativeTo"/>.
	/// The position is translated from the element that last reported pointer motion via
	/// <c>gtk_widget_compute_point</c>; <c>null</c> returns it relative to that element.
	/// </summary>
	public static Point GetPosition(IInputElement? relativeTo) =>
		MousePositionMap.Translate(_lastPositionSource, relativeTo, _lastPosition);

	internal static Point _lastPosition;

	/// <summary>Element whose coordinate space <see cref="_lastPosition"/> is in.</summary>
	internal static UIElement? _lastPositionSource;

	/// <summary>Records the latest pointer position and the element it is relative to.</summary>
	internal static void TrackPosition(UIElement source, Point position)
	{
		_lastPosition = position;
		_lastPositionSource = source;
	}

	/// <summary>Updates the tracked state of <paramref name="button"/>.</summary>
	internal static void SetButtonState(MouseButton button, MouseButtonState state)
	{
		switch (button)
		{
			case MouseButton.Left:     LeftButton = state; break;
			case MouseButton.Right:    RightButton = state; break;
			case MouseButton.Middle:   MiddleButton = state; break;
			case MouseButton.XButton1: XButton1 = state; break;
			case MouseButton.XButton2: XButton2 = state; break;
		}
	}

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
