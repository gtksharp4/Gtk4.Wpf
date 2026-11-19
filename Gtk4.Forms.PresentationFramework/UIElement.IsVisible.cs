using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// Read-only <see cref="IsVisible"/>: Visibility is Visible, the parent is visible and the root is
/// a shown <see cref="Window"/> (the analogue of WPF's presentation source). Recomputed top-down so
/// a parent raises <see cref="IsVisibleChanged"/> before its children.
/// </summary>
public abstract partial class UIElement
{
	static readonly DependencyPropertyKey IsVisiblePropertyKey =
		DependencyProperty.RegisterReadOnly(nameof(IsVisible), typeof(bool), typeof(UIElement),
			new PropertyMetadata(false, OnIsVisibleChanged));

	/// <summary>Identifies the read-only <see cref="IsVisible"/> dependency property.</summary>
	public static readonly DependencyProperty IsVisibleProperty = IsVisiblePropertyKey.DependencyProperty;

	/// <summary>Gets whether this element is visible on screen (false when detached or not shown).</summary>
	public bool IsVisible => (bool)(GetValue(IsVisibleProperty) ?? false);

	/// <summary>Occurs when <see cref="IsVisible"/> changes.</summary>
	public event DependencyPropertyChangedEventHandler? IsVisibleChanged;

	/// <summary>The value <see cref="IsVisible"/> should have; <see cref="Window"/> overrides the root rule.</summary>
	internal virtual bool ComputeIsVisible() =>
		Visibility == Visibility.Visible && LogicalParent != null && LogicalParent.IsVisible;

	internal void RecomputeIsVisible()
	{
		var visible = ComputeIsVisible();
		if (visible != IsVisible)
			SetValue(IsVisiblePropertyKey, visible);
	}

	static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var el = (UIElement)d;
		el.IsVisibleChanged?.Invoke(el, e);
		if (!(bool)(e.NewValue ?? false) && el.IsMouseCaptured)
			Mouse.Capture(null);
		foreach (var child in el.ChildSnapshot())
			child.RecomputeIsVisible();
	}

	void OnLogicalParentChangedVisibilityState()
	{
		CoerceValue(IsEnabledProperty);
		CoerceValue(IsHitTestVisibleProperty);
		RecomputeIsVisible();
	}
}
