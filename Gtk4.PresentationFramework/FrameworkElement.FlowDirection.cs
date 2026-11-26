namespace System.Windows;

/// <summary>
/// <see cref="FlowDirection"/> and <see cref="Language"/>. GTK applies text direction per widget and does not
/// inherit it from the parent widget, so every element that inherits a new value pushes it to its own widget.
/// Gtk.Box-based panels (StackPanel, DockPanel) and Gtk.Grid mirror; panels placed by this layer (Canvas) do not.
/// </summary>
public abstract partial class FrameworkElement
{
	/// <summary>Identifies the FlowDirection property (default LeftToRight; inherits).</summary>
	public static readonly DependencyProperty FlowDirectionProperty = DependencyProperty.Register(
		nameof(FlowDirection), typeof(FlowDirection), typeof(FrameworkElement),
		new FrameworkPropertyMetadata(
			FlowDirection.LeftToRight,
			FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.Inherits,
			OnFlowDirectionChanged),
		IsValidFlowDirection);

	internal static bool IsValidFlowDirection(object? value) =>
		value is FlowDirection.LeftToRight or FlowDirection.RightToLeft;

	static void OnFlowDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FrameworkElement fe && fe.HasBackingWidget)
		{
			fe.ApplyFlowDirection(fe.Widget);
			fe.OnFlowDirectionApplied();
		}
	}

	/// <summary>Gets or sets the content flow direction.</summary>
	public FlowDirection FlowDirection
	{
		get => (FlowDirection)GetValue(FlowDirectionProperty)!;
		set => SetValue(FlowDirectionProperty, value);
	}

	/// <summary>Gets the FlowDirection of <paramref name="element"/>.</summary>
	public static FlowDirection GetFlowDirection(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (FlowDirection)element.GetValue(FlowDirectionProperty)!;
	}

	/// <summary>Sets the FlowDirection of <paramref name="element"/>.</summary>
	public static void SetFlowDirection(DependencyObject element, FlowDirection value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(FlowDirectionProperty, value);
	}

	/// <summary>Called after the backing widget took a new direction; text elements re-render their content.</summary>
	internal virtual void OnFlowDirectionApplied()
	{
	}

	partial void ApplyFlowDirectionOnCreated(Gtk.Widget widget) => ApplyFlowDirection(widget);

	// A widget with direction NONE takes the global default, not its parent's, so the widgets this element owns
	// (an Entry's text child, a TextView, a DropDown's button and popover, a panel's inner boxes) get it too. The
	// walk stops at the backing widget of another element: that element applies its own inherited value.
	void ApplyFlowDirection(Gtk.Widget widget) => SetDirection(
		widget, FlowDirection == FlowDirection.RightToLeft ? Gtk.TextDirection.Rtl : Gtk.TextDirection.Ltr);

	void SetDirection(Gtk.Widget widget, Gtk.TextDirection direction)
	{
		widget.Direction = direction;
		for (var child = widget.FirstChild; child != null; child = child.NextSibling)
		{
			if (OwnerOf(child) is UIElement other && other != this) continue;
			SetDirection(child, direction);
		}
	}
}
