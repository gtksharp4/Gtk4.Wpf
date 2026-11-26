namespace System.Windows;

/// <summary>Specifies where a <see cref="Window"/> is positioned when first shown.</summary>
public enum WindowStartupLocation
{
	/// <summary>
	/// The window position is determined by <see cref="Window.Left"/> and
	/// <see cref="Window.Top"/> (the default; OS chooses when neither is set).
	/// </summary>
	Manual,
	/// <summary>The window is centered on the screen that contains the mouse cursor.</summary>
	CenterScreen,
	/// <summary>
	/// The window is centered over its <see cref="Window.Owner"/>; falls back to
	/// <see cref="CenterScreen"/> when there is no owner.
	/// </summary>
	CenterOwner,
}
