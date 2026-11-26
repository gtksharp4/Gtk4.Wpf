namespace System.Windows.Media;

/// <summary>
/// A brush that fills an area with the content of a <see cref="Drawing"/>,
/// with optional stretching and tiling (inherited from <see cref="TileBrush"/>).
/// Mirrors <c>System.Windows.Media.DrawingBrush</c>.
/// </summary>
/// <remarks>
/// <b>What paints:</b> when <see cref="Drawing"/> is non-null, the drawing is rendered
/// into an off-screen Cairo <see cref="Cairo.ImageSurface"/>, then used as a surface
/// pattern mapped through the shared <see cref="TileBrush"/> mapping (Viewbox/Viewport/Stretch/Alignment) and tiled if
/// <see cref="TileBrush.TileMode"/> is not <see cref="TileMode.None"/>.
/// All four <see cref="Stretch"/> values are honoured via <see cref="StretchMath"/>.
/// <para>
/// <b>Object-model only (no paint):</b> constructing a <see cref="DrawingBrush"/> and
/// assigning it to a <see cref="System.Windows.Shapes.Shape.Fill"/> or
/// <see cref="System.Windows.Controls.Control.Background"/> is safe without a GTK surface;
/// <see cref="ApplyToCairo"/> is never called until an actual draw pass occurs.
/// </para>
/// <para>
/// <b>Headless / no display:</b> if <see cref="Drawing"/> is null, or its bounds are
/// zero/empty, <see cref="ApplyToCairo"/> is a no-op.
/// </para>
/// </remarks>
public partial class DrawingBrush : TileBrush
{
	readonly FreezableChildWatcher _watcher;

	/// <summary>Creates an empty drawing brush.</summary>
	public DrawingBrush() => _watcher = new FreezableChildWatcher(() => OnChanged());

	/// <summary>Creates a drawing brush backed by <paramref name="drawing"/>.</summary>
	public DrawingBrush(Drawing drawing) : this() => Drawing = drawing;

	/// <summary>Identifies <see cref="Drawing"/>.</summary>
	public static readonly DependencyProperty DrawingProperty = DependencyProperty.Register(
		nameof(Drawing), typeof(Drawing), typeof(DrawingBrush), new PropertyMetadata(null));

	/// <summary>The drawing used as the brush content.</summary>
	public Drawing? Drawing
	{
		get => (Drawing?)GetValue(DrawingProperty);
		set => SetValue(DrawingProperty, value);
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == DrawingProperty || e.Property == TransformProperty
			|| e.Property == RelativeTransformProperty)
			_watcher.Set(e.Property, e.NewValue);
	}

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		Drawing?.Freeze();
		Transform?.Freeze();
		RelativeTransform?.Freeze();
	}

	/// <inheritdoc/>
	internal override void ApplyToCairo(Cairo.Context cr, double width, double height)
	{
		var drawing = Drawing;
		if (drawing == null)
		{
			SetTransparentSource(cr);
			return;
		}
		ApplyTile(cr, width, height, drawing.Bounds, drawing.Draw);
	}

	/// <inheritdoc/>
	public override Gdk.RGBA ToGdkRgba() =>
		new Gdk.RGBA { Red = 0, Green = 0, Blue = 0, Alpha = (float)Opacity };
}
