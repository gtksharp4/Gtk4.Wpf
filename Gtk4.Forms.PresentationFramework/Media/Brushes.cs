namespace System.Windows.Media;

/// <summary>
/// Named <see cref="SolidColorBrush"/> instances for each of the 141 <see cref="Colors"/>.
/// Mirrors <c>System.Windows.Media.Brushes</c>. Each access returns a new instance: brushes here
/// are not <c>Freezable</c>, so WPF's shared frozen instances are not reproduced (a shared mutable
/// brush would leak edits between consumers). The second half of the table lives in <c>Brushes.MtoZ.cs</c>.
/// </summary>
public static partial class Brushes
{
	/// <summary>AliceBlue brush.</summary>
	public static SolidColorBrush AliceBlue => new SolidColorBrush(Colors.AliceBlue);
	/// <summary>AntiqueWhite brush.</summary>
	public static SolidColorBrush AntiqueWhite => new SolidColorBrush(Colors.AntiqueWhite);
	/// <summary>Aqua brush.</summary>
	public static SolidColorBrush Aqua => new SolidColorBrush(Colors.Aqua);
	/// <summary>Aquamarine brush.</summary>
	public static SolidColorBrush Aquamarine => new SolidColorBrush(Colors.Aquamarine);
	/// <summary>Azure brush.</summary>
	public static SolidColorBrush Azure => new SolidColorBrush(Colors.Azure);
	/// <summary>Beige brush.</summary>
	public static SolidColorBrush Beige => new SolidColorBrush(Colors.Beige);
	/// <summary>Bisque brush.</summary>
	public static SolidColorBrush Bisque => new SolidColorBrush(Colors.Bisque);
	/// <summary>Black brush.</summary>
	public static SolidColorBrush Black => new SolidColorBrush(Colors.Black);
	/// <summary>BlanchedAlmond brush.</summary>
	public static SolidColorBrush BlanchedAlmond => new SolidColorBrush(Colors.BlanchedAlmond);
	/// <summary>Blue brush.</summary>
	public static SolidColorBrush Blue => new SolidColorBrush(Colors.Blue);
	/// <summary>BlueViolet brush.</summary>
	public static SolidColorBrush BlueViolet => new SolidColorBrush(Colors.BlueViolet);
	/// <summary>Brown brush.</summary>
	public static SolidColorBrush Brown => new SolidColorBrush(Colors.Brown);
	/// <summary>BurlyWood brush.</summary>
	public static SolidColorBrush BurlyWood => new SolidColorBrush(Colors.BurlyWood);
	/// <summary>CadetBlue brush.</summary>
	public static SolidColorBrush CadetBlue => new SolidColorBrush(Colors.CadetBlue);
	/// <summary>Chartreuse brush.</summary>
	public static SolidColorBrush Chartreuse => new SolidColorBrush(Colors.Chartreuse);
	/// <summary>Chocolate brush.</summary>
	public static SolidColorBrush Chocolate => new SolidColorBrush(Colors.Chocolate);
	/// <summary>Coral brush.</summary>
	public static SolidColorBrush Coral => new SolidColorBrush(Colors.Coral);
	/// <summary>CornflowerBlue brush.</summary>
	public static SolidColorBrush CornflowerBlue => new SolidColorBrush(Colors.CornflowerBlue);
	/// <summary>Cornsilk brush.</summary>
	public static SolidColorBrush Cornsilk => new SolidColorBrush(Colors.Cornsilk);
	/// <summary>Crimson brush.</summary>
	public static SolidColorBrush Crimson => new SolidColorBrush(Colors.Crimson);
	/// <summary>Cyan brush.</summary>
	public static SolidColorBrush Cyan => new SolidColorBrush(Colors.Cyan);
	/// <summary>DarkBlue brush.</summary>
	public static SolidColorBrush DarkBlue => new SolidColorBrush(Colors.DarkBlue);
	/// <summary>DarkCyan brush.</summary>
	public static SolidColorBrush DarkCyan => new SolidColorBrush(Colors.DarkCyan);
	/// <summary>DarkGoldenrod brush.</summary>
	public static SolidColorBrush DarkGoldenrod => new SolidColorBrush(Colors.DarkGoldenrod);
	/// <summary>DarkGray brush.</summary>
	public static SolidColorBrush DarkGray => new SolidColorBrush(Colors.DarkGray);
	/// <summary>DarkGreen brush.</summary>
	public static SolidColorBrush DarkGreen => new SolidColorBrush(Colors.DarkGreen);
	/// <summary>DarkKhaki brush.</summary>
	public static SolidColorBrush DarkKhaki => new SolidColorBrush(Colors.DarkKhaki);
	/// <summary>DarkMagenta brush.</summary>
	public static SolidColorBrush DarkMagenta => new SolidColorBrush(Colors.DarkMagenta);
	/// <summary>DarkOliveGreen brush.</summary>
	public static SolidColorBrush DarkOliveGreen => new SolidColorBrush(Colors.DarkOliveGreen);
	/// <summary>DarkOrange brush.</summary>
	public static SolidColorBrush DarkOrange => new SolidColorBrush(Colors.DarkOrange);
	/// <summary>DarkOrchid brush.</summary>
	public static SolidColorBrush DarkOrchid => new SolidColorBrush(Colors.DarkOrchid);
	/// <summary>DarkRed brush.</summary>
	public static SolidColorBrush DarkRed => new SolidColorBrush(Colors.DarkRed);
	/// <summary>DarkSalmon brush.</summary>
	public static SolidColorBrush DarkSalmon => new SolidColorBrush(Colors.DarkSalmon);
	/// <summary>DarkSeaGreen brush.</summary>
	public static SolidColorBrush DarkSeaGreen => new SolidColorBrush(Colors.DarkSeaGreen);
	/// <summary>DarkSlateBlue brush.</summary>
	public static SolidColorBrush DarkSlateBlue => new SolidColorBrush(Colors.DarkSlateBlue);
	/// <summary>DarkSlateGray brush.</summary>
	public static SolidColorBrush DarkSlateGray => new SolidColorBrush(Colors.DarkSlateGray);
	/// <summary>DarkTurquoise brush.</summary>
	public static SolidColorBrush DarkTurquoise => new SolidColorBrush(Colors.DarkTurquoise);
	/// <summary>DarkViolet brush.</summary>
	public static SolidColorBrush DarkViolet => new SolidColorBrush(Colors.DarkViolet);
	/// <summary>DeepPink brush.</summary>
	public static SolidColorBrush DeepPink => new SolidColorBrush(Colors.DeepPink);
	/// <summary>DeepSkyBlue brush.</summary>
	public static SolidColorBrush DeepSkyBlue => new SolidColorBrush(Colors.DeepSkyBlue);
	/// <summary>DimGray brush.</summary>
	public static SolidColorBrush DimGray => new SolidColorBrush(Colors.DimGray);
	/// <summary>DodgerBlue brush.</summary>
	public static SolidColorBrush DodgerBlue => new SolidColorBrush(Colors.DodgerBlue);
	/// <summary>Firebrick brush.</summary>
	public static SolidColorBrush Firebrick => new SolidColorBrush(Colors.Firebrick);
	/// <summary>FloralWhite brush.</summary>
	public static SolidColorBrush FloralWhite => new SolidColorBrush(Colors.FloralWhite);
	/// <summary>ForestGreen brush.</summary>
	public static SolidColorBrush ForestGreen => new SolidColorBrush(Colors.ForestGreen);
	/// <summary>Fuchsia brush.</summary>
	public static SolidColorBrush Fuchsia => new SolidColorBrush(Colors.Fuchsia);
	/// <summary>Gainsboro brush.</summary>
	public static SolidColorBrush Gainsboro => new SolidColorBrush(Colors.Gainsboro);
	/// <summary>GhostWhite brush.</summary>
	public static SolidColorBrush GhostWhite => new SolidColorBrush(Colors.GhostWhite);
	/// <summary>Gold brush.</summary>
	public static SolidColorBrush Gold => new SolidColorBrush(Colors.Gold);
	/// <summary>Goldenrod brush.</summary>
	public static SolidColorBrush Goldenrod => new SolidColorBrush(Colors.Goldenrod);
	/// <summary>Gray brush.</summary>
	public static SolidColorBrush Gray => new SolidColorBrush(Colors.Gray);
	/// <summary>Green brush.</summary>
	public static SolidColorBrush Green => new SolidColorBrush(Colors.Green);
	/// <summary>GreenYellow brush.</summary>
	public static SolidColorBrush GreenYellow => new SolidColorBrush(Colors.GreenYellow);
	/// <summary>Honeydew brush.</summary>
	public static SolidColorBrush Honeydew => new SolidColorBrush(Colors.Honeydew);
	/// <summary>HotPink brush.</summary>
	public static SolidColorBrush HotPink => new SolidColorBrush(Colors.HotPink);
	/// <summary>IndianRed brush.</summary>
	public static SolidColorBrush IndianRed => new SolidColorBrush(Colors.IndianRed);
	/// <summary>Indigo brush.</summary>
	public static SolidColorBrush Indigo => new SolidColorBrush(Colors.Indigo);
	/// <summary>Ivory brush.</summary>
	public static SolidColorBrush Ivory => new SolidColorBrush(Colors.Ivory);
	/// <summary>Khaki brush.</summary>
	public static SolidColorBrush Khaki => new SolidColorBrush(Colors.Khaki);
	/// <summary>Lavender brush.</summary>
	public static SolidColorBrush Lavender => new SolidColorBrush(Colors.Lavender);
	/// <summary>LavenderBlush brush.</summary>
	public static SolidColorBrush LavenderBlush => new SolidColorBrush(Colors.LavenderBlush);
	/// <summary>LawnGreen brush.</summary>
	public static SolidColorBrush LawnGreen => new SolidColorBrush(Colors.LawnGreen);
	/// <summary>LemonChiffon brush.</summary>
	public static SolidColorBrush LemonChiffon => new SolidColorBrush(Colors.LemonChiffon);
	/// <summary>LightBlue brush.</summary>
	public static SolidColorBrush LightBlue => new SolidColorBrush(Colors.LightBlue);
	/// <summary>LightCoral brush.</summary>
	public static SolidColorBrush LightCoral => new SolidColorBrush(Colors.LightCoral);
	/// <summary>LightCyan brush.</summary>
	public static SolidColorBrush LightCyan => new SolidColorBrush(Colors.LightCyan);
	/// <summary>LightGoldenrodYellow brush.</summary>
	public static SolidColorBrush LightGoldenrodYellow => new SolidColorBrush(Colors.LightGoldenrodYellow);
	/// <summary>LightGray brush.</summary>
	public static SolidColorBrush LightGray => new SolidColorBrush(Colors.LightGray);
	/// <summary>LightGreen brush.</summary>
	public static SolidColorBrush LightGreen => new SolidColorBrush(Colors.LightGreen);
	/// <summary>LightPink brush.</summary>
	public static SolidColorBrush LightPink => new SolidColorBrush(Colors.LightPink);
}
