using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class ToolBar
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new ToolBarAutomationPeer(this);
}
