using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class Image
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new ImageAutomationPeer(this);
}
