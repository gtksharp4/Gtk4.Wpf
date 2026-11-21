using System.Collections;
using System.Windows.Automation.Peers;

namespace System.Windows.Controls.Primitives;

public abstract partial class Selector
{
	// Reports a selection change to listening automation clients.
	internal void OnAutomationSelectionEvents(IList added, IList removed)
	{
		if (!(AutomationPeer.ListenerExists(AutomationEvents.SelectionPatternOnInvalidated)
			|| AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected)
			|| AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection)
			|| AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection)))
			return;
		(GetAutomationPeer() as SelectorAutomationPeer)?.RaiseSelectionEvents(added, removed);
	}
}
