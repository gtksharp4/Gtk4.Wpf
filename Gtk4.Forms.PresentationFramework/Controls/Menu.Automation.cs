using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class Menu
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new MenuAutomationPeer(this);
}
