using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of an <see cref="Image"/>; it is named only by AutomationProperties.</summary>
public class ImageAutomationPeer : FrameworkElementAutomationPeer
{
	public ImageAutomationPeer(Image owner) : base(owner) { }

	protected override string GetClassNameCore() => "Image";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Image;
}
