namespace System.Windows.Automation;

/// <summary>
/// Pushes <see cref="AutomationProperties"/> values to the GTK 4 accessible API.
/// </summary>
/// <remarks>
/// <b>GTK API used:</b> <c>Gtk.AccessibleExtensions.UpdateProperty(IAccessible, AccessibleProperty[], GLib.Value[])</c>\n/// (the single-value overload leaves the value unchanged, see <c>PeerAccessible</c>).
/// <c>Gtk.Widget</c> implements <c>Gtk.IAccessible</c>, so any realized widget is a valid target.
/// <para>
/// <b>Name → <c>Gtk.AccessibleProperty.Label</c></b><br/>
/// <b>HelpText → <c>Gtk.AccessibleProperty.HelpText</c></b>
/// </para>
/// </remarks>
internal static class AutomationAccessiblePush
{
	/// <summary>
	/// Reads <see cref="AutomationProperties.NameProperty"/> and
	/// <see cref="AutomationProperties.HelpTextProperty"/> from <paramref name="source"/>
	/// and pushes them to <paramref name="widget"/>'s GTK accessible properties.
	/// </summary>
	internal static void Push(Gtk.Widget widget, DependencyObject source)
	{
		PushLabel(widget, source);
		PushHelpText(widget, source);
	}

	static void PushLabel(Gtk.Widget widget, DependencyObject source)
	{
		string? name = source.GetValue(AutomationProperties.NameProperty) as string;
		if (name == null) return;
		Peers.PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.Label, name);
	}

	static void PushHelpText(Gtk.Widget widget, DependencyObject source)
	{
		string? help = source.GetValue(AutomationProperties.HelpTextProperty) as string;
		if (help == null) return;
		Peers.PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.HelpText, help);
	}
}
