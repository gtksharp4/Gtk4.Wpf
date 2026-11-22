namespace System.Windows.Shell;

/// <summary>The direction in which an element inside a window chrome resizes the window.</summary>
public enum ResizeGripDirection
{
	/// <summary>The element is not a resize grip.</summary>
	None = 0,

	/// <summary>Resizes from the top-left corner.</summary>
	TopLeft = 1,

	/// <summary>Resizes from the top edge.</summary>
	Top = 2,

	/// <summary>Resizes from the top-right corner.</summary>
	TopRight = 3,

	/// <summary>Resizes from the right edge.</summary>
	Right = 4,

	/// <summary>Resizes from the bottom-right corner.</summary>
	BottomRight = 5,

	/// <summary>Resizes from the bottom edge.</summary>
	Bottom = 6,

	/// <summary>Resizes from the bottom-left corner.</summary>
	BottomLeft = 7,

	/// <summary>Resizes from the left edge.</summary>
	Left = 8,
}
