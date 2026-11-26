using System;

namespace System.Windows.Controls;

/// <summary>
/// Injectable host that drives actual web navigation for <see cref="WebBrowser"/>.
/// The default implementation uses a <c>PhotinoWindow</c>; a fake is substituted in
/// tests to avoid any native-window or display dependency.
/// </summary>
/// <remarks>
/// <b>Windowing note:</b> <c>Photino.NET</c>'s <c>PhotinoWindow</c> is a native top-level
/// window (WebView2 on Windows, WebKitGTK on Linux, WKWebView on macOS). There is no public
/// API to embed it as a child GTK widget. The <see cref="WebBrowser"/> control therefore
/// renders a placeholder GTK widget in the layout tree; actual web content appears in
/// Photino's own window. True in-widget embedding is a later refinement.
/// </remarks>
public interface IWebBrowserHost : IDisposable
{
	/// <summary>Navigates the web view to <paramref name="source"/>.</summary>
	void Navigate(Uri source);

	/// <summary>Loads raw <paramref name="html"/> into the web view.</summary>
	void NavigateToString(string html);

	/// <summary>
	/// Evaluates <paramref name="script"/> in the current page and returns its result.
	/// <para>
	/// <b>Limitation through Photino:</b> Photino exposes only <c>SendWebMessage</c>
	/// (fire-and-forget) and no synchronous JS-eval endpoint. This method therefore
	/// sends the script as a web message and immediately returns <c>null</c>. Callers
	/// that need a return value must use Photino's message channel
	/// (<c>RegisterWebMessageReceivedHandler</c>) directly.
	/// </para>
	/// </summary>
	string? ExecuteScript(string script);

	/// <summary>Navigates back in the web view's own history.</summary>
	void GoBack();

	/// <summary>Navigates forward in the web view's own history.</summary>
	void GoForward();

	/// <summary>Reloads the current page.</summary>
	void Refresh();

	/// <summary>Raised when a page finishes loading; the argument is the final URI (may be null).</summary>
	event Action<Uri?>? LoadCompleted;
}