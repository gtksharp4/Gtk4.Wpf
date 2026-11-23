#pragma warning disable WPF0001 // layer-internal use of the experimental ThemeMode
namespace System.Windows;

/// <summary>Maps a <see cref="ThemeMode"/> onto the GTK 4 interface colour scheme.</summary>
static class ThemeModeGtk
{
	internal static bool IsValid(ThemeMode mode) =>
		mode == ThemeMode.None || mode == ThemeMode.System || mode == ThemeMode.Light || mode == ThemeMode.Dark;

	internal static Gtk.InterfaceColorScheme ColorScheme(ThemeMode mode) =>
		mode == ThemeMode.Dark ? Gtk.InterfaceColorScheme.Dark
		: mode == ThemeMode.Light ? Gtk.InterfaceColorScheme.Light
		: Gtk.InterfaceColorScheme.Default;

	/// <summary>Sets <c>gtk-interface-color-scheme</c> when GTK has a display; otherwise does nothing.</summary>
	internal static void Apply(ThemeMode mode)
	{
		try
		{
			if (Gdk.Display.Default == null) return;
			Gtk.Settings.Default.GtkInterfaceColorScheme = ColorScheme(mode);
		}
		catch (Exception)
		{
			// No usable GTK (headless or not initialised): the mode stays store-only.
		}
	}
}
