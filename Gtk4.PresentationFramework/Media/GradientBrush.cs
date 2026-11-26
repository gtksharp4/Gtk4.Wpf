using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Media;

/// <summary>
/// One colour-stop in a gradient: an offset in [0,1] and a colour.
/// Mirrors <c>System.Windows.Media.GradientStop</c>.
/// </summary>
public class GradientStop
{
	/// <summary>Creates a default stop (black at offset 0).</summary>
	public GradientStop() { }

	/// <summary>Creates a stop with specific colour and offset.</summary>
	public GradientStop(Color color, double offset) { Color = color; Offset = offset; }

	/// <summary>The colour at this stop.</summary>
	public Color Color { get; set; }

	/// <summary>Position in the gradient in [0,1].</summary>
	public double Offset { get; set; }
}

/// <summary>
/// An ordered collection of <see cref="GradientStop"/> instances.
/// Mirrors <c>System.Windows.Media.GradientStopCollection</c>.
/// </summary>
public class GradientStopCollection : List<GradientStop>
{
	/// <summary>Creates an empty collection.</summary>
	public GradientStopCollection() { }

	/// <summary>Creates a collection pre-populated from <paramref name="stops"/>.</summary>
	public GradientStopCollection(IEnumerable<GradientStop> stops) : base(stops) { }
}

/// <summary>
/// Base for gradient brushes.  Holds the <see cref="GradientStops"/> collection and
/// common gradient settings.  Mirrors <c>System.Windows.Media.GradientBrush</c>.
/// </summary>
public abstract partial class GradientBrush : Brush
{
	/// <summary>The gradient stops that define the colour transitions.</summary>
	public GradientStopCollection GradientStops { get; set; } = new GradientStopCollection();

	/// <summary>
	/// Coordinate space of the gradient points. Default <see cref="BrushMappingMode.RelativeToBoundingBox"/>.
	/// Stored only: points are always treated as relative.
	/// </summary>
	public BrushMappingMode MappingMode { get; set; } = BrushMappingMode.RelativeToBoundingBox;

	/// <summary>
	/// How the gradient fills beyond its end points. Default <see cref="GradientSpreadMethod.Pad"/>.
	/// Stored only: Cairo's default (pad) always applies.
	/// </summary>
	public GradientSpreadMethod SpreadMethod { get; set; } = GradientSpreadMethod.Pad;

	/// <summary>
	/// Returns the first stop's <see cref="Gdk.RGBA"/> as an approximation for
	/// widget colouring.  Full gradient paint is handled by <see cref="ApplyToCairo"/>.
	/// </summary>
	public override Gdk.RGBA ToGdkRgba()
	{
		var first = GradientStops.FirstOrDefault();
		if (first == null) return base.ToGdkRgba();
		var r = first.Color.ToGdkRgba();
		r.Alpha *= (float)Opacity;
		return r;
	}

	/// <summary>
	/// Helper: adds all gradient stops to a <see cref="Cairo.Gradient"/>.
	/// </summary>
	protected void AddStops(Cairo.Gradient pattern)
	{
		foreach (var stop in GradientStops)
		{
			var c = stop.Color.ToCairoColor();
			pattern.AddColorStop(stop.Offset, new Cairo.Color(c.R, c.G, c.B, c.A * Opacity));
		}
	}
}

/// <summary>
/// A brush that paints a linear gradient between two points.
/// Mirrors <c>System.Windows.Media.LinearGradientBrush</c>.
/// <para>
/// <b>Rendering note:</b> <see cref="StartPoint"/> and <see cref="EndPoint"/> use
/// WPF's relative coordinate space (0,0)–(1,1) mapped to the shape bounding box;
/// Cairo coordinates are computed at paint time from the draw-area dimensions.
/// </para>
/// </summary>
public partial class LinearGradientBrush : GradientBrush
{
	/// <summary>Start point in relative coordinates; default (0,0) = top-left.</summary>
	public System.Windows.Point StartPoint { get; set; } = new System.Windows.Point(0, 0);

