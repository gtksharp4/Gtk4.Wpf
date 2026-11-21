using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class GroupBox
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new GroupBoxAutomationPeer(this);
}
