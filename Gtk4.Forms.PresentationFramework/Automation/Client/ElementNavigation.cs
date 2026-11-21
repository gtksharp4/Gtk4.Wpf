using System.Collections.Generic;
using System.Windows.Automation.Peers;

namespace System.Windows.Automation;

/// <summary>
/// Walks the peer tree. A view condition prunes elements and promotes their children, as in the control and
/// content views of UI Automation. All members run on the GTK thread.
/// </summary>
static class ElementNavigation
{
	internal static List<AutomationPeer> RawChildren(AutomationPeer peer) =>
		peer.GetChildren() ?? new List<AutomationPeer>();

	/// <summary>The raw parent: a top-level window hangs under the root element.</summary>
	internal static AutomationPeer? RawParent(AutomationPeer peer)
	{
		if (peer is DesktopAutomationPeer) return null;
		var parent = peer.GetParent();
		if (parent != null) return parent;
		return peer is WindowAutomationPeer && RawChildren(DesktopAutomationPeer.Instance).Contains(peer)
			? DesktopAutomationPeer.Instance : null;
	}

	internal static bool Matches(Condition filter, AutomationPeer peer) =>
		ReferenceEquals(filter, Condition.TrueCondition) || filter.Matches(new AutomationElement(peer, null));

	internal static List<AutomationPeer> Children(AutomationPeer peer, Condition filter)
	{
		var result = new List<AutomationPeer>();
		Collect(peer, filter, result);
		return result;
	}

	static void Collect(AutomationPeer peer, Condition filter, List<AutomationPeer> into)
	{
		foreach (var child in RawChildren(peer))
			if (Matches(filter, child)) into.Add(child);
			else Collect(child, filter, into);
	}

	internal static AutomationPeer? Parent(AutomationPeer peer, Condition filter)
	{
		for (var parent = RawParent(peer); parent != null; parent = RawParent(parent))
			if (Matches(filter, parent)) return parent;
		return null;
	}

	internal static AutomationPeer? Sibling(AutomationPeer peer, Condition filter, int step)
	{
		// Siblings of a top-level match are those under the root, so the view still has a root.
		var parent = Parent(peer, filter) ?? Top(peer);
		if (parent == null) return null;
		var siblings = Children(parent, filter);
		int at = siblings.IndexOf(peer);
		int index = at + step;
		return at >= 0 && index >= 0 && index < siblings.Count ? siblings[index] : null;
	}

	static AutomationPeer? Top(AutomationPeer peer)
	{
		AutomationPeer? top = null;
		for (var up = RawParent(peer); up != null; up = RawParent(up))
			top = up;
		return top;
	}

	/// <summary>The element itself when it matches, otherwise its nearest matching ancestor.</summary>
	internal static AutomationPeer? Normalize(AutomationPeer peer, Condition filter) =>
		Matches(filter, peer) ? peer : Parent(peer, filter);

	/// <summary>Searches in document order: the element, then its descendants depth first.</summary>
	internal static void Search(AutomationPeer root, TreeScope scope, Condition condition, bool firstOnly,
		List<AutomationPeer> found)
	{
		if ((scope & TreeScope.Element) != 0 && Matches(condition, root))
			found.Add(root);
		if (firstOnly && found.Count > 0) return;
		if ((scope & (TreeScope.Children | TreeScope.Descendants)) != 0)
			SearchBelow(root, (scope & TreeScope.Descendants) != 0, condition, firstOnly, found);
	}

	static bool SearchBelow(AutomationPeer parent, bool deep, Condition condition, bool firstOnly,
		List<AutomationPeer> found)
	{
		foreach (var child in RawChildren(parent))
		{
			if (Matches(condition, child))
			{
				found.Add(child);
				if (firstOnly) return true;
			}
			if (deep && SearchBelow(child, true, condition, firstOnly, found)) return true;
		}
		return false;
	}
}
