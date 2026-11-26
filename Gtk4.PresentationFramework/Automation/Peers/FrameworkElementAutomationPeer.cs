namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="FrameworkElement"/>.</summary>
public class FrameworkElementAutomationPeer : UIElementAutomationPeer
{
	public FrameworkElementAutomationPeer(FrameworkElement owner) : base(owner) { }

	// AutomationProperties.Name, the labelling element, the element's own content, then a string ToolTip.
	protected override string GetNameCore()
	{
		var name = base.GetNameCore();
		if (name.Length == 0)
			name = GetLabeledBy()?.GetName() ?? string.Empty;
		if (name.Length == 0)
			name = ContentName();
		if (name.Length == 0)
			name = Owner.GetValue(FrameworkElement.ToolTipProperty) as string ?? string.Empty;
		return name;
	}

	/// <summary>The name the element's own content gives it: a button's text, a window's title.</summary>
	internal virtual string ContentName() => string.Empty;

	// AutomationId falls back to the element's x:Name.
	protected override string GetAutomationIdCore()
	{
		var id = base.GetAutomationIdCore();
		return id.Length > 0 ? id : ((FrameworkElement)Owner).Name ?? string.Empty;
	}
}
