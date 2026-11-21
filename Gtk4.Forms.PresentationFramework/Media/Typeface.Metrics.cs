using System.Windows.Markup;

namespace System.Windows.Media;

public partial class Typeface
{
	GtkFontMetrics Metrics => GtkFontMetrics.Measure(Family?.Name ?? FontFamily.Source, Style, Weight, Stretch);

	/// <summary>True when the family has no face this heavy and GTK emboldens a lighter one.</summary>
	public bool IsBoldSimulated => Weight.ToOpenTypeWeight() >= 600 && ResolvedWeight() is int w && w > 0 && w < 600;

	/// <summary>True when the family has no italic or oblique face and GTK slants an upright one.</summary>
	public bool IsObliqueSimulated => Style != FontStyles.Normal && Face is GtkFaceInfo f && f.Style == FontStyles.Normal;

	int ResolvedWeight() => Face?.Weight.ToOpenTypeWeight() ?? Metrics.ResolvedWeight;

	/// <summary>The face name keyed by language: the Pango face name, or Regular/Bold/Italic style words.</summary>
	public LanguageSpecificStringDictionary FaceNames =>
		LanguageSpecificStringDictionary.Single("en-US", Face?.Name ?? DescribeFace());

	string DescribeFace()
	{
		bool bold = Weight.ToOpenTypeWeight() >= 600, italic = Style != FontStyles.Normal;
		return bold && italic ? "Bold " + Style : bold ? "Bold" : italic ? Style.ToString() : "Regular";
	}

	/// <summary>Capital letter height as an em ratio.</summary>
	public double CapsHeight => Metrics.CapsHeight;

	/// <summary>Lowercase x height as an em ratio.</summary>
	public double XHeight => Metrics.XHeight;

	/// <summary>Underline position relative to the baseline, as an em ratio.</summary>
	public double UnderlinePosition => Metrics.UnderlinePosition;

	/// <summary>Underline thickness as an em ratio.</summary>
	public double UnderlineThickness => Metrics.UnderlineThickness;

	/// <summary>Strikethrough position relative to the baseline, as an em ratio.</summary>
	public double StrikethroughPosition => Metrics.StrikethroughPosition;

	/// <summary>Strikethrough thickness as an em ratio.</summary>
	public double StrikethroughThickness => Metrics.StrikethroughThickness;

	/// <summary>The glyph typeface for the font, or false when GTK does not list the family.</summary>
	public bool TryGetGlyphTypeface(out GlyphTypeface glyphTypeface)
	{
		glyphTypeface = null!;
		if (Face == null) return false;
		glyphTypeface = new GlyphTypeface(this, Family!.Name, Face);
		return true;
	}
}
