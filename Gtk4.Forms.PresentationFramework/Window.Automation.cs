using System.Windows.Automation.Peers;

namespace System.Windows;

public partial class Window
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new WindowAutomationPeer(this);
}