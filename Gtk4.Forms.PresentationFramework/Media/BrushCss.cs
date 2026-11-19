using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Windows.Media;

/// <summary>
/// Pure WPF-to-GTK-CSS mapping for brushes and borders; returns CSS declarations (no selector,
/// no braces), or an empty string when nothing should be emitted.
/// <para>
/// <b>Gaps</b>: <see cref="ImageBrush"/>, <see cref="VisualBrush"/> and <see cref="DrawingBrush"/> have
/// no CSS form and are store-only. Linear gradients map relative <c>StartPoint</c>/<c>EndPoint</c> to a
/// CSS angle: exact for axis-aligned and square-box diagonals, approximate on non-square boxes (WPF
/// stretches the gradient line with the box, CSS keeps the angle). Gradient <c>color</c> (foreground)
/// falls back to the first stop since CSS text colour cannot be a gradient.
/// </para>
/// </summary>
internal static class BrushCss
{
	static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

	static string F(double v) => v.ToString("G6", Inv);

	internal static string Rgba(Color c, double opacity) =>
		$"rgba({c.R},{c.G},{c.B},{F(c.A / 255.0 * opacity)})";

	/// <summary><c>background-color</c> / <c>background-image</c> for <paramref name="brush"/>.</summary>
	internal static string Background(Brush? brush)
	{
		switch (brush)
		{
			case SolidColorBrush s:
				return $"background-color: {Rgba(s.Color, s.Opacity)};";
			case LinearGradientBrush l when l.GradientStops.Count > 0:
				return $"background-image: linear-gradient({F(LinearAngle(l))}deg, {Stops(l)});";
			case RadialGradientBrush r when r.GradientStops.Count > 0:
				return $"background-image: radial-gradient(ellipse {F(r.RadiusX * 100)}% {F(r.RadiusY * 100)}% " +
					$"at {F(r.Center.X * 100)}% {F(r.Center.Y * 100)}%, {Stops(r)});";
			default:
				return string.Empty;
		}
	}

	/// <summary>CSS <c>color</c> for a foreground brush (first stop for gradients).</summary>
	internal static string Foreground(Brush? brush)
	{
		switch (brush)
		{
			case SolidColorBrush s:
				return $"color: {Rgba(s.Color, s.Opacity)};";
			case GradientBrush g when g.GradientStops.Count > 0:
				return $"color: {Rgba(g.GradientStops.OrderBy(x => x.Offset).First().Color, g.Opacity)};";
			default:
				return string.Empty;
		}
	}

	/// <summary><c>border-color</c> for a border brush (solid only; first stop for gradients).</summary>
	internal static string BorderColor(Brush? brush)
	{
		string color = Foreground(brush);
		return color.Length == 0 ? string.Empty : "border-" + color;
	}

	/// <summary><c>border-width</c> in CSS order: top right bottom left.</summary>
	internal static string BorderWidth(Thickness t) =>
		$"border-width: {F(t.Top)}px {F(t.Right)}px {F(t.Bottom)}px {F(t.Left)}px;";

	/// <summary><c>border-radius</c> in CSS order: top-left top-right bottom-right bottom-left.</summary>
	internal static string BorderRadius(CornerRadius r) =>
		$"border-radius: {F(r.TopLeft)}px {F(r.TopRight)}px {F(r.BottomRight)}px {F(r.BottomLeft)}px;";

	/// <summary>CSS angle (0deg = to top, clockwise) of the start-to-end vector in screen space.</summary>
	internal static double LinearAngle(LinearGradientBrush b)
	{
		double dx = b.EndPoint.X - b.StartPoint.X;
		double dy = b.EndPoint.Y - b.StartPoint.Y;
		if (dx == 0 && dy == 0) return 90;
		double deg = Math.Atan2(dx, -dy) * 180.0 / Math.PI;
		return Math.Round((deg + 360.0) % 360.0, 4);
	}

	static string Stops(GradientBrush b)
	{
		var sb = new StringBuilder();
		foreach (var stop in b.GradientStops.OrderBy(s => s.Offset))
		{
			if (sb.Length > 0) sb.Append(", ");
			sb.Append(Rgba(stop.Color, b.Opacity)).Append(' ').Append(F(stop.Offset * 100)).Append('%');
		}
		return sb.ToString();
	}
}
