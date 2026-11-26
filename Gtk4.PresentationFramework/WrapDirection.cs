namespace System.Windows;

/// <summary>Which sides of a block a floater may not wrap around.</summary>
public enum WrapDirection
{
	/// <summary>No wrapping restriction.</summary>
	None,
	/// <summary>Content wraps on the left.</summary>
	Left,
	/// <summary>Content wraps on the right.</summary>
	Right,
	/// <summary>Content wraps on both sides.</summary>
	Both,
}
