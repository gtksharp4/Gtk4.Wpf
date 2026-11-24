using System.IO;

namespace System.Windows.Navigation;

public sealed partial class NavigationService
{
	/// <summary>Raised when a navigation to a URI fails; set <c>Handled</c>, or the exception is thrown.</summary>
	public event NavigationFailedEventHandler? NavigationFailed;

	/// <summary>
	/// Navigates a link target: content that resolves is shown; an external or missing one raises
	/// <see cref="NavigationFailed"/> and, unhandled, throws, as WPF does. The navigator is left unchanged.
	/// </summary>
	internal bool NavigateLink(Uri uri)
	{
		var content = Resolve(uri);
		if (content != null) return NavigateCore(content, uri, null);

		Exception error = uri.IsAbsoluteUri && !uri.IsFile && uri.Scheme != "pack"
			? new NotSupportedException($"Navigating to external address '{uri}' is not supported.")
			: new FileNotFoundException($"Cannot locate resource '{uri.OriginalString}'.", uri.OriginalString);
		var args = new NavigationFailedEventArgs(uri, error, null, Host);
		NavigationFailed?.Invoke(Host ?? this, args);
		if (!args.Handled) throw error;
		return false;
	}
}
