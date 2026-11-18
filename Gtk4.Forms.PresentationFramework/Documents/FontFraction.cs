namespace System.Windows.Documents;

/// <summary>
/// Specifies the fraction display style for text.
/// Maps to OpenType font features via Pango <c>font_features</c>.
/// </summary>
public enum FontFraction
{
	/// <summary>Default fraction rendering. No OpenType feature applied.</summary>
	Normal,
	/// <summary>Diagonal (slashed) fractions (<c>frac=1</c>).</summary>
	Slashed,
	/// <summary>Stacked (vertical) fractions (<c>afrc=1</c>).</summary>
	Stacked,
}
