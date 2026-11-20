namespace System.Windows;

/// <summary>
/// <see cref="FlowDirection"/> and <see cref="Language"/>. GTK applies text direction per widget and does not
/// inherit it from the parent widget, so every element that inherits a new value pushes it to its own widget.
/// GTK gap: native widgets mirror themselves, but panels laid out by this layer are not mirrored.
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
		if (d is FrameworkElement fe && fe.IsWidgetMaterialized)
			fe.ApplyFlowDirection(fe.Widget);
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

	partial void ApplyFlowDirectionOnCreated(Gtk.Widget widget) => ApplyFlowDirection(widget);

	void ApplyFlowDirection(Gtk.Widget widget) =>
		widget.Direction = FlowDirection == FlowDirection.RightToLeft ? Gtk.TextDirection.Rtl : Gtk.TextDirection.Ltr;
}
