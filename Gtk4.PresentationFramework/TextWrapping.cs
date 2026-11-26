namespace System.Windows;

/// <summary>Text-wrapping mode.</summary>
public enum TextWrapping
{
	/// <summary>Wraps between words only; a long word overflows.</summary>
	WrapWithOverflow = 0,
	/// <summary>No wrapping.</summary>
	NoWrap = 1,
	/// <summary>Wraps at word boundaries, and inside a word that does not fit.</summary>
	Wrap = 2,
}
