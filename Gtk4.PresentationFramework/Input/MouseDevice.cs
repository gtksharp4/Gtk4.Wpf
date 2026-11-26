namespace System.Windows.Input;

/// <summary>The mouse device; one primary instance exists, see <see cref="Mouse.PrimaryDevice"/>.</summary>
public abstract class MouseDevice : InputDevice
{
	internal MouseDevice() { }

	/// <inheritdoc/>
	public override IInputElement? Target => Mouse.DirectlyOver;

	/// <summary>The element the pointer is directly over.</summary>
	public IInputElement? DirectlyOver => Mouse.DirectlyOver;

	/// <summary>The element that has captured the mouse.</summary>
	public IInputElement? Captured => Mouse.Captured;

	/// <summary>State of the left button.</summary>
	public MouseButtonState LeftButton => Mouse.LeftButton;

	/// <summary>State of the right button.</summary>
	public MouseButtonState RightButton => Mouse.RightButton;

	/// <summary>State of the middle button.</summary>
	public MouseButtonState MiddleButton => Mouse.MiddleButton;

	/// <summary>State of the first extended button.</summary>
	public MouseButtonState XButton1 => Mouse.XButton1;

	/// <summary>State of the second extended button.</summary>
	public MouseButtonState XButton2 => Mouse.XButton2;

	/// <summary>Captures the mouse to <paramref name="element"/>.</summary>
	public bool Capture(IInputElement? element) => Mouse.Capture(element);

	/// <summary>Captures the mouse to <paramref name="element"/> using <paramref name="captureMode"/>.</summary>
	public bool Capture(IInputElement? element, CaptureMode captureMode) => Mouse.Capture(element, captureMode);

	/// <summary>
	/// The pointer position relative to <paramref name="relativeTo"/>; null returns it relative to the element that
	/// last reported pointer motion (WPF: the window root).
	/// </summary>
	public Point GetPosition(IInputElement? relativeTo) => Mouse.GetPosition(relativeTo);
}

/// <summary>The one mouse device, fed by the GTK pointer controllers.</summary>
internal sealed class GtkMouseDevice : MouseDevice
{
}
