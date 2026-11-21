using System.Windows.Automation.Peers;

namespace System.Windows.Controls.Primitives;

public partial class StatusBar
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new StatusBarAutomationPeer(this);
}
