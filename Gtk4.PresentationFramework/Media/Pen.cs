namespace System.Windows.Media;

/// <summary>
/// Describes how a stroke is painted: brush + thickness.
/// Mirrors <c>System.Windows.Media.Pen</c>.
/// </summary>
public partial class Pen
{
	/// <summary>Creates a default pen (null brush, 1-unit thickness).</summary>
	public Pen() { }

	/// <summary>Creates a pen with a specific brush and thickness.</summary>
	public Pen(Brush brush, double thickness)
	{
		Brush = brush;
		Thickness = thickness;
	}

	/// <summary>The brush used to paint the stroke.</summary>
	public Brush? Brush { get; set; }

	/// <summary>Stroke width in device-independent units.</summary>
	public double Thickness { get; set; } = 1.0;

	/// <summary>
	/// Applies this pen to a <see cref="Cairo.Context"/> (line width + source).
	/// Call after building the path; then call <c>Stroke()</c> or <c>StrokePreserve()</c>.
	/// </summary>
	internal void ApplyToCairo(Cairo.Context cr, double width, double height)
	{
		cr.LineWidth = Thickness;
		Brush?.ApplyToCairo(cr, width, height);
	}
}