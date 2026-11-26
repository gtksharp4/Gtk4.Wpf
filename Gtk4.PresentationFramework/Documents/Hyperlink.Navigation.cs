using System.Windows.Navigation;

namespace System.Windows.Documents;

public partial class Hyperlink
{
	// WPF default behaviour: an unhandled RequestNavigate navigates the nearest navigator, or the one TargetName names.
	// A target that cannot be resolved raises NavigationFailed instead of blanking the navigator.
	void NavigateDefault(RequestNavigateEventArgs args)
	{
		var nearest = NavigationService.FindNavigator(this);
		var service = string.IsNullOrEmpty(args.Target)
			? nearest
			: NavigationService.FindTarget(this, args.Target!);
		service?.NavigateLink((nearest ?? service).ResolveRelative(args.Uri));
	}
}
