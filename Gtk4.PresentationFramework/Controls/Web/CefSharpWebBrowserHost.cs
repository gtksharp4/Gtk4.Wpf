#if WINDOWS
using System;
using System.Threading;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace System.Windows.Controls;

/// <summary>
/// Windows-only <see cref="IWebBrowserHost"/> backed by a visible CefSharp.WinForms
/// <c>ChromiumWebBrowser</c>. Selected automatically by <see cref="WebBrowser"/> when running on
/// Windows (the net10.0-windows package variant); other platforms use
/// <see cref="PhotinoWebBrowserHost"/>.
/// </summary>
/// <remarks>
/// A GTK app pumps the GLib loop, not a WinForms message loop, so the Chromium control is hosted on
/// a dedicated STA thread running <see cref="Application.Run(Form)"/>. All browser calls marshal to
/// that thread via <see cref="Control.BeginInvoke(Delegate)"/>; <see cref="ExecuteScript"/> returns
/// a real value (CefSharp supports synchronous-style JS eval, unlike Photino). If CEF fails to
/// initialise (e.g. missing native binaries) the host degrades to a no-op.
/// </remarks>
sealed class CefSharpWebBrowserHost : IWebBrowserHost
{
	public event Action<Uri?>? LoadCompleted;

	static readonly object _cefLock = new();

	Form? _form;
	ChromiumWebBrowser? _browser;
	Uri? _currentUri;
	bool _disposed;
	readonly ManualResetEventSlim _ready = new(false);

	public CefSharpWebBrowserHost()
	{
		var thread = new Thread(RunUiThread) { IsBackground = true, Name = "CefSharp WebBrowser" };
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		// Wait for the form + browser to be created; bounded so a broken CEF install cannot hang.
		_ready.Wait(TimeSpan.FromSeconds(15));
	}

	void RunUiThread()
	{
		try
		{
			EnsureCefInitialized();
			_browser = new ChromiumWebBrowser(string.Empty) { Dock = DockStyle.Fill };
			_browser.LoadingStateChanged += (_, e) =>
			{
				if (!e.IsLoading)
					LoadCompleted?.Invoke(_currentUri);
			};
			_form = new Form { Text = "WebBrowser", Width = 1024, Height = 768 };
			_form.Controls.Add(_browser);
			_form.Shown += (_, _) => _ready.Set();
			// Fully qualified: System.Windows.Application (the WPF shim) also has a Run method.
			System.Windows.Forms.Application.Run(_form); // visible window + message loop for this host's lifetime
		}
		catch
		{
			// CEF native missing / init failure: degrade to a no-op so the control still loads.
			_ready.Set();
		}
	}

	static void EnsureCefInitialized()
	{
		lock (_cefLock)
		{
			if (Cef.IsInitialized == true)
				return;
			Cef.Initialize(new CefSettings());
		}
	}

	// ── IWebBrowserHost ───────────────────────────────────────────────────────

	public void Navigate(Uri source)
	{
		if (_disposed) return;
		_currentUri = source;
		OnUi(() => _browser?.Load(source.ToString()));
	}

	public void NavigateToString(string html)
	{
		if (_disposed) return;
		OnUi(() => _browser?.LoadHtml(html));
	}

	public string? ExecuteScript(string script)
	{
		if (_disposed || _browser == null) return null;
		try
		{
			var task = _browser.EvaluateScriptAsync(script);
			if (task.Wait(TimeSpan.FromSeconds(5)) && task.Result.Success)
				return task.Result.Result?.ToString();
		}
		catch
		{
			// Eval failed (navigation in progress, bad script, browser gone) — return null.
		}
		return null;
	}

	public void GoBack() => OnUi(() => { if (_browser?.CanGoBack == true) _browser.Back(); });
	public void GoForward() => OnUi(() => { if (_browser?.CanGoForward == true) _browser.Forward(); });
	public void Refresh() => OnUi(() => _browser?.Reload());

	// ── Lifecycle ─────────────────────────────────────────────────────────────

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;

		var form = _form;
		if (form != null && !form.IsDisposed)
		{
			try { form.BeginInvoke(new Action(form.Close)); } // ends Application.Run on the STA thread
			catch { /* thread/window already gone */ }
		}
		_ready.Dispose();
	}

	void OnUi(Action action)
	{
		if (_disposed) return;
		var form = _form;
		if (form == null || form.IsDisposed) return;
		try
		{
			if (form.InvokeRequired)
				form.BeginInvoke(action);
			else
				action();
		}
		catch
		{
			// Window handle not created yet or already destroyed.
		}
	}
}
#endif
