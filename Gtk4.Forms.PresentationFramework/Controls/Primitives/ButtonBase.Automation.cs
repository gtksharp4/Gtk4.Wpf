using System.Windows.Automation.Peers;

namespace System.Windows.Controls.Primitives;

public abstract partial class ButtonBase
{
	// Clicks as a user would: raises Click (ToggleButton toggles first), then runs the command.
	internal void AutomationClick() => OnClick();

	// Button and RepeatButton report the invoke to listening clients first thing in OnClick, as in WPF.
	internal void RaiseAutomationInvoked()
	{
		if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
			GetAutomationPeer()?.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
	}
}
