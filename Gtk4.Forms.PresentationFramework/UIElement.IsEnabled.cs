using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// <see cref="IsEnabled"/>: the local value ANDed with <see cref="IsEnabledCore"/> and the parent's
/// effective value (coercion, so the base value survives and is restored on re-enable). Applied as
/// <c>Gtk.Widget.Sensitive</c>, which GTK also propagates to descendants and which stops controller
/// dispatch, so mouse and keyboard stop on a disabled subtree and the theme draws <c>:disabled</c>.
/// </summary>
public abstract partial class UIElement
{
	/// <summary>Identifies the <see cref="IsEnabled"/> dependency property.</summary>
	public static readonly DependencyProperty IsEnabledProperty =
		DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(UIElement),
			new PropertyMetadata(true, OnIsEnabledChanged, CoerceIsEnabled));

	/// <summary>Gets or sets whether this element is enabled in the user interface.</summary>
	public bool IsEnabled
	{
		get => (bool)(GetValue(IsEnabledProperty) ?? true);
		set => SetValue(IsEnabledProperty, value);
	}

	/// <summary>Occurs when <see cref="IsEnabled"/> changes.</summary>
	public event DependencyPropertyChangedEventHandler? IsEnabledChanged;

	/// <summary>
	/// Element-specific enabled state ANDed into <see cref="IsEnabled"/>. Command sources override it
	/// and call <c>CoerceValue(IsEnabledProperty)</c> when it may have changed.
	/// </summary>
	protected virtual bool IsEnabledCore => true;

	static object? CoerceIsEnabled(DependencyObject d, object? baseValue)
	{
		var el = (UIElement)d;
		if (!(bool)(baseValue ?? true)) return false;
		return el.IsEnabledCore && (el.LogicalParent == null || el.LogicalParent.IsEnabled);
	}

	static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var el = (UIElement)d;
		bool enabled = (bool)(e.NewValue ?? true);
		if (el.HasBackingWidget)
			el._widget!.Sensitive = enabled;
		el.IsEnabledChanged?.Invoke(el, e);
		if (!enabled)
		{
			if (el.IsFocused) el.SetFocusedInternal(false);
			if (el.IsMouseCaptured) Mouse.Capture(null);
		}
		// Parent first, then children (WPF InvalidateForceInheritPropertyOnChildren).
		foreach (var child in el.ChildSnapshot())
			child.CoerceValue(IsEnabledProperty);
	}
}
