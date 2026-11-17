using Gtk;

namespace Xamarin.Forms.Platform.GTK.Helpers;

public class GrabHelper
{
	// GTK4: explicit pointer/keyboard grabs are removed entirely (Gdk.Seat.Grab/Ungrab,
	// Gdk.GrabStatus, Gtk.Grab, and the Gdk.Window surface property on a widget all went
	// away). Modal input capture is now intrinsic to Gtk.Popover. The callers
	// (DatePickerWindow and TimePickerWindow) already render as popup windows; no manual
	// grab is needed or possible.
	public static void GrabWindow(Window window)
	{
		window.GrabFocus();
	}

	public static void RemoveGrab(Window window)
	{
		// No-op: GTK4 has no seat grabs to release.
	}
}