using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Windows.Media;

/// <summary>
/// Pure WPF-to-GTK-CSS mapping for font properties. A null argument means "not set" and emits
/// nothing, so unset properties keep the theme (and GTK CSS inheritance) in charge.
/// </summary>
internal static class FontCss
{
	static readonly string[] StretchKeywords =
	{
		"ultra-condensed", "extra-condensed", "condensed", "semi-condensed", "normal",
		"semi-expanded", "expanded", "extra-expanded", "ultra-expanded",
	};

	internal static string Build(
		FontFamily? family, double? size, FontWeight? weight, FontStyle? style, FontStretch? stretch)
	{
		var sb = new StringBuilder();
		if (family != null && Family(family).Length > 0) Add(sb, "font-family: " + Family(family));
		if (size.HasValue && size.Value > 0 && !double.IsNaN(size.Value) && !double.IsInfinity(size.Value))
			Add(sb, "font-size: " + Size(size.Value));
		if (weight.HasValue)
			Add(sb, "font-weight: " + weight.Value.ToOpenTypeWeight().ToString(CultureInfo.InvariantCulture));
		if (style.HasValue) Add(sb, "font-style: " + style.Value.ToString().ToLowerInvariant());
		if (stretch.HasValue) Add(sb, "font-stretch: " + StretchKeywords[stretch.Value.ToOpenTypeStretch() - 1]);
		return sb.ToString();
	}

	/// <summary>
	/// Quotes each comma-separated entry, escaping backslashes and double quotes. A location such as
	/// <c>./Fonts/#Roboto</c> is reduced to the family name after the last <c>#</c>.
	/// </summary>
	/// <remarks>
	/// Font files referenced by URI are not loaded; the family resolves only if installed or
	/// registered with fontconfig.
	/// </remarks>
	internal static string Family(FontFamily family)
	{
		var names = family.Source.Split(',')
			.Select(n => n.Trim())
			.Select(n => n.LastIndexOf('#') is var i && i >= 0 ? n.Substring(i + 1).Trim() : n)
			.Where(n => n.Length > 0)
			.Select(n => "\"" + n.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
		return string.Join(", ", names);
	}

	/// <summary>WPF DIPs equal CSS px at 96 dpi.</summary>
	internal static string Size(double dips) => dips.ToString("G6", CultureInfo.InvariantCulture) + "px";

	static void Add(StringBuilder sb, string declaration)
	{
		if (sb.Length > 0) sb.Append(' ');
		sb.Append(declaration).Append(';');
	}
}
