namespace System.Windows;

/// <summary>
/// Specifies the display state of an element (WPF-compatible).
/// </summary>
public enum Visibility
{
	/// <summary>Element is visible and takes up space in layout.</summary>
	Visible = 0,
	/// <summary>Element is invisible but still takes up space in layout.</summary>
	Hidden = 1,
	/// <summary>Element is invisible and does not take up space in layout.</summary>
	Collapsed = 2
}