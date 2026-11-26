namespace System.Windows;

/// <summary>Specifies the border and title-bar style for a <see cref="Window"/>.</summary>
public enum WindowStyle
{
	/// <summary>
	/// No title bar or border. Maps to <c>Gtk.Window.Decorated = false</c>.
	/// </summary>
	None,
	/// <summary>
	/// A single-line border and title bar (the default WPF style).
	/// Maps to <c>Gtk.Window.Decorated = true</c>.
	/// GTK4 limitation: GTK4 does not distinguish between SingleBorderWindow and
	/// ThreeDBorderWindow — both render as the native platform decoration.
	/// </summary>
	SingleBorderWindow,
	/// <summary>
	/// A three-dimensional border and title bar.
	/// Maps to <c>Gtk.Window.Decorated = true</c> (same as
	/// <see cref="SingleBorderWindow"/> — GTK4 uses native decoration only).
	/// </summary>
	ThreeDBorderWindow,
	/// <summary>
	/// A thin tool-window border and title bar (smaller than a normal window).
	/// Maps to <c>Gtk.Window.Decorated = true</c>. GTK4 does not expose a reduced
	/// decoration size, so this is visually identical to <see cref="SingleBorderWindow"/>.
	/// </summary>
	ToolWindow,
}
