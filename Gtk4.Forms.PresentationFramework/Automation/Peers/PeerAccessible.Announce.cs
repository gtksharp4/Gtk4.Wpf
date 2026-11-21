namespace System.Windows.Automation.Peers;

static partial class PeerAccessible
{
	/// <summary>Test seam: sees every announcement before it goes to GTK.</summary>
	internal static Action<string, Gtk.AccessibleAnnouncementPriority>? AnnouncementObserver;

	/// <summary>Has a screen reader read <paramref name="message"/>.</summary>
	internal static void Announce(Gtk.Widget widget, string message, Gtk.AccessibleAnnouncementPriority priority)
	{
		AnnouncementObserver?.Invoke(message, priority);
		widget.Announce(message, priority);
	}

	/// <summary>Points <paramref name="relation"/> of <paramref name="widget"/> at the targets.</summary>
	internal static void SetRelation(Gtk.Widget widget, Gtk.AccessibleRelation relation,
		params Gtk.Widget[] targets)
	{
		Gtk.AccessibleExtensions.UpdateRelation(widget, relation, targets);
	}

	/// <summary>Removes an explicit relation.</summary>
	internal static void ResetRelation(Gtk.Widget widget, Gtk.AccessibleRelation relation) =>
		widget.ResetRelation(relation);
}
