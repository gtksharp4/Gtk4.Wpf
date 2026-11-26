namespace System.Windows;

/// <summary>How the height of a line box is determined.</summary>
public enum LineStackingStrategy
{
	/// <summary>The line height is the maximum of the content's heights.</summary>
	MaxHeight,
	/// <summary>The line height is <c>LineHeight</c>.</summary>
	BlockLineHeight,
}
