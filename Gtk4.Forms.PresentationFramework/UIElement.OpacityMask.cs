using System.Windows.Media;

namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>
	/// Identifies the <see cref="OpacityMask"/> dependency property (default null).
	/// <para>
	/// The mask is applied in the GTK snapshot of the native containers (StackPanel, DockPanel, Grid, Canvas,
	/// Border): the mask brush (solid, gradient or tile) is painted over the element bounds and the element
	/// content (background and children) is drawn through its alpha. Gaps: stock widgets (controls, text) and
	/// Shapes draw without the snapshot override, so the mask is stored but not applied there.
	/// </para>
	/// </summary>
	public static readonly DependencyProperty OpacityMaskProperty = DependencyProperty.Register(
		nameof(OpacityMask), typeof(Brush), typeof(UIElement),
		new UIPropertyMetadata(null, (d, e) => ((UIElement)d).QueueMaskRedraw()));

	/// <summary>Gets or sets the brush whose alpha channel masks this element.</summary>
	public Brush? OpacityMask
	{
		get => (Brush?)GetValue(OpacityMaskProperty);
		set => SetValue(OpacityMaskProperty, value);
	}

	void QueueMaskRedraw()
	{
		if (IsWidgetCreated) Widget.QueueDraw();
	}
}
