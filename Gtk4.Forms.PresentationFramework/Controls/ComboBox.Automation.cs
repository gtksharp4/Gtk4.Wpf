using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class ComboBox
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new ComboBoxAutomationPeer(this);
}