namespace System.Windows.Documents;

/// <summary>
/// Specifies the numeral style for text.
/// Maps to OpenType font features via Pango <c>font_features</c>.
/// </summary>
public enum FontNumeralStyle
{
	/// <summary>Default numeral style. No OpenType feature applied.</summary>
	Normal,
	/// <summary>Lining (ranging) figures that sit on the baseline (<c>lnum=1</c>).</summary>
	Lining,
	/// <summary>Old-style figures with ascenders and descenders (<c>onum=1</c>).</summary>
	OldStyle,
}
