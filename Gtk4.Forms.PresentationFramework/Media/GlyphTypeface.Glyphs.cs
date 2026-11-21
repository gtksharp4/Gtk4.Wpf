using System.Collections.Generic;

namespace System.Windows.Media;

public sealed partial class GlyphTypeface
{
	IDictionary<int, ushort>? _map;

	/// <summary>
	/// The code points of the Basic Multilingual Plane the face covers. Pango reports coverage, not glyph indices,
	/// so each value is the position in coverage order, not the font glyph id.
	/// </summary>
	public IDictionary<int, ushort> CharacterToGlyphMap => _map ?? (_map = ReadCoverage());

	/// <summary>The number of covered code points (see <see cref="CharacterToGlyphMap"/>).</summary>
	public int GlyphCount => CharacterToGlyphMap.Count;

	IDictionary<int, ushort> ReadCoverage()
	{
		var map = new Dictionary<int, ushort>();
		var fontMap = GtkFontCatalog.FontMap;
		if (fontMap == null) return map;
		try
		{
			using (var desc = new Pango.FontDescription())
			{
				desc.Family = _family;
				desc.Style = Style == FontStyles.Italic ? Pango.Style.Italic
					: Style == FontStyles.Oblique ? Pango.Style.Oblique : Pango.Style.Normal;
				desc.Weight = (Pango.Weight)Weight.ToOpenTypeWeight();
				desc.Stretch = (Pango.Stretch)(Stretch.ToOpenTypeStretch() - 1);
				desc.Size = 10 * 1024;
				var font = fontMap.CreateContext().LoadFont(desc);
				if (font == null) return map;
				ushort glyph = 1;
				for (int c = 0x20; c <= 0xFFFF && glyph < ushort.MaxValue; c++)
				{
					if (c >= 0xD800 && c <= 0xDFFF) continue;
					if (font.HasChar((char)c)) map[c] = glyph++;
				}
			}
		}
		catch (Exception)
		{
			// Coverage is best effort.
		}
		return map;
	}
}
