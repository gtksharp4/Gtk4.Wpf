namespace System.Windows.Media;

/// <summary>
/// Named <see cref="SolidColorBrush"/> instances corresponding to <see cref="Colors"/>.
/// Mirrors <c>System.Windows.Media.Brushes</c>.
/// Each property returns a new instance (brushes are mutable; WPF frozen brushes are
/// not implemented yet — freeze semantics are a Phase-5 addition).
/// </summary>
public static class Brushes
{
	// ── Achromatic ───────────────────────────────────────────────────────────
	/// <summary>Transparent brush.</summary>
	public static SolidColorBrush Transparent => new SolidColorBrush(Colors.Transparent);
	/// <summary>Black brush.</summary>
	public static SolidColorBrush Black => new SolidColorBrush(Colors.Black);
	/// <summary>White brush.</summary>
	public static SolidColorBrush White => new SolidColorBrush(Colors.White);
	/// <summary>Gray brush.</summary>
	public static SolidColorBrush Gray => new SolidColorBrush(Colors.Gray);
	/// <summary>DarkGray brush.</summary>
	public static SolidColorBrush DarkGray => new SolidColorBrush(Colors.DarkGray);
	/// <summary>LightGray brush.</summary>
	public static SolidColorBrush LightGray => new SolidColorBrush(Colors.LightGray);
	/// <summary>Silver brush.</summary>
	public static SolidColorBrush Silver => new SolidColorBrush(Colors.Silver);

	// ── Primary / secondary ──────────────────────────────────────────────────
	/// <summary>Red brush.</summary>
	public static SolidColorBrush Red => new SolidColorBrush(Colors.Red);
	/// <summary>Green brush.</summary>
	public static SolidColorBrush Green => new SolidColorBrush(Colors.Green);
	/// <summary>Blue brush.</summary>
	public static SolidColorBrush Blue => new SolidColorBrush(Colors.Blue);
	/// <summary>Yellow brush.</summary>
	public static SolidColorBrush Yellow => new SolidColorBrush(Colors.Yellow);
	/// <summary>Cyan brush.</summary>
	public static SolidColorBrush Cyan => new SolidColorBrush(Colors.Cyan);
	/// <summary>Magenta brush.</summary>
	public static SolidColorBrush Magenta => new SolidColorBrush(Colors.Magenta);
	/// <summary>Orange brush.</summary>
	public static SolidColorBrush Orange => new SolidColorBrush(Colors.Orange);
	/// <summary>Purple brush.</summary>
	public static SolidColorBrush Purple => new SolidColorBrush(Colors.Purple);
	/// <summary>Brown brush.</summary>
	public static SolidColorBrush Brown => new SolidColorBrush(Colors.Brown);

	// ── Light variants ───────────────────────────────────────────────────────
	/// <summary>LightBlue brush.</summary>
	public static SolidColorBrush LightBlue => new SolidColorBrush(Colors.LightBlue);
	/// <summary>LightGreen brush.</summary>
	public static SolidColorBrush LightGreen => new SolidColorBrush(Colors.LightGreen);
	/// <summary>LightYellow brush.</summary>
	public static SolidColorBrush LightYellow => new SolidColorBrush(Colors.LightYellow);
	/// <summary>LightPink brush.</summary>
	public static SolidColorBrush LightPink => new SolidColorBrush(Colors.LightPink);
	/// <summary>LightCoral brush.</summary>
	public static SolidColorBrush LightCoral => new SolidColorBrush(Colors.LightCoral);

	// ── Dark variants ────────────────────────────────────────────────────────
	/// <summary>DarkRed brush.</summary>
	public static SolidColorBrush DarkRed => new SolidColorBrush(Colors.DarkRed);
	/// <summary>DarkGreen brush.</summary>
	public static SolidColorBrush DarkGreen => new SolidColorBrush(Colors.DarkGreen);
	/// <summary>DarkBlue brush.</summary>
	public static SolidColorBrush DarkBlue => new SolidColorBrush(Colors.DarkBlue);
	/// <summary>DarkOrange brush.</summary>
	public static SolidColorBrush DarkOrange => new SolidColorBrush(Colors.DarkOrange);

	// ── Other named ──────────────────────────────────────────────────────────
	/// <summary>Pink brush.</summary>
	public static SolidColorBrush Pink => new SolidColorBrush(Colors.Pink);
	/// <summary>Lime brush.</summary>
	public static SolidColorBrush Lime => new SolidColorBrush(Colors.Lime);
	/// <summary>Navy brush.</summary>
	public static SolidColorBrush Navy => new SolidColorBrush(Colors.Navy);
	/// <summary>Teal brush.</summary>
	public static SolidColorBrush Teal => new SolidColorBrush(Colors.Teal);
	/// <summary>Maroon brush.</summary>
	public static SolidColorBrush Maroon => new SolidColorBrush(Colors.Maroon);
	/// <summary>Olive brush.</summary>
	public static SolidColorBrush Olive => new SolidColorBrush(Colors.Olive);
	/// <summary>Coral brush.</summary>
	public static SolidColorBrush Coral => new SolidColorBrush(Colors.Coral);
	/// <summary>Gold brush.</summary>
	public static SolidColorBrush Gold => new SolidColorBrush(Colors.Gold);
	/// <summary>Crimson brush.</summary>
	public static SolidColorBrush Crimson => new SolidColorBrush(Colors.Crimson);
	/// <summary>Violet brush.</summary>
	public static SolidColorBrush Violet => new SolidColorBrush(Colors.Violet);
	/// <summary>Indigo brush.</summary>
	public static SolidColorBrush Indigo => new SolidColorBrush(Colors.Indigo);
	/// <summary>Khaki brush.</summary>
	public static SolidColorBrush Khaki => new SolidColorBrush(Colors.Khaki);
	/// <summary>Beige brush.</summary>
	public static SolidColorBrush Beige => new SolidColorBrush(Colors.Beige);
}