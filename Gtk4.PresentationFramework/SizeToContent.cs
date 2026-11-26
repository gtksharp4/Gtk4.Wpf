namespace System.Windows;

/// <summary>
/// Specifies whether a <see cref="Window"/> automatically sizes itself to fit its content.
/// </summary>
public enum SizeToContent
{
	/// <summary>
	/// Explicit <see cref="FrameworkElement.Width"/> and <see cref="FrameworkElement.Height"/>
	/// are used. No automatic sizing.
	/// </summary>
	Manual,
	/// <summary>
	/// Width is sized to content; <see cref="FrameworkElement.Height"/> is explicit (or the
	/// GTK natural height when no Height is set).
	/// </summary>
	Width,
	/// <summary>
	/// Height is sized to content; <see cref="FrameworkElement.Width"/> is explicit (or the
	/// GTK natural width when no Width is set).
	/// </summary>
	Height,
	/// <summary>Both width and height are sized to fit content.</summary>
	WidthAndHeight,
}
