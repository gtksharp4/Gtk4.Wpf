using System.Windows.Navigation;

namespace System.Windows.Controls;

public partial class Frame
{
	/// <summary>Raised before navigation; the sender is this frame. Set <c>Cancel</c> to abort.</summary>
	public event NavigatingCancelEventHandler Navigating
	{
		add => NavigationService.Navigating += value;
		remove => NavigationService.Navigating -= value;
	}

	/// <summary>Raised when a navigation is cancelled or <see cref="StopLoading"/> is called.</summary>
	public event NavigationStoppedEventHandler NavigationStopped
	{
		add => NavigationService.NavigationStopped += value;
		remove => NavigationService.NavigationStopped -= value;
	}

	/// <summary>Reloads the current content; see <see cref="NavigationService.Refresh"/>.</summary>
	public void Refresh() => NavigationService.Refresh();

	/// <summary>Stops the navigation in progress; see <see cref="NavigationService.StopLoading"/>.</summary>
	public void StopLoading() => NavigationService.StopLoading();
}
