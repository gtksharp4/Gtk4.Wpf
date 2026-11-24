namespace System.Windows.Automation.Peers;

/// <summary>
/// Typed wrappers over <c>Gtk.AccessibleExtensions.Update*</c>. They use the array overloads: the
/// single-value overloads of GtkSharp4 leave the accessible value unchanged.
/// </summary>
static partial class PeerAccessible
{
	/// <summary>Test seam: sees every property push and reset (a reset reports a null value).</summary>
	internal static Action<Gtk.Widget, Gtk.AccessibleProperty, object?>? Spy;

	internal static void SetProperty(Gtk.Widget widget, Gtk.AccessibleProperty property, string value) =>
		Update(widget, property, new GLib.Value(value), value);

	internal static void SetProperty(Gtk.Widget widget, Gtk.AccessibleProperty property, bool value) =>
		Update(widget, property, new GLib.Value(value), value);

	internal static void SetProperty(Gtk.Widget widget, Gtk.AccessibleProperty property, int value) =>
		Update(widget, property, new GLib.Value(value), value);

	internal static void SetProperty(Gtk.Widget widget, Gtk.AccessibleProperty property, double value) =>
		Update(widget, property, new GLib.Value(value), value);

	static void Update(Gtk.Widget widget, Gtk.AccessibleProperty property, GLib.Value value, object boxed)
	{
		Spy?.Invoke(widget, property, boxed);
		Gtk.AccessibleExtensions.UpdateProperty(widget, new[] { property }, new[] { value });
	}

	internal static void SetState(Gtk.Widget widget, Gtk.AccessibleState state, bool value) =>
		UpdateState(widget, state, value ? 1 : 0);

	internal static void SetTristate(Gtk.Widget widget, Gtk.AccessibleState state, bool? value) =>
		UpdateState(widget, state, (int)(value == null ? Gtk.AccessibleTristate.Mixed
			: value.Value ? Gtk.AccessibleTristate.True : Gtk.AccessibleTristate.False));

	// GTK reads boolean and tristate states from an int value; a boolean value is ignored.
	static void UpdateState(Gtk.Widget widget, Gtk.AccessibleState state, int value) =>
		Gtk.AccessibleExtensions.UpdateState(widget, new[] { state }, new[] { new GLib.Value(value) });
}
