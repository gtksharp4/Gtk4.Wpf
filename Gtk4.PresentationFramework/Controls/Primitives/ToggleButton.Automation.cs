using System.Windows.Automation.Peers;

namespace System.Windows.Controls.Primitives;

public partial class ToggleButton
{
	internal void AutomationToggle() => OnToggle();

	protected override AutomationPeer? OnCreateAutomationPeer() => new ToggleButtonAutomationPeer(this);
}