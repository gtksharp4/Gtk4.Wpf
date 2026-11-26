using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// <see cref="IsMouseOver"/> / <see cref="IsMouseDirectlyOver"/> read-only DPs.
/// </summary>
/// <remarks>
/// GTK 4 mapping: <c>GtkEventControllerMotion:contains-pointer</c> is exactly WPF's
/// ancestor-inclusive <see cref="IsMouseOver"/> (pointer over the widget or any descendant), and
/// <c>:is-pointer</c> is <see cref="IsMouseDirectlyOver"/> (pointer over the widget itself, not a
/// child). Both are read in the motion controller's enter/leave/notify handlers
/// (<c>UIElement.GtkMouseInput.cs</c>), which call <see cref="UpdateMouseOver"/>.
/// </remarks>
public abstract partial class UIElement
{
	static readonly DependencyPropertyKey _isMouseOverKey =
		DependencyProperty.RegisterReadOnly(nameof(IsMouseOver), typeof(bool), typeof(UIElement),
			new PropertyMetadata(false));

	/// <summary>Identifies the read-only <see cref="IsMouseOver"/> dependency property.</summary>
	public static readonly DependencyProperty IsMouseOverProperty = _isMouseOverKey.DependencyProperty;

	static readonly DependencyPropertyKey _isMouseDirectlyOverKey =
		DependencyProperty.RegisterReadOnly(nameof(IsMouseDirectlyOver), typeof(bool), typeof(UIElement),
			new PropertyMetadata(false));

	/// <summary>Identifies the read-only <see cref="IsMouseDirectlyOver"/> dependency property.</summary>
	public static readonly DependencyProperty IsMouseDirectlyOverProperty =
		_isMouseDirectlyOverKey.DependencyProperty;

	/// <summary>Gets whether the pointer is over this element or any of its descendants.</summary>
	public bool IsMouseOver => (bool)(GetValue(IsMouseOverProperty) ?? false);

	/// <summary>Gets whether the pointer is directly over this element (not over a child).</summary>
	public bool IsMouseDirectlyOver => (bool)(GetValue(IsMouseDirectlyOverProperty) ?? false);

	/// <summary>Occurs when <see cref="IsMouseDirectlyOver"/> changes.</summary>
	public event DependencyPropertyChangedEventHandler? IsMouseDirectlyOverChanged;

	/// <summary>
	/// Applies the pointer-containment state reported by GTK: <paramref name="containsPointer"/>
	/// → <see cref="IsMouseOver"/>, <paramref name="isPointer"/> → <see cref="IsMouseDirectlyOver"/>
	/// (and <see cref="Mouse.DirectlyOver"/>).
	/// </summary>
	internal void UpdateMouseOver(bool containsPointer, bool isPointer)
	{
		if (IsMouseOver != containsPointer)
			SetValue(_isMouseOverKey, containsPointer);

		bool wasDirect = IsMouseDirectlyOver;
		if (wasDirect == isPointer) return;
		SetValue(_isMouseDirectlyOverKey, isPointer);
		if (isPointer)
			Mouse.DirectlyOver = this;
		else if (ReferenceEquals(Mouse.DirectlyOver, this))
			Mouse.DirectlyOver = null;
		IsMouseDirectlyOverChanged?.Invoke(this,
			new DependencyPropertyChangedEventArgs(IsMouseDirectlyOverProperty, wasDirect, isPointer));
	}

	// ── Programmatic seam (mirrors what GTK reports for a pointer crossing) ──────────

	/// <summary>
	/// Simulates the pointer entering this element: this element becomes directly-over and every
	/// ancestor becomes contains-pointer (what GTK's motion controllers report), the previous
	/// directly-over chain is cleared, and <c>MouseEnter</c> is raised on this element.
	/// </summary>
	internal void SimulateMouseEnterForTest()
	{
		if (Mouse.DirectlyOver is UIElement previous && !ReferenceEquals(previous, this))
			previous.SimulateMouseLeaveForTest();
		UpdateMouseOver(true, true);
		for (var p = LogicalParent; p != null; p = p.LogicalParent)
			p.UpdateMouseOver(true, false);
		RaiseEvent(new MouseEventArgs(MouseEnterEvent, Mouse._lastPosition,
			Mouse.LeftButton, Mouse.RightButton) { PositionSource = this });
	}

	/// <summary>
	/// Simulates the pointer leaving this element (and therefore its ancestors' bounds): clears
	/// both flags up the chain and raises <c>MouseLeave</c> on this element.
	/// </summary>
	internal void SimulateMouseLeaveForTest()
	{
		UpdateMouseOver(false, false);
		for (var p = LogicalParent; p != null; p = p.LogicalParent)
			p.UpdateMouseOver(false, false);
		RaiseEvent(new MouseEventArgs(MouseLeaveEvent, Mouse._lastPosition,
			Mouse.LeftButton, Mouse.RightButton) { PositionSource = this });
	}
}
