using System.Windows.Documents;

namespace System.Windows.Navigation;

/// <summary>Provides data for the routed <see cref="Hyperlink.RequestNavigateEvent"/>.</summary>
public class RequestNavigateEventArgs : RoutedEventArgs
{
	/// <summary>Creates an instance for derived classes.</summary>
	protected RequestNavigateEventArgs()
	{
		RoutedEvent = Hyperlink.RequestNavigateEvent;
	}

	/// <summary>Creates an instance with the given URI and target.</summary>
	public RequestNavigateEventArgs(Uri uri, string? target)
	{
		Uri = uri;
		Target = target;
		RoutedEvent = Hyperlink.RequestNavigateEvent;
	}

	/// <summary>The URI the user wants to navigate to.</summary>
	public Uri Uri { get; } = null!;

	/// <summary>The name of the frame or window that should handle the navigation (null when none given).</summary>
	public string? Target { get; }
}

/// <summary>Handler for <see cref="Hyperlink.RequestNavigateEvent"/>.</summary>
public delegate void RequestNavigateEventHandler(object sender, RequestNavigateEventArgs e);
