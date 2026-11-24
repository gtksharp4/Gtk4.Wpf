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
/// <b>Gaps:</b> an element with no background or with descendants overflowing its bounds is clipped to its render
/// size (WPF uses the descendant bounds). A source that is not realised, or has no size, paints transparent.
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
