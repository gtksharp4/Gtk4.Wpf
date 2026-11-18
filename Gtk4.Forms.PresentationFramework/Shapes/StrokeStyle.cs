using System;
using System.Windows.Media;

namespace System.Windows.Shapes;

/// <summary>
/// Pure, surface-free helpers that convert WPF stroke-style values to Cairo equivalents.
/// Extracted for unit-testability without a GTK surface.
/// </summary>
/// <remarks>
/// <para>
/// <b>Dash array:</b> WPF dash values are multiples of <c>StrokeThickness</c>;
/// Cairo dash values are in user-space units.  <see cref="ToCairoDashes"/> performs
/// the multiplication.
/// </para>
/// <para>
/// <b>Line caps:</b> Cairo has a single <c>LineCap</c> applied to both ends of an
/// open path.  WPF distinguishes <c>StrokeStartLineCap</c> and
/// <c>StrokeEndLineCap</c>.  This implementation maps the <em>start cap</em> to
/// Cairo's single cap; the end cap is ignored.  Shapes with open paths (e.g.
/// <c>Line</c>) may render with the same cap at both ends regardless of
/// <c>StrokeEndLineCap</c>.
/// </para>
/// <para>
/// <b><c>PenDashCap</c>:</b> Cairo has no per-dash-segment cap concept; the
/// <c>StrokeDashCap</c> property is accepted for API compatibility but not applied.
/// </para>
/// </remarks>
internal static class StrokeStyle
{
	/// <summary>
	/// Converts a WPF dash array to Cairo dashes by multiplying each value by
	/// <paramref name="thickness"/>.  Returns an empty array when
	/// <paramref name="dashes"/> is null or empty (meaning no dash pattern).
	/// <para>
	/// An odd-length pattern is passed through unchanged; Cairo (like WPF) implicitly
	/// repeats the pattern, so the dash/gap sequence matches visually.
	/// </para>
	/// </summary>
	internal static double[] ToCairoDashes(DoubleCollection? dashes, double thickness)
	{
		if (dashes == null || dashes.Count == 0)
			return Array.Empty<double>();

		var result = new double[dashes.Count];
		for (int i = 0; i < dashes.Count; i++)
			result[i] = dashes[i] * thickness;
		return result;
	}

	/// <summary>
	/// Maps a <see cref="PenLineCap"/> to the corresponding <see cref="Cairo.LineCap"/>.
	/// <c>Triangle</c> has no Cairo equivalent and falls back to <c>Butt</c>.
	/// </summary>
	internal static Cairo.LineCap ToCairoLineCap(PenLineCap cap) =>
		cap switch
		{
			PenLineCap.Flat     => Cairo.LineCap.Butt,
			PenLineCap.Square   => Cairo.LineCap.Square,
			PenLineCap.Round    => Cairo.LineCap.Round,
			PenLineCap.Triangle => Cairo.LineCap.Butt, // no Cairo equivalent
			_                   => Cairo.LineCap.Butt,
		};

	/// <summary>
	/// Maps a <see cref="PenLineJoin"/> to the corresponding <see cref="Cairo.LineJoin"/>.
	/// </summary>
	internal static Cairo.LineJoin ToCairoLineJoin(PenLineJoin join) =>
		join switch
		{
			PenLineJoin.Miter => Cairo.LineJoin.Miter,
			PenLineJoin.Bevel => Cairo.LineJoin.Bevel,
			PenLineJoin.Round => Cairo.LineJoin.Round,
			_                 => Cairo.LineJoin.Miter,
		};

	/// <summary>
	/// Applies all stroke-style properties to <paramref name="cr"/> before the
	/// Cairo <c>Stroke()</c> call.  Must be called after <c>LineWidth</c> is set.
	/// </summary>
	internal static void Apply(
		Cairo.Context cr,
		DoubleCollection? dashArray,
		double dashOffset,
		double thickness,
		PenLineCap startLineCap,
		PenLineJoin lineJoin,
		double miterLimit)
	{
		var cairoDashes = ToCairoDashes(dashArray, thickness);
		if (cairoDashes.Length > 0)
			cr.SetDash(cairoDashes, dashOffset * thickness);
		else
			cr.SetDash(Array.Empty<double>(), 0);

		cr.LineCap  = ToCairoLineCap(startLineCap);
		cr.LineJoin = ToCairoLineJoin(lineJoin);
		cr.MiterLimit = miterLimit;
	}
}
