using System;
using System.Globalization;
using System.Text;

namespace System.Windows.Media;

/// <summary>
/// Converts a WPF <see cref="Transform"/> and a relative <see cref="System.Windows.Point"/>
/// origin into the CSS strings required to drive a <c>Gtk.CssProvider</c>.
/// <para>
/// <b>What renders vs object-model-only</b>: <see cref="BuildCssTransform"/> and
/// <see cref="BuildCssOrigin"/> produce valid CSS. Whether GTK 4 honours the
/// <c>transform</c> CSS property depends on the GTK version and renderer in use
/// (confirmed supported from GTK 4.14 onwards). The object model (the DP values and
/// <see cref="Transform.Value"/>) always works regardless of GTK availability.
/// </para>
/// </summary>
internal static class RenderTransformCss
{
	const string Fmt = "G6";
	static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

	static string F(double v) => v.ToString(Fmt, Inv);

	/// <summary>
	/// Builds a CSS <c>transform</c> value string from <paramref name="transform"/>.
	/// Returns <c>"none"</c> for the identity transform.
	/// </summary>
	internal static string BuildCssTransform(Transform? transform)
	{
		if (transform == null) return "none";
		Matrix m = transform.Value;
		if (m.IsIdentity) return "none";
		// Use matrix() for maximum fidelity regardless of transform type.
		// CSS matrix(a,b,c,d,e,f) == affine matrix:
		//   | a  c  e |      a=M11 b=M12 c=M21 d=M22 e=OffsetX f=OffsetY
		//   | b  d  f |
		//   | 0  0  1 |
		return $"matrix({F(m.M11)},{F(m.M12)},{F(m.M21)},{F(m.M22)},{F(m.OffsetX)},{F(m.OffsetY)})";
	}

	/// <summary>
	/// Builds a CSS <c>transform-origin</c> value string from a normalised
	/// <paramref name="origin"/> (0..1 in each axis).
	/// </summary>
	internal static string BuildCssOrigin(Point origin) =>
		$"{F(origin.X * 100)}% {F(origin.Y * 100)}%";

	/// <summary>
	/// Builds a complete CSS snippet that assigns <c>transform</c> and
	/// <c>transform-origin</c> for a widget identified by <paramref name="cssName"/>.
	/// </summary>
	/// <param name="cssName">
	/// Value that will appear in the CSS selector. Typically <c>widget.name(X)</c>
	/// or a unique class name added to the widget.
	/// </param>
	internal static string BuildCssSnippet(string selector, Transform? transform, Point origin)
	{
		string tValue = BuildCssTransform(transform);
		string oValue = BuildCssOrigin(origin);
		return $"{selector} {{ transform: {tValue}; transform-origin: {oValue}; }}";
	}
}
