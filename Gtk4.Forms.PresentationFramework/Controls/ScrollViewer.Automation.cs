using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class ScrollViewer
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new ScrollViewerAutomationPeer(this);
}
