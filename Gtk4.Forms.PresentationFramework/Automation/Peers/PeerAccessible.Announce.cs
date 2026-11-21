namespace System.Windows.Automation.Peers;

static partial class PeerAccessible
{
	/// <summary>Test seam: sees every announcement before it goes to GTK.</summary>
	internal static Action<string, Gtk.AccessibleAnnouncementPriority>? AnnouncementObserver;

	/// <summary>Has a screen reader read <paramref name="message"/>; a no-op where GTK lacks the call.</summary>
	internal static void Announce(Gtk.Widget widget, string message, Gtk.AccessibleAnnouncementPriority priority)
	{
		AnnouncementObserver?.Invoke(message, priority);
		try { GtkInterop.Announce(widget.Handle, message, (int)priority); }
		catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException) { }
	}

	/// <summary>Points <paramref name="relation"/> of <paramref name="widget"/> at <paramref name="targets"/>.</summary>
	internal static void SetRelation(Gtk.Widget widget, Gtk.AccessibleRelation relation, params Gtk.Widget[] targets)
	{
		var handles = new IntPtr[targets.Length];
		for (int i = 0; i < targets.Length; i++) handles[i] = targets[i].Handle;
		try { GtkInterop.UpdateRelation(widget.Handle, (int)relation, handles); }
		catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException) { }
	}

	/// <summary>Removes an explicit relation.</summary>
	internal static void ResetRelation(Gtk.Widget widget, Gtk.AccessibleRelation relation)
	{
		try { GtkInterop.ResetRelation(widget.Handle, (int)relation); }
		catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException) { }
	}
}
