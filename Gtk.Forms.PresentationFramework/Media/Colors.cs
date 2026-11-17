namespace System.Windows.Media;

/// <summary>
/// Named WPF colours — a subset of the standard set, covering the most-used entries.
/// Values match WPF's <c>System.Windows.Media.Colors</c>.
/// </summary>
public static class Colors
{
	// ── Achromatic ───────────────────────────────────────────────────────────
	/// <summary>Transparent (#00FFFFFF).</summary>
	public static Color Transparent => Color.FromArgb(0, 255, 255, 255);
	/// <summary>Black (#FF000000).</summary>
	public static Color Black => Color.FromRgb(0, 0, 0);
	/// <summary>White (#FFFFFFFF).</summary>
	public static Color White => Color.FromRgb(255, 255, 255);
	/// <summary>Gray (#FF808080).</summary>
	public static Color Gray => Color.FromRgb(128, 128, 128);
	/// <summary>DarkGray (#FFA9A9A9).</summary>
	public static Color DarkGray => Color.FromRgb(169, 169, 169);
	/// <summary>LightGray (#FFD3D3D3).</summary>
	public static Color LightGray => Color.FromRgb(211, 211, 211);
	/// <summary>Silver (#FFC0C0C0).</summary>
	public static Color Silver => Color.FromRgb(192, 192, 192);

	// ── Primary / secondary ──────────────────────────────────────────────────
	/// <summary>Red (#FFFF0000).</summary>
	public static Color Red => Color.FromRgb(255, 0, 0);
	/// <summary>Green (#FF008000).</summary>
	public static Color Green => Color.FromRgb(0, 128, 0);
	/// <summary>Blue (#FF0000FF).</summary>
	public static Color Blue => Color.FromRgb(0, 0, 255);
	/// <summary>Yellow (#FFFFFF00).</summary>
	public static Color Yellow => Color.FromRgb(255, 255, 0);
	/// <summary>Cyan (#FF00FFFF).</summary>
	public static Color Cyan => Color.FromRgb(0, 255, 255);
	/// <summary>Magenta (#FFFF00FF).</summary>
	public static Color Magenta => Color.FromRgb(255, 0, 255);
	/// <summary>Orange (#FFFFA500).</summary>
	public static Color Orange => Color.FromRgb(255, 165, 0);
	/// <summary>Purple (#FF800080).</summary>
	public static Color Purple => Color.FromRgb(128, 0, 128);
	/// <summary>Brown (#FFA52A2A).</summary>
	public static Color Brown => Color.FromRgb(165, 42, 42);

	// ── Light variants ───────────────────────────────────────────────────────
	/// <summary>LightBlue (#FFADD8E6).</summary>
	public static Color LightBlue => Color.FromRgb(173, 216, 230);
	/// <summary>LightGreen (#FF90EE90).</summary>
	public static Color LightGreen => Color.FromRgb(144, 238, 144);
	/// <summary>LightYellow (#FFFFFFE0).</summary>
	public static Color LightYellow => Color.FromRgb(255, 255, 224);
	/// <summary>LightPink (#FFFFB6C1).</summary>
	public static Color LightPink => Color.FromRgb(255, 182, 193);
	/// <summary>LightCoral (#FFF08080).</summary>
	public static Color LightCoral => Color.FromRgb(240, 128, 128);

	// ── Dark variants ────────────────────────────────────────────────────────
	/// <summary>DarkRed (#FF8B0000).</summary>
	public static Color DarkRed => Color.FromRgb(139, 0, 0);
	/// <summary>DarkGreen (#FF006400).</summary>
	public static Color DarkGreen => Color.FromRgb(0, 100, 0);
	/// <summary>DarkBlue (#FF00008B).</summary>
	public static Color DarkBlue => Color.FromRgb(0, 0, 139);
	/// <summary>DarkOrange (#FFFF8C00).</summary>
	public static Color DarkOrange => Color.FromRgb(255, 140, 0);

	// ── Other named ──────────────────────────────────────────────────────────
	/// <summary>Pink (#FFFFC0CB).</summary>
	public static Color Pink => Color.FromRgb(255, 192, 203);
	/// <summary>Lime (#FF00FF00).</summary>
	public static Color Lime => Color.FromRgb(0, 255, 0);
	/// <summary>Navy (#FF000080).</summary>
	public static Color Navy => Color.FromRgb(0, 0, 128);
	/// <summary>Teal (#FF008080).</summary>
	public static Color Teal => Color.FromRgb(0, 128, 128);
	/// <summary>Maroon (#FF800000).</summary>
	public static Color Maroon => Color.FromRgb(128, 0, 0);
	/// <summary>Olive (#FF808000).</summary>
	public static Color Olive => Color.FromRgb(128, 128, 0);
	/// <summary>Coral (#FFFF7F50).</summary>
	public static Color Coral => Color.FromRgb(255, 127, 80);
	/// <summary>Gold (#FFFFD700).</summary>
	public static Color Gold => Color.FromRgb(255, 215, 0);
	/// <summary>Crimson (#FFDC143C).</summary>
	public static Color Crimson => Color.FromRgb(220, 20, 60);
	/// <summary>Violet (#FFEE82EE).</summary>
	public static Color Violet => Color.FromRgb(238, 130, 238);
	/// <summary>Indigo (#FF4B0082).</summary>
	public static Color Indigo => Color.FromRgb(75, 0, 130);
	/// <summary>Aqua — same as Cyan.</summary>
	public static Color Aqua => Color.FromRgb(0, 255, 255);
	/// <summary>Fuchsia — same as Magenta.</summary>
	public static Color Fuchsia => Color.FromRgb(255, 0, 255);
	/// <summary>Khaki (#FFF0E68C).</summary>
	public static Color Khaki => Color.FromRgb(240, 230, 140);
	/// <summary>Beige (#FFF5F5DC).</summary>
	public static Color Beige => Color.FromRgb(245, 245, 220);
	/// <summary>Snow (#FFFFFAFA).</summary>
	public static Color Snow => Color.FromRgb(255, 250, 250);
}