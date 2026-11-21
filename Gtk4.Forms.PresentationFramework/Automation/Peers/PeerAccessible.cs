namespace System.Windows.Automation.Peers;

/// <summary>
/// Typed wrappers over <c>Gtk.AccessibleExtensions.Update*</c>. They use the array overloads: the
/// single-value overloads of GtkSharp4 leave the accessible value unchanged.
/// </summary>
static partial class PeerAccessible
{
	internal static void SetProperty(Gtk.Widget widget, Gtk.AccessibleProperty property, string value) =>
		Gtk.AccessibleExtensions.UpdateProperty(widget, new[] { property }, new[] { new GLib.Value(value) });

	internal static void SetProperty(Gtk.Widget widget, Gtk.AccessibleProperty property, bool value) =>
		Gtk.AccessibleExtensions.UpdateProperty(widget, new[] { property }, new[] { new GLib.Value(value) });

	internal static void SetProperty(Gtk.Widget widget, Gtk.AccessibleProperty property, int value) =>
		Gtk.AccessibleExtensions.UpdateProperty(widget, new[] { property }, new[] { new GLib.Value(value) });

	internal static void SetProperty(Gtk.Widget widget, Gtk.AccessibleProperty property, double value) =>
		Gtk.AccessibleExtensions.UpdateProperty(widget, new[] { property }, new[] { new GLib.Value(value) });

	internal static void SetState(Gtk.Widget widget, Gtk.AccessibleState state, bool value) =>
		UpdateState(widget, state, value ? 1 : 0);

	internal static void SetTristate(Gtk.Widget widget, Gtk.AccessibleState state, bool? value) =>
		UpdateState(widget, state, (int)(value == null ? Gtk.AccessibleTristate.Mixed
			: value.Value ? Gtk.AccessibleTristate.True : Gtk.AccessibleTristate.False));

	// GTK reads boolean and tristate states from an int value; a boolean value is ignored.
	static void UpdateState(Gtk.Widget widget, Gtk.AccessibleState state, int value) =>
		Gtk.AccessibleExtensions.UpdateState(widget, new[] { state }, new[] { new GLib.Value(value) });
}
