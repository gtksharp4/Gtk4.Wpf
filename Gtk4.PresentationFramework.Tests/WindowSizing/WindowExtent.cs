using System;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.WindowSizing;

static class WindowExtent
{
	/// <summary>
	/// The outer size of the window. Without a compositor (e.g. Xvfb with no window manager) GTK draws a solid
	/// client-side border (CSS class solid-csd) inside the surface, so the Gtk.Window widget is smaller than the
	/// surface WPF Width/Height describe; elsewhere the widget is the window.
	/// </summary>
	public static (int W, int H) Of(WpfWindow win)
	{
		var gtk = (Gtk.Window)win.Widget;
		var surface = gtk.Surface;
		bool solidCsd = Array.IndexOf(gtk.CssClasses ?? Array.Empty<string>(), "solid-csd") >= 0;
		return solidCsd && surface != null ? (surface.Width, surface.Height) : (gtk.Width, gtk.Height);
	}
}
