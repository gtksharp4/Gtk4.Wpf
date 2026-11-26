using System;

namespace System.Windows.Navigation;

/// <summary>
/// Direction of a navigation, mirroring <c>System.Windows.Navigation.NavigationMode</c>.
/// </summary>
public enum NavigationMode
{
	/// <summary>Forward navigation (Navigate overloads).</summary>
	New,
	/// <summary>Back-journal navigation.</summary>
	Back,
	/// <summary>Forward-journal navigation.</summary>
	Forward,
	/// <summary>Page refresh.</summary>
	Refresh,
}

/// <summary>
/// Arguments for the <see cref="NavigationService.Navigating"/> event.
/// Set <see cref="Cancel"/> to <c>true</c> to abort the pending navigation.
/// </summary>
public class NavigatingCancelEventArgs : EventArgs
{
	/// <summary>The content (or Uri) that is about to be navigated to.</summary>
	public object? Content { get; }

	/// <summary>The URI being navigated to, or <c>null</c> for object-content navigations.</summary>
	public Uri? Uri { get; }

	/// <summary>Direction of this navigation.</summary>
	public NavigationMode NavigationMode { get; }

	/// <summary>Set to <c>true</c> to cancel the navigation.</summary>
	public bool Cancel { get; set; }

	public NavigatingCancelEventArgs(object? content, Uri? uri, NavigationMode mode = NavigationMode.New)
	{
		Content = content;
		Uri = uri;
		NavigationMode = mode;
	}
}