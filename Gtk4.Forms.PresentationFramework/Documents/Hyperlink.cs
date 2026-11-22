using System;

namespace System.Windows.Documents;

/// <summary>
/// An inline element that acts as a hyperlink. Renders as a Pango
/// <c>&lt;a href="..."&gt;</c> when <see cref="NavigateUri"/> is set,
/// or as a plain underlined span otherwise.
/// </summary>
public partial class Hyperlink : Span
{
	/// <summary>The URI the hyperlink navigates to.</summary>
	public Uri? NavigateUri { get; set; }

	/// <summary>Raised when the hyperlink is clicked.</summary>
	public event RoutedEventHandler? Click;

	/// <summary>Raised when navigation is requested.</summary>
	public event RequestNavigateEventHandler? RequestNavigate;

	/// <inheritdoc/>
	internal override void AppendMarkup(System.Text.StringBuilder sb)
	{
		if (NavigateUri != null)
		{
			sb.Append($"<a href=\"{PangoMarkup.Escape(NavigateUri.ToString())}\">");
			base.AppendMarkup(sb);
			sb.Append("</a>");
		}
		else
		{
			sb.Append("<u>");
			base.AppendMarkup(sb);
			sb.Append("</u>");
		}
	}

	/// <summary>Programmatically fires <see cref="Click"/> and <see cref="RequestNavigate"/>.</summary>
	internal void OnClick()
	{
		Click?.Invoke(this, new RoutedEventArgs());
		if (NavigateUri != null)
			RequestNavigate?.Invoke(this, new RequestNavigateEventArgs(NavigateUri, TargetName));
	}
}

/// <summary>Provides data for the <see cref="Hyperlink.RequestNavigate"/> event.</summary>
public class RequestNavigateEventArgs : EventArgs
{
	/// <summary>The URI the user wants to navigate to.</summary>
	public Uri Uri { get; }

	/// <summary>The name of the frame or window that should handle the navigation (null when none given).</summary>
	public string? Target { get; }

	/// <summary>Creates an instance with the given URI and target.</summary>
	public RequestNavigateEventArgs(Uri uri, string? target)
	{
		Uri = uri ?? throw new ArgumentNullException(nameof(uri));
		Target = target;
	}

	/// <summary>Creates an instance with the given URI.</summary>
	public RequestNavigateEventArgs(Uri uri) : this(uri, null)
	{
	}
}

/// <summary>Handler for <see cref="Hyperlink.RequestNavigate"/>.</summary>
public delegate void RequestNavigateEventHandler(object sender, RequestNavigateEventArgs e);