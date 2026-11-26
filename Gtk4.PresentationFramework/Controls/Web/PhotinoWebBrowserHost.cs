using System;

namespace System.Windows.Controls;

/// <summary>
/// <see cref="IWebBrowserHost"/> backed by a <c>Photino.NET.PhotinoWindow</c>.
/// </summary>
/// <remarks>
/// The <c>PhotinoWindow</c> is created lazily on the first navigation and only when
/// both a display and the Photino native library are available.  When either is absent
/// (headless CI, missing native DLL) the host degrades to a no-op placeholder so the
/// rest of the <see cref="WebBrowser"/> control still builds and loads without hanging.
///
/// <b>Availability probe:</b> the <c>new PhotinoWindow(…)</c> constructor is the real
/// guard — it is the first P/Invoke that loads <c>Photino.Native</c> and will throw
/// <see cref="DllNotFoundException"/> when the native library is absent.
///
/// <b>JS eval limitation:</b> Photino provides only <c>SendWebMessage</c> (fire-and-
/// forget).  <see cref="ExecuteScript"/> fires the message and immediately returns
/// <c>null</c>; callers that need a return value must register a web-message handler
/// on the Photino window directly (not supported through this seam).
///
/// <b>WebMessageReceived:</b> Photino fires page → host messages via
/// <c>RegisterWebMessageReceivedHandler</c>.  This host raises <see cref="LoadCompleted"/>
/// when a well-known <c>__loaded__</c> web-message arrives; a real app injects a small
/// bootstrap script to send it.  Without the script the event is never raised through
/// this channel; <see cref="WebBrowser"/> fires <see cref="LoadCompleted"/> immediately
/// as a best-effort fallback when the host does not confirm.
/// </remarks>
sealed class PhotinoWebBrowserHost : IWebBrowserHost
{
	/// <summary>Raised when a <c>__loaded__</c> web-message arrives from the page.</summary>
	public event Action<Uri?>? LoadCompleted;

	Photino.NET.PhotinoWindow? _window;
	Uri? _currentUri;
	bool _disposed;

	// ── IWebBrowserHost ───────────────────────────────────────────────────────

	public void Navigate(Uri source)
	{
		if (_disposed) return;
		_currentUri = source;
		EnsureWindow()?.Load(source);
	}

	public void NavigateToString(string html)
	{
		if (_disposed) return;
		EnsureWindow()?.LoadRawString(html);
	}

	/// <inheritdoc/>
	/// <remarks>
	/// Photino has no synchronous JS-eval API; the script is sent as a web message
	/// and <c>null</c> is returned immediately.
	/// </remarks>
	public string? ExecuteScript(string script)
	{
		if (_disposed) return null;
		EnsureWindow()?.SendWebMessage(script);
		return null;
	}

	public void GoBack()
	{
		if (_disposed) return;
		// Photino exposes no history-navigation API; send a JS back-navigation message.
		EnsureWindow()?.SendWebMessage("__goback__");
	}

	public void GoForward()
	{
		if (_disposed) return;
		EnsureWindow()?.SendWebMessage("__goforward__");
	}

	public void Refresh()
	{
		if (_disposed) return;
		if (_currentUri != null)
			EnsureWindow()?.Load(_currentUri);
	}

	// ── Lifecycle ─────────────────────────────────────────────────────────────

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		try { _window?.Close(); }
		catch { /* native window may already be gone */ }
		_window = null;
	}

	// ── Helpers ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Returns the existing window or creates one.  Returns <c>null</c> when the
	/// Photino native library is unavailable or no display is accessible.
	/// </summary>
	/// <remarks>
	/// The <c>new PhotinoWindow(…)</c> constructor call is the real native-lib guard:
	/// it triggers the P/Invoke that loads <c>Photino.Native</c> and throws
	/// <see cref="DllNotFoundException"/> if the library is absent.  The earlier
	/// headless check avoids opening a window when no display is configured on Linux.
	/// No other probe is needed — reflection on the managed type never triggers the
	/// native load, so a <c>typeof(PhotinoWindow).FullName</c>-style check is a no-op.
	/// </remarks>
	Photino.NET.PhotinoWindow? EnsureWindow()
	{
		if (_window != null) return _window;

		// On Linux, skip when no display is reachable (avoids a hang inside GDK).
		if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
			    System.Runtime.InteropServices.OSPlatform.Linux))
		{
			var display = Environment.GetEnvironmentVariable("DISPLAY");
			var wayland = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
			if (string.IsNullOrEmpty(display) && string.IsNullOrEmpty(wayland))
				return null;
		}

		try
		{
			// This ctor is the first P/Invoke into Photino.Native; DllNotFoundException
			// here means the native library is absent — degrade to no-op.
			var win = new Photino.NET.PhotinoWindow(null!)
			{
				Title = "WebBrowser",
			};
			win.RegisterWebMessageReceivedHandler(OnWebMessage);
			_window = win;
			return _window;
		}
		catch (Exception)
		{
			// Native lib absent, or window creation failed (e.g. no display on Linux
			// that slipped past the check above).  Degrade to no-op placeholder.
			return null;
		}
	}

	void OnWebMessage(object? sender, string message)
	{
		if (message == "__loaded__")
			LoadCompleted?.Invoke(_currentUri);
	}
}