namespace System.Windows.Automation;

/// <summary>
/// Pushes <see cref="AutomationProperties"/> values to the GTK 4 accessible API.
/// </summary>
/// <remarks>
/// <b>GTK API used:</b> <c>Gtk.AccessibleExtensions.UpdateProperty(IAccessible, AccessibleProperty[], GLib.Value[])</c>
/// (the single-value overload leaves the value unchanged, see <c>PeerAccessible</c>) and
/// <c>gtk_accessible_reset_property</c> when a value is cleared.
/// <c>Gtk.Widget</c> implements <c>Gtk.IAccessible</c>, so any realized widget is a valid target.
/// <para>
/// <b>Name → <c>Gtk.AccessibleProperty.Label</c></b><br/>
/// <b>HelpText → <c>Gtk.AccessibleProperty.HelpText</c></b>
/// </para>
/// Only explicit values are pushed: a name GTK derives from the widget's content stays with GTK.
/// </remarks>
internal static class AutomationAccessiblePush
{
	/// <summary>
	/// Reads <see cref="AutomationProperties.NameProperty"/> and
	/// <see cref="AutomationProperties.HelpTextProperty"/> from <paramref name="source"/>
	/// and pushes them to <paramref name="widget"/>'s GTK accessible properties. Empty values reset the
	/// GTK property, except at <paramref name="created"/> time, when nothing was pushed yet.
	/// </summary>
	internal static void Push(Gtk.Widget widget, DependencyObject source, bool created = false)
	{
		var peer = (source as UIElement)?.ExistingAutomationPeer as Peers.UIElementAutomationPeer;
		if (peer != null) widget = peer.Target(widget);
		var name = source.GetValue(AutomationProperties.NameProperty) as string;
		var help = source.GetValue(AutomationProperties.HelpTextProperty) as string;
		// A peer also resolves the labelling element's name, so it owns clearing the label.
		if (!string.IsNullOrEmpty(name) || (!created && peer == null))
			Peers.PeerAccessible.SetOrResetProperty(widget, Gtk.AccessibleProperty.Label, name);
		if (!string.IsNullOrEmpty(help) || !created)
			Peers.PeerAccessible.SetOrResetProperty(widget, Gtk.AccessibleProperty.HelpText, help);
	}
}
