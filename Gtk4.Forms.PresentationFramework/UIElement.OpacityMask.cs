using System.Windows.Media;

namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>
	/// Identifies the <see cref="OpacityMask"/> dependency property (default null). Stored only: GTK 4 has no
	/// per-widget alpha mask in the widget tree, so the mask does not change the rendering.
	/// </summary>
	public static readonly DependencyProperty OpacityMaskProperty = DependencyProperty.Register(
		nameof(OpacityMask), typeof(Brush), typeof(UIElement), new UIPropertyMetadata(null));

	/// <summary>Gets or sets the brush whose alpha channel masks this element.</summary>
	public Brush? OpacityMask
	{
		get => (Brush?)GetValue(OpacityMaskProperty);
		set => SetValue(OpacityMaskProperty, value);
	}
}
