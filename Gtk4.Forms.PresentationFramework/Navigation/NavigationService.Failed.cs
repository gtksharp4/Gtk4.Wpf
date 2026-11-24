using System.IO;

namespace System.Windows.Navigation;

public sealed partial class NavigationService
{
	/// <summary>Raised when a navigation to a URI fails; for a missing page, unhandled, the exception is thrown.</summary>
	public event NavigationFailedEventHandler? NavigationFailed;

	/// <summary>
	/// Navigates a link target: content that resolves is shown. A missing page raises
	/// <see cref="NavigationFailed"/> and, unhandled, throws, as WPF does. An external address cannot be hosted
	/// here (WPF shows web content): it raises <see cref="NavigationFailed"/> and never throws. The navigator is
	/// left unchanged either way.
	/// </summary>
	internal bool NavigateLink(Uri uri)
	{
		var content = Resolve(uri);
		if (content != null) return NavigateCore(content, uri, null);

		var external = uri.IsAbsoluteUri && !uri.IsFile && uri.Scheme != "pack";
		Exception error = external
			? new NotSupportedException($"Navigating to external address '{uri}' is not supported.")
			: new FileNotFoundException($"Cannot locate resource '{uri.OriginalString}'.", uri.OriginalString);
		var args = new NavigationFailedEventArgs(uri, error, null, Host);
		NavigationFailed?.Invoke(Host ?? this, args);
		if (!args.Handled && !external) throw error;
		return false;
	}
}
