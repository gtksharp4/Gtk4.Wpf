namespace System.Windows.Media;

/// <summary>
/// A brush that fills an area with the content of a <see cref="Visual"/>, mapped and tiled like WPF's
/// <see cref="TileBrush"/> (Viewbox, Viewport, Stretch, Alignment, TileMode, Transform, RelativeTransform, Opacity).
/// Mirrors <c>System.Windows.Media.VisualBrush</c>.
/// </summary>
/// <remarks>
/// <b>Sources:</b> a <see cref="DrawingVisual"/> replays its recorded commands. A <see cref="UIElement"/> is rendered
/// from its GTK widget (a <c>Gtk.WidgetPaintable</c> drawn into a cached Cairo surface that is refreshed when the
/// widget invalidates), see <c>VisualBrush.Source.cs</c>. Content bounds are (0,0,RenderSize).
/// <para>
/// <b>Live refresh:</b> the brush raises <c>Changed</c> (one per main-loop turn) when the source widget invalidates,
/// so bindings, layout, animation and size changes repaint the owner. A brush painted inside its own source returns
/// the last frame. Changing only the Viewbox keeps the cached source pixels.
/// </para>
/// <para>
/// <b>Detached sources:</b> an unparented element is measured to infinity and arranged to its desired size
/// (<see cref="AutoLayoutContent"/>), then mapped inside one shared, near-transparent, non-interactive host window
/// (<c>DetachedSourceHost</c>, not a WPF window) that is released with the last brush.
/// </para>
/// <para>
/// <b>Gaps:</b> descendant overflow is clipped to the render size (WPF uses descendant bounds); the tile raster is
/// at device scale 1 (no HiDPI factor). A <c>Control</c> Background (BrushCss) and a TextBlock Foreground paint a
/// VisualBrush as nothing rather than black until those owners call <c>BrushPaint.Fill</c> from a Backdrop snapshot;
/// <c>Shape.OpacityMask</c> is store-only until Shape gets the same snapshot hook. A source that is not realised or has
/// no size paints transparent.
/// </para>
/// </remarks>
public partial class VisualBrush : TileBrush
{
	/// <summary>Creates an empty visual brush.</summary>
	public VisualBrush() { }

	/// <summary>Creates a visual brush backed by <paramref name="visual"/>.</summary>
	public VisualBrush(Visual visual) => Visual = visual;

	/// <summary>Identifies <see cref="Visual"/>. Default null.</summary>
	public static readonly DependencyProperty VisualProperty = DependencyProperty.Register(
		nameof(Visual), typeof(Visual), typeof(VisualBrush), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="AutoLayoutContent"/>. Default true.</summary>
	public static readonly DependencyProperty AutoLayoutContentProperty = DependencyProperty.Register(
		nameof(AutoLayoutContent), typeof(bool), typeof(VisualBrush), new PropertyMetadata(true));

	/// <summary>The visual used as the brush content.</summary>
	public Visual? Visual
	{
		get => (Visual?)GetValue(VisualProperty);
		set => SetValue(VisualProperty, value);
	}

	/// <summary>Whether an unparented <see cref="UIElement"/> source is measured and arranged to its desired size.</summary>
	public bool AutoLayoutContent
	{
		get => (bool)GetValue(AutoLayoutContentProperty)!;
		set => SetValue(AutoLayoutContentProperty, value);
	}

	/// <inheritdoc/>
	internal override void ApplyToCairo(Cairo.Context cr, double width, double height)
	{
		switch (Visual)
		{
			case DrawingVisual { Drawing: { Commands.Count: > 0 } ctx }:
				ApplyTile(cr, width, height, DrawingContextRenderer.ComputeBounds(ctx),
					c => DrawingContextRenderer.Render(ctx, c, paintText: true));
				break;
			case UIElement element:
				ApplyElement(cr, width, height, element);
				break;
			default:
				TileBrush.SetTransparentSource(cr);
				break;
		}
	}

	/// <inheritdoc/>
	public override Gdk.RGBA ToGdkRgba() =>
		new Gdk.RGBA { Red = 0, Green = 0, Blue = 0, Alpha = (float)Opacity };
}
