using System;
using System.Windows.Controls;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// In-process stand-in for <see cref="IWebBrowserHost"/>. No native window, no Photino.
/// </summary>
sealed class FakeWebBrowserHost : IWebBrowserHost
{
	public event Action<Uri?>? LoadCompleted;

	public Uri? LastNavigatedUri { get; private set; }
	public string? LastHtml { get; private set; }
	public string? LastExecutedScript { get; private set; }
	public string? ExecuteScriptResult { get; set; }
	public bool GoBackCalled { get; private set; }
	public bool GoForwardCalled { get; private set; }
	public bool RefreshCalled { get; private set; }
	public bool IsDisposed { get; private set; }

	public void Navigate(Uri source) => LastNavigatedUri = source;
	public void NavigateToString(string html) => LastHtml = html;
	public string? ExecuteScript(string script)
	{
		LastExecutedScript = script;
		return ExecuteScriptResult;
	}
	public void GoBack() => GoBackCalled = true;
	public void GoForward() => GoForwardCalled = true;
	public void Refresh() => RefreshCalled = true;

	/// <summary>Triggers the load-completed callback (simulates page finish).</summary>
	public void SimulateLoadCompleted(Uri? uri) => LoadCompleted?.Invoke(uri);

	public void Dispose() => IsDisposed = true;
}