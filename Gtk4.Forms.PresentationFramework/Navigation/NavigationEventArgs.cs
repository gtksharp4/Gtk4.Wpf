using System;
using System.Net;

namespace System.Windows.Navigation;

/// <summary>Arguments for <c>Navigated</c> and <c>LoadCompleted</c> (WPF's <c>NavigationEventArgs</c>).</summary>
public class NavigationEventArgs : EventArgs
{
	internal NavigationEventArgs(
		Uri? uri, object? content, object? extraData, object? navigator, bool isNavigationInitiator)
	{
		Uri = uri;
		Content = content;
		ExtraData = extraData;
		Navigator = navigator;
		IsNavigationInitiator = isNavigationInitiator;
	}

	/// <summary>The content that was navigated to.</summary>
	public object? Content { get; }

	/// <summary>The URI that was navigated to, or <c>null</c> for object-content navigations.</summary>
	public Uri? Uri { get; }

	/// <summary>The extra data passed to <c>Navigate</c>.</summary>
	public object? ExtraData { get; }

	/// <summary>The navigator (<see cref="NavigationService"/>, frame or window) that navigated.</summary>
	public object? Navigator { get; }

	/// <summary>Always <c>null</c>: navigation never goes through a web request here.</summary>
	public WebResponse? WebResponse => null;

	/// <summary>Whether the navigator that raised the event also started the navigation.</summary>
	public bool IsNavigationInitiator { get; }
}