	/// <summary>End point in relative coordinates; default (1,0) = top-right (horizontal).</summary>
	public System.Windows.Point EndPoint { get; set; } = new System.Windows.Point(1, 0);

	/// <summary>Convenience ctor: start/end colours with default horizontal direction.</summary>
	public LinearGradientBrush(Color startColor, Color endColor, double angle = 0)
	{
		GradientStops.Add(new GradientStop(startColor, 0));
		GradientStops.Add(new GradientStop(endColor, 1));
		// angle 90 → vertical; angle 0 → horizontal
		double rad = angle * Math.PI / 180.0;
		EndPoint = new System.Windows.Point(Math.Cos(rad), Math.Sin(rad));
	}

	/// <summary>Creates a brush with pre-defined stops.</summary>
	public LinearGradientBrush(GradientStopCollection stops,
		System.Windows.Point start, System.Windows.Point end)
	{
		GradientStops = stops;
		StartPoint = start;
		EndPoint = end;
	}

	/// <summary>Creates an empty brush.</summary>
	public LinearGradientBrush() { }

	/// <inheritdoc/>
	internal override void ApplyToCairo(Cairo.Context cr, double width, double height)
	{
		if (GradientStops.Count == 0) return;
		using var pat = new Cairo.LinearGradient(
			StartPoint.X * width, StartPoint.Y * height,
			EndPoint.X * width, EndPoint.Y * height);
		AddStops(pat);
		cr.SetSource(pat);
	}
}

/// <summary>
/// A brush that paints a radial gradient expanding from a focal point.
/// Mirrors <c>System.Windows.Media.RadialGradientBrush</c>.
/// <para>
/// <b>Rendering note:</b> all coordinates are in relative (0,0)–(1,1) space
/// mapped to the bounding box at paint time.
/// </para>
/// </summary>
public partial class RadialGradientBrush : GradientBrush
{
	/// <summary>Centre of the gradient ellipse; default (0.5,0.5).</summary>
	public System.Windows.Point Center { get; set; } = new System.Windows.Point(0.5, 0.5);

	/// <summary>Focal point; default equals <see cref="Center"/>.</summary>
	public System.Windows.Point GradientOrigin { get; set; } = new System.Windows.Point(0.5, 0.5);

	/// <summary>Horizontal radius as a fraction of the width; default 0.5.</summary>
	public double RadiusX { get; set; } = 0.5;

	/// <summary>Vertical radius as a fraction of the height; default 0.5.</summary>
	public double RadiusY { get; set; } = 0.5;

	/// <summary>Creates a radial brush with two colours (inner → outer).</summary>
	public RadialGradientBrush(Color innerColor, Color outerColor)
	{
		GradientStops.Add(new GradientStop(innerColor, 0));
		GradientStops.Add(new GradientStop(outerColor, 1));
	}

	/// <summary>Creates an empty radial brush.</summary>
	public RadialGradientBrush() { }

	/// <inheritdoc/>
	internal override void ApplyToCairo(Cairo.Context cr, double width, double height)
	{
		if (GradientStops.Count == 0) return;
		// Cairo's radial gradient takes two circles; approximate with equal centers + max radius.
		double cx = Center.X * width;
		double cy = Center.Y * height;
		double fx = GradientOrigin.X * width;
		double fy = GradientOrigin.Y * height;
		double radius = Math.Max(RadiusX * width, RadiusY * height);
		using var pat = new Cairo.RadialGradient(fx, fy, 0, cx, cy, radius);
		AddStops(pat);
		cr.SetSource(pat);
	}
}

/// <summary>
/// How a gradient fills the area outside its end points.
/// Mirrors <c>System.Windows.Media.GradientSpreadMethod</c>.
/// </summary>
public enum GradientSpreadMethod
{
	/// <summary>The end colours fill the rest.</summary>
	Pad,
	/// <summary>The gradient repeats in reverse.</summary>
	Reflect,
	/// <summary>The gradient repeats.</summary>
	Repeat,
}
