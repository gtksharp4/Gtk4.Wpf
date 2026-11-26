using System;

namespace System.Windows.Navigation;

/// <summary>Handles <c>NavigationFailed</c>.</summary>
public delegate void NavigationFailedEventHandler(object sender, NavigationFailedEventArgs e);

/// <summary>Arguments for <c>NavigationFailed</c> (WPF's <c>NavigationFailedEventArgs</c>).</summary>
public class NavigationFailedEventArgs : EventArgs
{
	internal NavigationFailedEventArgs(Uri? uri, Exception exception, object? extraData, object? navigator)
	{
		Uri = uri;
		Exception = exception;
		ExtraData = extraData;
		Navigator = navigator;
	}

	/// <summary>The URI that could not be navigated to.</summary>
	public Uri? Uri { get; }

	/// <summary>Why the navigation failed.</summary>
	public Exception Exception { get; }

	/// <summary>The extra data passed to <c>Navigate</c>.</summary>
	public object? ExtraData { get; }

	/// <summary>The navigator (frame or window) that failed.</summary>
	public object? Navigator { get; }

	/// <summary>Always <c>null</c>: navigation never goes through a web request here.</summary>
	public System.Net.WebRequest? WebRequest => null;

	/// <summary>Set to true to handle the failure; otherwise the exception is thrown.</summary>
	public bool Handled { get; set; }
}
