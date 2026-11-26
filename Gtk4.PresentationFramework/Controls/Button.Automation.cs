using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class Button
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new ButtonAutomationPeer(this);
}