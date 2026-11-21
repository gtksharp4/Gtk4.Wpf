using System.Collections.Generic;
using System.Globalization;

namespace System.Windows.Media;

/// <summary>
/// A physical face of a font as GTK sees it. Pango exposes family, face, style, weight, stretch and metrics;
/// the font name-table strings it does not expose (copyright, designer, licence, ...) are empty dictionaries.
/// </summary>
public sealed partial class GlyphTypeface
{
	readonly Typeface _typeface;
	readonly string _family;
	readonly GtkFaceInfo _face;
	readonly GtkFontMetrics _metrics;

	internal GlyphTypeface(Typeface typeface, string family, GtkFaceInfo face)
	{
		_typeface = typeface;
		_family = family;
		_face = face;
		_metrics = GtkFontMetrics.Measure(family, face.Style, face.Weight, face.Stretch);
	}

	/// <summary>Loading a face from a font file is not supported: Pango has no per-file loader.</summary>
	/// <exception cref="NotSupportedException">Always.</exception>
	public GlyphTypeface(Uri typefaceSource) : this(typefaceSource, StyleSimulations.None)
	{
	}

	/// <summary>Loading a face from a font file is not supported: Pango has no per-file loader.</summary>
	/// <exception cref="NotSupportedException">Always.</exception>
	public GlyphTypeface(Uri typefaceSource, StyleSimulations styleSimulations)
	{
		if (typefaceSource == null) throw new ArgumentNullException(nameof(typefaceSource));
		throw new NotSupportedException("A GlyphTypeface cannot be loaded from a font file under GTK.");
	}

	/// <summary>
	/// Identifies the face. Pango does not expose the font file, so this is a <c>urn:pango-font:</c> URI
	/// built from the family and face names.
	/// </summary>
	public Uri FontUri => new Uri("urn:pango-font:" + Uri.EscapeDataString(_family + "/" + _face.Name));

	/// <summary>The face style.</summary>
	public FontStyle Style => _face.Style;

	/// <summary>The face weight.</summary>
	public FontWeight Weight => _face.Weight;

	/// <summary>The face stretch.</summary>
	public FontStretch Stretch => _face.Stretch;

	/// <summary>Simulations GTK applies for the requested typeface.</summary>
	public StyleSimulations StyleSimulations =>
		(_typeface.IsBoldSimulated ? StyleSimulations.BoldSimulation : StyleSimulations.None)
		| (_typeface.IsObliqueSimulated ? StyleSimulations.ItalicSimulation : StyleSimulations.None);

	/// <summary>The font version; Pango does not expose it, so 0.</summary>
	public double Version => 0.0;

	/// <summary>Distance from the top of the line to the baseline, as an em ratio.</summary>
	public double Baseline => _metrics.Baseline;

	/// <summary>Line height as an em ratio.</summary>
	public double Height => _metrics.Height;

	/// <summary>Capital letter height as an em ratio.</summary>
	public double CapsHeight => _metrics.CapsHeight;

	/// <summary>Lowercase x height as an em ratio.</summary>
	public double XHeight => _metrics.XHeight;

	/// <summary>Underline position relative to the baseline, as an em ratio.</summary>
	public double UnderlinePosition => _metrics.UnderlinePosition;

	/// <summary>Underline thickness as an em ratio.</summary>
	public double UnderlineThickness => _metrics.UnderlineThickness;

	/// <summary>Strikethrough position relative to the baseline, as an em ratio.</summary>
	public double StrikethroughPosition => _metrics.StrikethroughPosition;

	/// <summary>Strikethrough thickness as an em ratio.</summary>
	public double StrikethroughThickness => _metrics.StrikethroughThickness;

	/// <summary>True for symbol fonts; Pango does not say, so false.</summary>
	public bool Symbol => false;

	static readonly CultureInfo En = CultureInfo.GetCultureInfo("en-US");

	static IDictionary<CultureInfo, string> One(string value) =>
		new System.Collections.ObjectModel.ReadOnlyDictionary<CultureInfo, string>(
			new Dictionary<CultureInfo, string> { [En] = value });

	static IDictionary<CultureInfo, string> Empty() =>
		new System.Collections.ObjectModel.ReadOnlyDictionary<CultureInfo, string>(
			new Dictionary<CultureInfo, string>());

	/// <summary>The family name (en-US).</summary>
	public IDictionary<CultureInfo, string> FamilyNames => One(_family);

	/// <summary>The face name (en-US).</summary>
	public IDictionary<CultureInfo, string> FaceNames => One(_face.Name);

	/// <summary>The family name as GDI would list it; the same Pango family name.</summary>
	public IDictionary<CultureInfo, string> Win32FamilyNames => One(_family);

	/// <summary>The face name as GDI would list it; the same Pango face name.</summary>
	public IDictionary<CultureInfo, string> Win32FaceNames => One(_face.Name);

	/// <summary>Name-table version strings; empty (not exposed by Pango).</summary>
	public IDictionary<CultureInfo, string> VersionStrings => Empty();

	/// <summary>Name-table copyright notices; empty (not exposed by Pango).</summary>
	public IDictionary<CultureInfo, string> Copyrights => Empty();

	/// <summary>Name-table trademarks; empty (not exposed by Pango).</summary>
	public IDictionary<CultureInfo, string> Trademarks => Empty();

	/// <summary>Name-table manufacturer names; empty (not exposed by Pango).</summary>
	public IDictionary<CultureInfo, string> ManufacturerNames => Empty();

	/// <summary>Name-table designer names; empty (not exposed by Pango).</summary>
	public IDictionary<CultureInfo, string> DesignerNames => Empty();

	/// <summary>Name-table designer URLs; empty (not exposed by Pango).</summary>
	public IDictionary<CultureInfo, string> DesignerUrls => Empty();

	/// <summary>Name-table vendor URLs; empty (not exposed by Pango).</summary>
	public IDictionary<CultureInfo, string> VendorUrls => Empty();

	/// <summary>Name-table descriptions; empty (not exposed by Pango).</summary>
	public IDictionary<CultureInfo, string> Descriptions => Empty();

	/// <summary>Name-table licence descriptions; empty (not exposed by Pango).</summary>
	public IDictionary<CultureInfo, string> LicenseDescriptions => Empty();

	/// <summary>Name-table sample texts; empty (not exposed by Pango).</summary>
	public IDictionary<CultureInfo, string> SampleTexts => Empty();
}
