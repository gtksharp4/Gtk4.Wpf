using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class TabControl
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new TabControlAutomationPeer(this);
}
