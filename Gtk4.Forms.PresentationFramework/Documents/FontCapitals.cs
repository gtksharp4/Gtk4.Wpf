namespace System.Windows.Documents;

/// <summary>
/// Specifies the capitalization style for text.
/// Maps to OpenType font features via Pango <c>font_features</c>.
/// </summary>
public enum FontCapitals
{
	/// <summary>No special capitalization. No OpenType feature applied.</summary>
	Normal,
	/// <summary>All characters rendered as small capitals (<c>smcp=1</c>).</summary>
	SmallCaps,
	/// <summary>Both upper and lowercase rendered as small capitals
	/// (<c>c2sc=1,smcp=1</c>).</summary>
	AllSmallCaps,
	/// <summary>All characters rendered as petite capitals (<c>pcap=1</c>).</summary>
	PetiteCaps,
	/// <summary>Both upper and lowercase rendered as petite capitals
	/// (<c>c2pc=1,pcap=1</c>).</summary>
	AllPetiteCaps,
	/// <summary>Unicase rendering — a mix of small capitals and normal lowercase
	/// (<c>unic=1</c>).</summary>
	Unicase,
	/// <summary>Titling capitals — lighter-weight capitals for large display use
	/// (<c>titl=1</c>).</summary>
	Titling,
}
