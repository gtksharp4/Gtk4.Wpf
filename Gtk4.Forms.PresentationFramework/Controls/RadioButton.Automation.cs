using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class RadioButton
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new RadioButtonAutomationPeer(this);
}