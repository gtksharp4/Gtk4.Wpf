using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Windows.Automation.Peers;

public partial class UIElementAutomationPeer
{
	// Reverse link label -> elements it labels, so a label whose widget is created late can push the relation.
	static readonly ConditionalWeakTable<UIElement, List<WeakReference<UIElement>>> _labelled =
		new ConditionalWeakTable<UIElement, List<WeakReference<UIElement>>>();

	// AutomationProperties.LabeledBy -> the GTK labelled-by relation (the label's own accessible node).
	void SyncLabelledBy(Gtk.Widget widget)
	{
		var label = AutomationProperties.GetLabeledBy(Owner);
		if (label != null) Track(label);
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

	void Track(UIElement label)
	{
		var list = _labelled.GetOrCreateValue(label);
		foreach (var item in list)
			if (item.TryGetTarget(out var known) && ReferenceEquals(known, Owner)) return;
		list.Add(new WeakReference<UIElement>(Owner));
	}

	/// <summary>Pushes the labelled-by relation again on the elements <paramref name="label"/> labels.</summary>
	internal static void ResyncLabelledElements(UIElement label)
	{
		if (!_labelled.TryGetValue(label, out var list)) return;
		foreach (var item in list.ToArray())
		{
			if (!item.TryGetTarget(out var element) || !element.HasBackingWidget) continue;
			if (element.GetAutomationPeer() is UIElementAutomationPeer peer)
				peer.SyncLabelledBy(peer.Target(element.Widget));
		}
	}

	internal override void Announce(string message, Gtk.AccessibleAnnouncementPriority priority)
	{
		if (Owner.HasBackingWidget) PeerAccessible.Announce(Target(Owner.Widget), message, priority);
	}
}
