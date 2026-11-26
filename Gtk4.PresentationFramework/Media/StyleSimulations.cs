namespace System.Windows.Media;

/// <summary>Specifies the style simulations applied to a glyph typeface.</summary>
[Flags]
public enum StyleSimulations
{
	/// <summary>No simulation.</summary>
	None = 0,

	/// <summary>Italic simulation.</summary>
	ItalicSimulation = 1,

	/// <summary>Bold simulation.</summary>
	BoldSimulation = 2,

	/// <summary>Bold and italic simulation.</summary>
	BoldItalicSimulation = 3,
}
