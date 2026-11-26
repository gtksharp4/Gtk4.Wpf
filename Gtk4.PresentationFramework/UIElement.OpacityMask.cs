using System.Windows.Media;

namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>
	/// Identifies the <see cref="OpacityMask"/> dependency property (default null).
	/// <para>
	/// The mask is applied in the GTK snapshot of the native containers (StackPanel, DockPanel, Grid, Canvas,
	/// Border): a mask on any child of those panels (panels, controls and shapes alike) is pushed around the
	/// child's whole render, CSS background and border included. A masked element under any other parent (a
	/// window's content) masks its content only, not its CSS background or border.
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
