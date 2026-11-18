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
/// </remarks>
public class GeometryDrawing : Drawing
{
	/// <summary>The geometry to fill and/or stroke.</summary>
	public Geometry? Geometry { get; set; }

	/// <summary>Brush used to fill the geometry interior; null for no fill.</summary>
	public Brush? Brush { get; set; }

	/// <summary>Pen used to stroke the geometry outline; null for no stroke.</summary>
	public Pen? Pen { get; set; }

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
