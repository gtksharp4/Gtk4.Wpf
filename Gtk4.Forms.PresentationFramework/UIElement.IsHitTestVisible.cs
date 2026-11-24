namespace System.Windows;

/// <summary>
/// <see cref="IsHitTestVisible"/>, coerced by the parent like <see cref="IsEnabled"/>. Applied as
/// <c>Gtk.Widget.CanTarget</c>, which also makes descendants untargetable (WPF's inheritance).
/// </summary>
public abstract partial class UIElement
{
	/// <summary>Identifies the <see cref="IsHitTestVisible"/> dependency property.</summary>
	public static readonly DependencyProperty IsHitTestVisibleProperty =
		DependencyProperty.Register(nameof(IsHitTestVisible), typeof(bool), typeof(UIElement),
			new PropertyMetadata(true, OnIsHitTestVisibleChanged, CoerceIsHitTestVisible));

	/// <summary>Gets or sets whether this element can be the target of hit testing.</summary>
	public bool IsHitTestVisible
	{
		get => (bool)(GetValue(IsHitTestVisibleProperty) ?? true);
		set => SetValue(IsHitTestVisibleProperty, value);
	}

	/// <summary>Occurs when <see cref="IsHitTestVisible"/> changes.</summary>
	public event DependencyPropertyChangedEventHandler? IsHitTestVisibleChanged;

	static object? CoerceIsHitTestVisible(DependencyObject d, object? baseValue)
	{
		var el = (UIElement)d;
		if (!(bool)(baseValue ?? true)) return false;
		return el.LogicalParent == null || el.LogicalParent.IsHitTestVisible;
	}

	static void OnIsHitTestVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var el = (UIElement)d;
		if (el.HasBackingWidget)
			el.ApplyCanTarget(el._widget!);
		el.IsHitTestVisibleChanged?.Invoke(el, e);
		foreach (var child in el.ChildSnapshot())
			child.CoerceValue(IsHitTestVisibleProperty);
	}

	void ApplyCanTarget(Gtk.Widget widget) =>
		widget.CanTarget = !SuppressCanTarget && VisibilityGtkMap.CanTarget(WidgetVisibility, IsHitTestVisible);
}
