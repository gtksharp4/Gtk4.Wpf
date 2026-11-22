namespace System.Windows.Shell;

/// <summary>The edges of a window on which the system draws a non-client frame.</summary>
[Flags]
public enum NonClientFrameEdges
{
	/// <summary>No frame edges.</summary>
	None = 0,

	/// <summary>The left edge.</summary>
	Left = 1,

	/// <summary>The top edge.</summary>
	Top = 2,

	/// <summary>The right edge.</summary>
	Right = 4,

	/// <summary>The bottom edge.</summary>
	Bottom = 8,
}
