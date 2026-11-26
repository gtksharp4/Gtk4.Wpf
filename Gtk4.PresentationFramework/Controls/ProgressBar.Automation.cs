using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class ProgressBar
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new ProgressBarAutomationPeer(this);
}
