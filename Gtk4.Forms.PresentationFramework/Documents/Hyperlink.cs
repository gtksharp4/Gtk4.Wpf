using System;
using System.Windows.Navigation;

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

	/// <summary>Programmatically fires <see cref="Click"/> and <see cref="RequestNavigateEvent"/>.</summary>
	internal void OnClick()
	{
		Click?.Invoke(this, new RoutedEventArgs());
		if (NavigateUri != null)
			RaiseEvent(new RequestNavigateEventArgs(NavigateUri, TargetName));
	}
}
