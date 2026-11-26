namespace System.Windows.Automation.Peers;

public partial class UIElementAutomationPeer
{
	protected override string GetAcceleratorKeyCore() => AutomationProperties.GetAcceleratorKey(Owner);

	protected override string GetItemStatusCore() => AutomationProperties.GetItemStatus(Owner);

	protected override AutomationHeadingLevel GetHeadingLevelCore() => AutomationProperties.GetHeadingLevel(Owner);
}
