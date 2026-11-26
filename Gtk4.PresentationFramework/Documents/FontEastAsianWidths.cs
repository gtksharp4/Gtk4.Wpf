namespace System.Windows.Documents;

/// <summary>Specifies the glyph widths used for East Asian text.</summary>
public enum FontEastAsianWidths
{
	/// <summary>Font default.</summary>
	Normal = 0,

	/// <summary>Proportional widths (<c>pwid=1</c>).</summary>
	Proportional = 1,

	/// <summary>Full-width glyphs (<c>fwid=1</c>).</summary>
	Full = 2,

	/// <summary>Half-width glyphs (<c>hwid=1</c>).</summary>
	Half = 3,

	/// <summary>Third-width glyphs (<c>twid=1</c>).</summary>
	Third = 4,

	/// <summary>Quarter-width glyphs (<c>qwid=1</c>).</summary>
	Quarter = 5,
}
