using System;
using Gtk;

namespace Xamarin.Forms.Platform.GTK;

/// <summary>
/// Application-wide GTK theming.
/// </summary>
/// <remarks>
/// The GTK2 implementation probed the Windows registry for a GTK# 2.12 install and
/// called kernel32!SetDllDirectory so the native DLLs could be found; on Linux the
/// loader finds libgtk-4 through the normal search path, so none of that applies.
///
/// GTK3 replaced the RC-file mechanism (<c>Gtk.Rc.Parse</c>) with CSS; GTK4 keeps CSS
/// but drops Gdk.Screen — <see cref="LoadCustomTheme"/> now uses Gdk.Display.Default.
/// </remarks>
public static class GtkThemes
{
	static CssProvider s_customThemeProvider;

	public static bool IsInitialized { get; private set; }

	public static void Init()
	{
		if (IsInitialized)
			return;

		IsInitialized = true;
	}

	/// <summary>
	/// Applies a custom GTK4 CSS theme across the default display of the application,
	/// replacing any theme previously applied through this method.
	/// </summary>
	/// <param name="filename">Path to a GTK4 CSS file.</param>
	public static void LoadCustomTheme(string filename)
	{
		if (string.IsNullOrEmpty(filename))
			return;

		if (!IsInitialized)
			throw new InvalidOperationException("call GtkThemes.Init() before this");

		// GTK4: Gdk.Screen is removed; use Gdk.Display.Default.
		var display = Gdk.Display.Default;

		if (display == null)
			return;

		if (s_customThemeProvider != null)
		{
			StyleContext.RemoveProviderForDisplay(display, s_customThemeProvider);
			s_customThemeProvider = null;
		}

		var provider = new CssProvider();
		provider.LoadFromPath(filename);

		StyleContext.AddProviderForDisplay(display, provider, StyleProviderPriority.Application);
		s_customThemeProvider = provider;
	}
}