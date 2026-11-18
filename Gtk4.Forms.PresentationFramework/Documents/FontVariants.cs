namespace System.Windows.Documents;

/// <summary>
/// Specifies the glyph variant for text.
/// Maps to OpenType font features via Pango <c>font_features</c>.
/// </summary>
public enum FontVariants
{
	/// <summary>Default glyph variant. No OpenType feature applied.</summary>
	Normal,
	/// <summary>Superscript glyphs (<c>sups=1</c>).</summary>
	Superscript,
	/// <summary>Subscript glyphs (<c>subs=1</c>).</summary>
	Subscript,
	/// <summary>Ordinal glyphs — superscript letters used in ordinal numbers
	/// (<c>ordn=1</c>).</summary>
	Ordinal,
	/// <summary>Inferior (scientific subscript) glyphs (<c>sinf=1</c>).</summary>
	Inferior,
	/// <summary>Ruby (small annotation) glyphs (<c>ruby=1</c>).</summary>
	Ruby,
}
