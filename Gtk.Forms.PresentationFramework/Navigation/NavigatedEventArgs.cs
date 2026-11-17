using System;

namespace System.Windows.Navigation;

/// <summary>Arguments for <see cref="NavigationService.Navigated"/>.</summary>
public class NavigatedEventArgs : EventArgs
{
	/// <summary>The content that was navigated to.</summary>
	public object? Content { get; }

	/// <summary>The URI that was navigated to, or <c>null</c> for object-content navigations.</summary>
	public Uri? Uri { get; }

	public NavigatedEventArgs(object? content, Uri? uri)
	{
		Content = content;
		Uri = uri;
	}
}