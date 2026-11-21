using System.Windows.Media;

namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>
	/// Identifies the <see cref="Clip"/> property (default null; AffectsRender). Shapes honour any geometry
	/// through Cairo. Native GTK widgets (Image, controls, panels) cannot clip to an arbitrary path in GTK 4,
	/// so any non-null clip clips to the widget bounds (<c>Overflow.Hidden</c>); the geometry's own
	/// position, size and corner radii are not applied. That is a documented gap.
	/// </summary>
	public static readonly DependencyProperty ClipProperty = DependencyProperty.Register(
		nameof(Clip), typeof(Geometry), typeof(UIElement),
		new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnClipPropertyChanged));

	static void OnClipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not UIElement el) return;
		if (el.HasBackingWidget)
			el.ApplyClipToBounds(el._widget!);
		el.OnClipChanged();
	}

	/// <summary>Gets or sets the geometry that clips this element's content.</summary>
	public Geometry? Clip
	{
		get => (Geometry?)GetValue(ClipProperty);
		set => SetValue(ClipProperty, value);
	}

	/// <summary>True when a clip must be approximated by clipping the native widget to its bounds.</summary>
	internal bool NativeClipActive => Clip != null && !PaintsClipItself;

	/// <summary>True for elements that apply <see cref="Clip"/> exactly while painting (Shape).</summary>
	internal virtual bool PaintsClipItself => false;

	/// <summary>Called after <see cref="Clip"/> changes, so a self-painting element can redraw.</summary>
	internal virtual void OnClipChanged() { }

	/// <summary>Installs <see cref="Clip"/> as the Cairo clip for a self-painting element.</summary>
	internal void ApplyClip(Cairo.Context cr)
	{
		if (Clip is not { } clip) return;
		clip.BuildPath(cr);
		cr.Clip();
	}
}
