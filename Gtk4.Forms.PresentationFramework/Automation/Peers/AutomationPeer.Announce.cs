namespace System.Windows.Automation.Peers;

public abstract partial class AutomationPeer
{
	/// <summary>Hands a message to the screen reader through GTK; peers without a widget say nothing.</summary>
	internal virtual void Announce(string message, Gtk.AccessibleAnnouncementPriority priority) { }

	// A live region's change is read out as the region's name: assertive interrupts, polite waits.
	void AnnounceLiveRegion()
	{
		var setting = GetLiveSetting();
		if (setting == AutomationLiveSetting.Off) return;
		var name = GetName();
		if (name.Length == 0) return;
		(EventsSource ?? this).Announce(name, setting == AutomationLiveSetting.Assertive
			? Gtk.AccessibleAnnouncementPriority.High : Gtk.AccessibleAnnouncementPriority.Medium);
	}
}
