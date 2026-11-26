namespace System.Windows.Media;

/// <summary>
/// A drawing that paints a <see cref="Geometry"/> with an optional fill <see cref="Brush"/>
/// and/or outline <see cref="Pen"/>.
/// Mirrors <c>System.Windows.Media.GeometryDrawing</c>.
/// </summary>
/// <remarks>
/// <b>What paints:</b> <see cref="Draw"/> builds the Cairo path from <see cref="Geometry"/>
/// and fills / strokes using the existing brush→Cairo and <see cref="Pen"/> machinery.
/// Fill is painted first, then stroke, matching WPF painter's model.
/// <b>Object-model only:</b> all properties are available without a GTK surface.
/// <see cref="Pen"/> is not a <see cref="Freezable"/> here, so its changes do not propagate.
/// </remarks>
public class GeometryDrawing : Drawing
{
	readonly FreezableChildWatcher _watcher;

	/// <summary>Creates an empty geometry drawing.</summary>
	public GeometryDrawing() => _watcher = new FreezableChildWatcher(() => OnChanged());

	/// <summary>Creates a geometry drawing.</summary>
	public GeometryDrawing(Brush? brush, Pen? pen, Geometry? geometry) : this()
	{
		if (brush != null) Brush = brush;
		if (pen != null) Pen = pen;
		if (geometry != null) Geometry = geometry;
	}

	/// <summary>Identifies <see cref="Geometry"/>.</summary>
	public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register(
		nameof(Geometry), typeof(Geometry), typeof(GeometryDrawing), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="Brush"/>.</summary>
	public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
		nameof(Brush), typeof(Brush), typeof(GeometryDrawing), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="Pen"/>.</summary>
	public static readonly DependencyProperty PenProperty = DependencyProperty.Register(
		nameof(Pen), typeof(Pen), typeof(GeometryDrawing), new PropertyMetadata(null));

	/// <summary>The geometry to fill and/or stroke.</summary>
	public Geometry? Geometry
	{
		get => (Geometry?)GetValue(GeometryProperty);
		set => SetValue(GeometryProperty, value);
	}

	/// <summary>Brush used to fill the geometry interior; null for no fill.</summary>
	public Brush? Brush
	{
		get => (Brush?)GetValue(BrushProperty);
		set => SetValue(BrushProperty, value);
	}

	/// <summary>Pen used to stroke the geometry outline; null for no stroke.</summary>
	public Pen? Pen
	{
		get => (Pen?)GetValue(PenProperty);
		set => SetValue(PenProperty, value);
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == GeometryProperty || e.Property == BrushProperty)
			_watcher.Set(e.Property, e.NewValue);
	}

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		Geometry?.Freeze();
		Brush?.Freeze();
	}

	/// <inheritdoc/>
	public override Rect Bounds => Geometry?.Bounds ?? Rect.Empty;

	/// <inheritdoc/>
	internal override void Draw(Cairo.Context cr)
	{
		var geom = Geometry;
		if (geom == null) return;

		double w = Bounds.Width;
		double h = Bounds.Height;

		if (Brush != null)
		{
			cr.Save();
			geom.BuildPath(cr);
			Brush.ApplyToCairo(cr, w, h);
			cr.FillPreserve();
			cr.NewPath();
			cr.Restore();
		}

		if (Pen?.Brush != null)
		{
			cr.Save();
			geom.BuildPath(cr);
			Pen.ApplyToCairo(cr, w, h);
			cr.Stroke();
			cr.Restore();
		}
	}
}
