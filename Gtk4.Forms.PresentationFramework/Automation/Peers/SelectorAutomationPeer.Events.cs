using System.Collections;

namespace System.Windows.Automation.Peers;

public abstract partial class SelectorAutomationPeer
{
	// More changed items than this are reported as one Invalidated event, as in WPF.
	const int InvalidatedLimit = 20;

	/// <summary>Raises the selection pattern events for one selection change, as WPF does.</summary>
	internal void RaiseSelectionEvents(IList added, IList removed)
	{
		int selected = 0;
		foreach (var _ in SelectedItemsList()) selected++;
		if (selected == 1 && added.Count == 1)
		{
			if (added[0] is { } item)
				ItemPeerFor(item).RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
		}
		else if (added.Count + removed.Count > InvalidatedLimit)
			RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
		else
		{
			foreach (var item in added)
				if (item != null)
					ItemPeerFor(item).RaiseAutomationEvent(
						AutomationEvents.SelectionItemPatternOnElementAddedToSelection);
			foreach (var item in removed)
				if (item != null)
					ItemPeerFor(item).RaiseAutomationEvent(
						AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection);
		}
	}
}
