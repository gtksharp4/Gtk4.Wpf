using System.Collections.Generic;
using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// <c>PreviewMouseMove</c> and <see cref="IsMouseCaptureWithin"/> — the mouse members that
/// complete the move/capture surface.
/// </summary>
public abstract partial class UIElement
{
	/// <summary>Tunnel PreviewMouseMove (raised before <see cref="MouseMoveEvent"/>, sharing its Handled flow).</summary>
	public static readonly RoutedEvent PreviewMouseMoveEvent =
		EventManager.RegisterRoutedEvent("PreviewMouseMove", RoutingStrategy.Tunnel,
			typeof(MouseEventHandler), typeof(UIElement));

	/// <summary>Occurs when the pointer moves while over this element (tunnel phase).</summary>
	public event MouseEventHandler? PreviewMouseMove
	{
		add    => AddHandler(PreviewMouseMoveEvent, value!);
		remove => RemoveHandler(PreviewMouseMoveEvent, value!);
	}

	static readonly DependencyPropertyKey _isMouseCaptureWithinKey =
		DependencyProperty.RegisterReadOnly(nameof(IsMouseCaptureWithin), typeof(bool), typeof(UIElement),
			new PropertyMetadata(false));

	/// <summary>Identifies the read-only <see cref="IsMouseCaptureWithin"/> dependency property.</summary>
	public static readonly DependencyProperty IsMouseCaptureWithinProperty =
		_isMouseCaptureWithinKey.DependencyProperty;

	/// <summary>
	/// Gets whether the mouse is captured by this element or by an element in its subtree
	/// (ancestors are followed through the logical parent chain, as input routing is here).
	/// </summary>
	public bool IsMouseCaptureWithin => (bool)(GetValue(IsMouseCaptureWithinProperty) ?? false);

	/// <summary>Occurs when <see cref="IsMouseCaptureWithin"/> changes.</summary>
	public event DependencyPropertyChangedEventHandler? IsMouseCaptureWithinChanged;

	void SetMouseCaptureWithin(bool value)
	{
		bool old = IsMouseCaptureWithin;
		if (old == value) return;
		SetValue(_isMouseCaptureWithinKey, value);
		IsMouseCaptureWithinChanged?.Invoke(this,
			new DependencyPropertyChangedEventArgs(IsMouseCaptureWithinProperty, old, value));
	}

	/// <summary>
	/// Moves <see cref="IsMouseCaptureWithin"/> from <paramref name="oldCaptor"/>'s chain to
	/// <paramref name="newCaptor"/>'s: clears it on elements only in the old chain, then sets it
	/// on the new chain (captor first, then ancestors).
	/// </summary>
	internal static void UpdateMouseCaptureWithin(UIElement? oldCaptor, UIElement? newCaptor)
	{
		var newChain = new HashSet<UIElement>(ReferenceEqualityComparer.Instance);
		for (var e = newCaptor; e != null; e = e.LogicalParent)
			newChain.Add(e);
		for (var e = oldCaptor; e != null; e = e.LogicalParent)
		{
			if (!newChain.Contains(e))
				e.SetMouseCaptureWithin(false);
		}
		for (var e = newCaptor; e != null; e = e.LogicalParent)
			e.SetMouseCaptureWithin(true);
	}
}
