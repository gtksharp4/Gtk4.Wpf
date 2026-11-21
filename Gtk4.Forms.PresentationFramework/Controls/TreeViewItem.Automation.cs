using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class TreeViewItem
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new TreeViewItemAutomationPeer(this);
}
