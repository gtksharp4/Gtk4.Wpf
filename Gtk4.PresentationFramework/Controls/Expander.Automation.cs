using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class Expander
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new ExpanderAutomationPeer(this);
}
