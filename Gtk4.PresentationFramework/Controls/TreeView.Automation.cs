using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class TreeView
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new TreeViewAutomationPeer(this);
}
