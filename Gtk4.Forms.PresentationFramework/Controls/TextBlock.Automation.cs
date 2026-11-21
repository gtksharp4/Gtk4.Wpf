using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class TextBlock
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new TextBlockAutomationPeer(this);
}