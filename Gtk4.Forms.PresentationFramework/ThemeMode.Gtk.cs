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

	static bool? _hasColorScheme;

	/// <summary>True when GTK has <c>gtk-interface-color-scheme</c> (GTK 4.20+); older GTK throws on lookup.</summary>
	internal static bool HasColorScheme(Gtk.Settings settings)
	{
		if (_hasColorScheme is bool known) return known;
		try
		{
			_ = settings.GtkInterfaceColorScheme;
			_hasColorScheme = true;
		}
		catch (Exception)
		{
			_hasColorScheme = false;
		}
		return _hasColorScheme.Value;
	}

	/// <summary>
	/// Sets <c>gtk-interface-color-scheme</c> (before GTK 4.20: <c>gtk-application-prefer-dark-theme</c>)
	/// when GTK has a display; otherwise does nothing.
	/// </summary>
	internal static void Apply(ThemeMode mode)
	{
		try
		{
			if (Gdk.Display.Default == null) return;
			var settings = Gtk.Settings.Default;
			if (HasColorScheme(settings)) settings.GtkInterfaceColorScheme = ColorScheme(mode);
			else settings.GtkApplicationPreferDarkTheme = mode == ThemeMode.Dark;
		}
		catch (Exception)
		{
			// No usable GTK (headless or not initialised): the mode stays store-only.
		}
	}
}
