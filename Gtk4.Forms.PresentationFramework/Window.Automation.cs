using System.Windows.Automation.Peers;

namespace System.Windows;

public partial class Window
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new WindowAutomationPeer(this);

	/// <summary>Whether the window has closed; its automation elements are then unavailable.</summary>
	internal bool IsClosedWindow => _closed;
}
