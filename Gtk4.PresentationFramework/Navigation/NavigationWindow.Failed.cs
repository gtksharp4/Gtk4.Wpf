namespace System.Windows.Navigation;

public partial class NavigationWindow
{
	/// <summary>Raised when a navigation fails; the sender is this window.</summary>
	public event NavigationFailedEventHandler NavigationFailed
	{
		add => NavigationService.NavigationFailed += value;
		remove => NavigationService.NavigationFailed -= value;
	}
}
