namespace System.Windows.Input;

/// <summary>
/// Managed mouse capture.
/// </summary>
/// <remarks>
/// GTK 4 already gives an implicit pointer grab to the widget whose <c>GtkGestureClick</c>
/// claimed a press, so press → drag outside → release is delivered natively. Capture is
/// tracked here and the motion/button handlers re-target <c>MouseMove</c>/<c>MouseDown</c>/
/// <c>MouseUp</c> to <see cref="Captured"/>. <b>GTK 4 gap:</b> with no button held there is no
/// explicit grab API for widgets inside a toplevel (<c>gdk_seat_grab</c> is not exposed for
/// them), so pointer motion outside the toplevel is not delivered while captured.
/// <para>
/// <b>Gap vs WPF:</b> capture is never released automatically. WPF's <c>MouseDevice</c> drops
/// capture (raising <c>LostMouseCapture</c>) when the captor becomes disabled, invisible or
/// non-hit-testable, or when the OS takes the capture away (<c>WM_CAPTURECHANGED</c>, e.g.
/// alt-tab). Here <see cref="Captured"/> keeps pointing at the element in all those cases until
/// <see cref="Capture(IInputElement)"/> is called with <c>null</c> (or
/// <see cref="UIElement.ReleaseMouseCapture"/>), and input keeps being re-targeted to it.
/// </para>
/// </remarks>
public static partial class Mouse
{
	static CaptureMode _captureMode;

	/// <summary>The element that has captured the mouse, or <c>null</c>.</summary>
	public static IInputElement? Captured { get; internal set; }

	/// <summary>Captures the mouse to <paramref name="element"/> (<see cref="CaptureMode.Element"/>).</summary>
	/// <returns><c>true</c> if the capture succeeded (or <paramref name="element"/> is <c>null</c>).</returns>
	public static bool Capture(IInputElement? element) =>
		Capture(element, element == null ? CaptureMode.None : CaptureMode.Element);

	/// <summary>
	/// Captures the mouse to <paramref name="element"/> using <paramref name="captureMode"/>.
	/// A <c>null</c> element or <see cref="CaptureMode.None"/> releases capture.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the capture changed as requested; <c>false</c> when the element cannot
	/// take capture (not enabled or not visible).
	/// </returns>
	public static bool Capture(IInputElement? element, CaptureMode captureMode)
	{
		if (captureMode != CaptureMode.None && captureMode != CaptureMode.Element
			&& captureMode != CaptureMode.SubTree)
			throw new ComponentModel.InvalidEnumArgumentException(
				nameof(captureMode), (int)captureMode, typeof(CaptureMode));

		if (element == null || captureMode == CaptureMode.None)
		{
			element = null;
			captureMode = CaptureMode.None;
		}
		else if (element is not UIElement uie || !uie.CanCaptureMouse)
		{
			return false;
		}

		var old = Captured as UIElement;
		var next = element as UIElement;
		_captureMode = captureMode;
		if (ReferenceEquals(old, next)) return true;

		Captured = next;
		old?.SetMouseCaptured(false);
		next?.SetMouseCaptured(true);
		UIElement.UpdateMouseCaptureWithin(old, next);
		old?.RaiseMouseCaptureEvent(UIElement.LostMouseCaptureEvent);
		next?.RaiseMouseCaptureEvent(UIElement.GotMouseCaptureEvent);
		return true;
	}

	/// <summary>
	/// Returns the element that should receive mouse input hit on <paramref name="hit"/>:
	/// the captured element (or, for <see cref="CaptureMode.SubTree"/>, <paramref name="hit"/>
	/// when it lies inside the captured subtree); <paramref name="hit"/> when nothing is captured.
	/// </summary>
	internal static UIElement GetInputTarget(UIElement hit)
	{
		if (Captured is not UIElement captured) return hit;
		if (_captureMode == CaptureMode.SubTree)
		{
			for (UIElement? e = hit; e != null; e = e.LogicalParent)
				if (ReferenceEquals(e, captured)) return hit;
		}
		return captured;
	}
}
