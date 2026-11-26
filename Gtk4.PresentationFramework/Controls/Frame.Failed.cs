using System.Windows.Navigation;

namespace System.Windows.Controls;

public partial class Frame
{
	/// <summary>Raised when a navigation fails; the sender is this frame.</summary>
	public event NavigationFailedEventHandler NavigationFailed
	{
		add => NavigationService.NavigationFailed += value;
		remove => NavigationService.NavigationFailed -= value;
	}
}
