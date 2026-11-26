using System.Windows.Automation.Peers;

namespace System.Windows.Input;

public static partial class Keyboard
{
	// Tells automation clients that focus moved, naming the nearest element that has a peer.
	static void RaiseAutomationFocusChanged(System.Windows.UIElement element)
	{
		if (!AutomationPeer.ListenerExists(AutomationEvents.AutomationFocusChanged)) return;
		for (var e = element; e != null; e = e.VisualParent as System.Windows.UIElement)
			if (e.GetAutomationPeer() is { } peer)
			{
				peer.RaiseAutomationEvent(AutomationEvents.AutomationFocusChanged);
				return;
			}
	}
}
