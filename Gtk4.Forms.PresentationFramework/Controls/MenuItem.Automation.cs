using System.Linq;
using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class MenuItem
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new MenuItemAutomationPeer(this);

	internal bool HasSubItems => Items.OfType<MenuItem>().Any();

	internal void AutomationActivate() => Activate();

	// A drop-down item shows its sub-items in the popover of its menu button.
	internal bool IsSubmenuShown => _gtkMenuButton?.Popover?.Visible == true;

	internal void AutomationSetSubmenuShown(bool shown)
	{
		if (_gtkMenuButton?.Popover is not { } popover) return;
		if (shown) popover.Popup();
		else popover.Popdown();
	}

	void RaiseAutomationInvoked()
	{
		if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
			GetAutomationPeer()?.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
	}
}
