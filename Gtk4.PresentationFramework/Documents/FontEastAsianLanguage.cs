namespace System.Windows.Documents;

/// <summary>Specifies the East Asian glyph form variant.</summary>
public enum FontEastAsianLanguage
{
	/// <summary>Font default.</summary>
	Normal = 0,

	/// <summary>JIS78 forms (<c>jp78=1</c>).</summary>
	Jis78 = 1,

	/// <summary>JIS83 forms (<c>jp83=1</c>).</summary>
	Jis83 = 2,

	/// <summary>JIS90 forms (<c>jp90=1</c>).</summary>
	Jis90 = 3,

	/// <summary>JIS2004 forms (<c>jp04=1</c>).</summary>
	Jis04 = 4,

	/// <summary>Hojo Kanji forms (<c>hojo=1</c>).</summary>
	HojoKanji = 5,

	/// <summary>NLC Kanji forms (<c>nlck=1</c>).</summary>
	NlcKanji = 6,

	/// <summary>Simplified Chinese forms (<c>smpl=1</c>).</summary>
	Simplified = 7,

	/// <summary>Traditional Chinese forms (<c>trad=1</c>).</summary>
	Traditional = 8,

	/// <summary>Traditional name forms (<c>tnam=1</c>).</summary>
	TraditionalNames = 9,
}
