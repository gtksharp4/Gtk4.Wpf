namespace System.Windows.Automation.Peers;

public partial class UIElementAutomationPeer
{
	// AutomationProperties.LabeledBy -> the GTK labelled-by relation (the label's own accessible node).
	void SyncLabelledBy(Gtk.Widget widget)
	{
		var label = AutomationProperties.GetLabeledBy(Owner);
		if (label != null && label.HasBackingWidget)
		{
			var target = label.GetAutomationPeer() is UIElementAutomationPeer peer
				? peer.Target(label.Widget) : label.Widget;
			PeerAccessible.SetRelation(widget, Gtk.AccessibleRelation.LabelledBy, target);
			_labelledByPushed = true;
		}
		else if (_labelledByPushed)
		{
			PeerAccessible.ResetRelation(widget, Gtk.AccessibleRelation.LabelledBy);
			_labelledByPushed = false;
		}
	}

	internal override void Announce(string message, Gtk.AccessibleAnnouncementPriority priority)
	{
		if (Owner.HasBackingWidget) PeerAccessible.Announce(Target(Owner.Widget), message, priority);
	}
}
