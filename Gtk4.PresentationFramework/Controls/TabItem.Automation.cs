using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class TabItem
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new TabItemWrapperAutomationPeer(this);
}
