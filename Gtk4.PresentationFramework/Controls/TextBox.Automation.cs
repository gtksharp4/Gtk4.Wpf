using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class TextBox
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new TextBoxAutomationPeer(this);
}