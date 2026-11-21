using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class Slider
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new SliderAutomationPeer(this);
}