using System;
using System.Windows.Media;

namespace System.Windows;

/// <summary>
/// <see cref="LayoutTransform"/> dependency property on <see cref="FrameworkElement"/>.
/// <para>
/// Unlike <see cref="UIElement.RenderTransform"/> (render-only), <c>LayoutTransform</c>
/// participates in the layout pass: the parent sees the transformed, axis-aligned bounding
/// box of this element when measuring and arranging.
/// </para>
/// <para>
/// <b>GTK caveat:</b> GTK does its own native widget layout independently of the managed
/// WPF layout pass. The managed <see cref="UIElement.DesiredSize"/>/<see cref="UIElement.RenderSize"/>
/// values are authoritative for the WPF API, but the native GTK widget position and size may
/// differ because GTK does not honour the transformed slot calculated here. Visual rendering of
/// the rotation/scale is applied best-effort via CSS transform (same mechanism as
/// <see cref="UIElement.RenderTransform"/>); pixel-perfect correspondence is not guaranteed.
/// </para>
/// </summary>
public abstract partial class FrameworkElement
{
	// ── LayoutTransform DP ───────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="LayoutTransform"/> dependency property.
	/// Default: <see cref="Transform.Identity"/> (no transform).
	/// </summary>
	public static readonly DependencyProperty LayoutTransformProperty =
		DependencyProperty.Register(
			nameof(LayoutTransform),
			typeof(Transform),
			typeof(FrameworkElement),
			new FrameworkPropertyMetadata(
				Transform.Identity,
				inherits: false,
				affectsMeasure: true,
				affectsArrange: true,
				affectsRender: false,
				propertyChangedCallback: OnLayoutTransformChanged));

	static void OnLayoutTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		// InvalidateMeasure is triggered automatically by affectsMeasure=true.
		// Apply best-effort CSS visual (same mechanism as RenderTransform) when the
		// widget is already realized.
		if (d is UIElement el && el.IsWidgetCreated)
			el.ApplyLayoutTransformCss();
	}

	/// <summary>
	/// Gets or sets the transform applied during the layout pass, affecting this
	/// element's contribution to its parent's measure and arrange.
	/// Setting <c>null</c> resets to the identity transform.
	/// </summary>
	public Transform? LayoutTransform
	{
		get => GetValue(LayoutTransformProperty) as Transform ?? Transform.Identity;
		set => SetValue(LayoutTransformProperty, value ?? Transform.Identity);
	}

	// ── Geometric helper ────────────────────────────────────────────────────

	/// <summary>
	/// Computes the axis-aligned bounding box of <paramref name="size"/> after applying
	/// <paramref name="matrix"/>, centred at the origin.
	/// <para>
	/// This is the core pure function used by <see cref="MeasureCore"/> to expand the
	/// reported <c>DesiredSize</c> to the transformed bounding box; it is intentionally
	/// static and side-effect-free so it can be unit-tested headlessly. Note: arrange is
	/// not yet transform-aware — <c>RenderSize</c> stays the untransformed inner size
	/// (carry-over), so WPF's arrange-time layout-transform placement is not reproduced.
	/// </para>
	/// </summary>
	/// <param name="size">Untransformed element size.</param>
	/// <param name="matrix">Affine transform matrix.</param>
	/// <returns>
	/// Bounding box width and height of the transformed rectangle. Translation
	/// components of <paramref name="matrix"/> do not affect the bounding size.
	/// </returns>
	internal static Size LayoutTransformBounds(Size size, Matrix matrix)
	{
		if (matrix.IsIdentity)
			return size;

		double w = size.Width;
		double h = size.Height;

		// Transform all four corners of the rectangle (origin at top-left = 0,0).
		// Use only the linear (rotation/scale/skew) part; translation shifts the
		// position, not the bounding size, so we zero offsets.
		double m11 = matrix.M11, m12 = matrix.M12;
		double m21 = matrix.M21, m22 = matrix.M22;

		// Corners: (0,0), (w,0), (w,h), (0,h)
		double x0 = 0,     y0 = 0;
		double x1 = w * m11,          y1 = w * m12;
		double x2 = w * m11 + h * m21, y2 = w * m12 + h * m22;
		double x3 = h * m21,           y3 = h * m22;

		double minX = Math.Min(Math.Min(x0, x1), Math.Min(x2, x3));
		double maxX = Math.Max(Math.Max(x0, x1), Math.Max(x2, x3));
		double minY = Math.Min(Math.Min(y0, y1), Math.Min(y2, y3));
		double maxY = Math.Max(Math.Max(y0, y1), Math.Max(y2, y3));

		return new Size(
			Math.Max(0, maxX - minX),
			Math.Max(0, maxY - minY));
	}

}
