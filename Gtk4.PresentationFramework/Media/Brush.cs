using System;

namespace System.Windows.Media;

/// <summary>
/// Abstract base for all WPF brushes.  Mirrors <c>System.Windows.Media.Brush</c>;
/// implements <c>Freezable</c>-like semantics via <see cref="Opacity"/>.
/// </summary>
public abstract partial class Brush
{
	/// <summary>
	/// Applies this brush as the Cairo source on <paramref name="cr"/>.
	/// Subclasses override to paint their specific pattern.
	/// </summary>
	internal abstract void ApplyToCairo(Cairo.Context cr, double width, double height);

	/// <summary>
	/// Returns a <see cref="Gdk.RGBA"/> approximation suitable for GTK widget
	/// colouring.  Used when a full Cairo draw is not available (e.g. widget
	/// background hints).  Subclasses override for a faithful approximation;
	/// the default returns opaque black.
	/// </summary>
	public virtual Gdk.RGBA ToGdkRgba() =>
		new Gdk.RGBA { Red = 0, Green = 0, Blue = 0, Alpha = (float)Opacity };
}

/// <summary>
/// A brush that paints with a single solid <see cref="Color"/>.
/// Mirrors <c>System.Windows.Media.SolidColorBrush</c>.
/// </summary>
public partial class SolidColorBrush : Brush
{
	/// <summary>Creates a transparent solid brush.</summary>
	public SolidColorBrush() { }

	/// <summary>Creates a solid brush with the given colour.</summary>
	public SolidColorBrush(Color color) => Color = color;


	/// <inheritdoc/>
	internal override void ApplyToCairo(Cairo.Context cr, double width, double height)
	{
		var c = Color.ToCairoColor();
		cr.SetSourceRGBA(c.R, c.G, c.B, c.A * Opacity);
	}

	/// <inheritdoc/>
	public override Gdk.RGBA ToGdkRgba()
	{
		var r = Color.ToGdkRgba();
		r.Alpha *= (float)Opacity;
		return r;
	}
}