namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="FrameworkElement"/>.</summary>
public class FrameworkElementAutomationPeer : UIElementAutomationPeer
{
	public FrameworkElementAutomationPeer(FrameworkElement owner) : base(owner) { }

	// AutomationProperties.Name, then the labelling element, as WPF does.
	protected override string GetNameCore()
	{
		var name = base.GetNameCore();
		if (name.Length == 0)
			name = GetLabeledBy()?.GetName() ?? string.Empty;
		return name;
	}

	// AutomationId falls back to the element's x:Name.
	protected override string GetAutomationIdCore()
	{
		var id = base.GetAutomationIdCore();
		return id.Length > 0 ? id : ((FrameworkElement)Owner).Name ?? string.Empty;
	}
}
