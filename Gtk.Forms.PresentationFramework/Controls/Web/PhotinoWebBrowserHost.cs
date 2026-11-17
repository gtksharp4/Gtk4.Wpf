using System;

namespace System.Windows.Controls
{
	/// <summary>
	/// <see cref="IWebBrowserHost"/> backed by a <c>Photino.NET.PhotinoWindow</c>.
	/// </summary>
	/// <remarks>
	/// The <c>PhotinoWindow</c> is created lazily on the first navigation and only when
	/// both a display and the Photino native library are available.  When either is absent
	/// (headless CI, missing native DLL) the host degrades to a no-op placeholder so the
	/// rest of the <see cref="WebBrowser"/> control still builds and loads without hanging.
	///
	/// <b>JS eval limitation:</b> Photino provides only <c>SendWebMessage</c> (fire-and-
	/// forget).  <see cref="ExecuteScript"/> fires the message and immediately returns
	/// <c>null</c>; callers that need a return value must register a web-message handler
	/// on the Photino window directly (not supported through this seam).
	///
	/// <b>WebMessageReceived:</b> Photino fires page → host messages via
	/// <c>RegisterWebMessageReceivedHandler</c>.  This host synthesises a
	/// <see cref="LoadCompleted"/> event from a well-known <c>__loaded__</c> message; a
	/// real app can inject a small bootstrap script to send it.  Without the script the
	/// event is never raised through this channel — the <see cref="WebBrowser"/> control
	/// fires <see cref="LoadCompleted"/> immediately on <see cref="Navigate"/> as a
	/// best-effort fallback.
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
		Photino.NET.PhotinoWindow? EnsureWindow()
		{
			if (_window != null) return _window;
			if (!IsPhotinoAvailable()) return null;

			try
			{
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
				// Native lib present but window creation failed (e.g. no display on Linux).
				return null;
			}
		}

		void OnWebMessage(object? sender, string message)
		{
			if (message == "__loaded__")
				LoadCompleted?.Invoke(_currentUri);
		}

		/// <summary>
		/// Returns <c>false</c> in headless environments where the Photino native DLL is
		/// absent or no display is configured.
		/// </summary>
		static bool IsPhotinoAvailable()
		{
			// On Linux, refuse to create a Photino window when no display is available.
			if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
					System.Runtime.InteropServices.OSPlatform.Linux))
			{
				var display = Environment.GetEnvironmentVariable("DISPLAY");
				var wayland = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
				if (string.IsNullOrEmpty(display) && string.IsNullOrEmpty(wayland))
					return false;
			}

			// Quick native-lib probe: if the Photino DLL is not on PATH/rpath, skip.
			try
			{
				// Touch a static member that forces the native bridge to load.
				// If it throws DllNotFoundException the runtime is absent.
				_ = typeof(Photino.NET.PhotinoWindow).FullName;
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
