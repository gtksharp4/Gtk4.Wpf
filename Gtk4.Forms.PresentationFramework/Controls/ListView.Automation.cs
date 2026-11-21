using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class ListView
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new ListViewAutomationPeer(this);
}
