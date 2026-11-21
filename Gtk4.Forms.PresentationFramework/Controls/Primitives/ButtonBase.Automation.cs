using System.Windows.Automation.Peers;

namespace System.Windows.Controls.Primitives;

public abstract partial class ButtonBase
{
	// Clicks as a user would: raises Click (ToggleButton toggles first), then runs the command.
	internal void AutomationClick() => OnClick();

	// WPF reports the invoke to listening clients at the start of OnClick.
	void RaiseAutomationInvoked()
	{
		if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
			GetAutomationPeer()?.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
	}
}
