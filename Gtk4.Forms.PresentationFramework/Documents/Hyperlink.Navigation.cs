using System.Windows.Navigation;

namespace System.Windows.Documents;

public partial class Hyperlink
{
	// WPF default behaviour: an unhandled RequestNavigate navigates the nearest navigator, or the one TargetName names.
	void NavigateDefault(RequestNavigateEventArgs args)
	{
		var nearest = NavigationService.FindNavigator(this);
		var service = string.IsNullOrEmpty(args.Target)
			? nearest
			: NavigationService.FindTarget(this, args.Target!);
		service?.Navigate((nearest ?? service).ResolveRelative(args.Uri));
	}
}
