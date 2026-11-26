using System.Collections.Generic;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Child lists for peers whose children are the containers of an items control.</summary>
static class PeerTree
{
	/// <summary>The peers of the containers of <paramref name="control"/>; null when there are none.</summary>
	internal static List<AutomationPeer>? Containers(ItemsControl control)
	{
		_ = control.Widget; // the containers exist once the widget does
		var result = new List<AutomationPeer>();
		for (int i = 0; i < control.Items.Count; i++)
			if (control.ContainerAt(i) is { } container)
				AddPeerOf(container, result);
		return result.Count == 0 ? null : result;
	}

	/// <summary>Adds the peer of <paramref name="element"/>, or those of its descendants when it has none.</summary>
	internal static void AddPeerOf(UIElement element, List<AutomationPeer> into)
	{
		if (element is ContentPresenter { Content: UIElement hosted })
		{
			AddPeerOf(hosted, into);
			return;
		}
		var peer = element.GetAutomationPeer();
		if (peer != null) into.Add(peer);
		else UIElementAutomationPeer.Collect(element, into);
	}
}
