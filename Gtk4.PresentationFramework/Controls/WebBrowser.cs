using System;
using System.Windows.Navigation;

namespace System.Windows.Controls;

/// <summary>
/// WPF-compatible web-browser control backed by a <see cref="IWebBrowserHost"/>.
/// </summary>
/// <remarks>
/// <b>Windowing:</b> the backing GTK widget is a <see cref="Gtk.Box"/> containing a
/// placeholder <see cref="Gtk.Label"/> that shows the current source URI.  Web content
/// renders in Photino's own native top-level window (not embedded in the GTK tree).
/// True in-widget embedding is a future refinement.
///
/// <b>InvokeScript:</b> routes to <see cref="IWebBrowserHost.ExecuteScript"/>.  Through
/// the default Photino host this sends a web message and returns <c>null</c> immediately
/// (Photino has no synchronous JS-eval API).  Inject a custom host to get real return values.
///
/// <b>Navigate overload with extra args:</b> the <c>targetFrameName</c>, <c>postData</c>
/// and <c>additionalHeaders</c> parameters are accepted for WPF source compatibility but
/// ignored; only the URI is acted upon.
/// </remarks>
public partial class WebBrowser : Control, IDisposable
{
	// ── Host factory (injectable seam) ────────────────────────────────────────

	/// <summary>
	/// Factory used to create the <see cref="IWebBrowserHost"/> for each
	/// <see cref="WebBrowser"/> instance.  Replace this in tests to inject a fake.
	/// <para>
	/// Default: a visible CefSharp browser on Windows (the net10.0-windows package variant),
	/// Photino on every other platform/variant.
	/// </para>
	/// </summary>
	public static Func<IWebBrowserHost> HostFactory { get; set; } = CreateDefaultHost;

	// True only in the Windows package variant, where the CefSharp backend is compiled in.
	internal const bool CefSharpCompiledIn =
#if WINDOWS
		true;
#else
		false;
#endif

	/// <summary>
	/// Pure backend-selection decision: use CefSharp when running on Windows <i>and</i> the CefSharp
	/// backend is compiled into this build; use Photino otherwise. Kept separate so it is testable
	/// without instantiating a native host.
	/// </summary>
	internal static bool ShouldUseCefSharp(bool isWindows, bool cefSharpCompiledIn)
		=> isWindows && cefSharpCompiledIn;

	static IWebBrowserHost CreateDefaultHost()
	{
		if (ShouldUseCefSharp(OperatingSystem.IsWindows(), CefSharpCompiledIn))
		{
#if WINDOWS
			return new CefSharpWebBrowserHost();
#endif
		}
		return new PhotinoWebBrowserHost();
	}

	IWebBrowserHost? _host;

	IWebBrowserHost Host
	{
		get
		{
			if (_host == null)
			{
				_host = HostFactory();
				_host.LoadCompleted += OnHostLoadCompleted;
			}
			return _host;
		}
	}

	// ── Source DP ─────────────────────────────────────────────────────────────

	/// <summary>URI of the currently loaded document.</summary>
	public static readonly DependencyProperty SourceProperty =
		DependencyProperty.Register(
			nameof(Source),
			typeof(Uri),
			typeof(WebBrowser),
			new PropertyMetadata(null, OnSourceDpChanged));

	static void OnSourceDpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		// Guard against re-entry from NavigateCore writing back to Source.
		var wb = (WebBrowser)d;
		if (wb._suppressSourceCallback) return;
		if (e.NewValue is Uri uri)
			wb.Navigate(uri);
	}

	bool _suppressSourceCallback;

	/// <summary>Gets or sets the current source URI.  Setting triggers navigation.</summary>
	public Uri? Source
	{
		get => (Uri?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	// ── CanGoBack / CanGoForward (read-only DPs) ──────────────────────────────

	internal static readonly DependencyPropertyKey CanGoBackPropertyKey =
		DependencyProperty.RegisterReadOnly(
			nameof(CanGoBack), typeof(bool), typeof(WebBrowser),
			new PropertyMetadata(false));

	/// <summary>Read-only DP — true when back-navigation is possible.</summary>
	public static readonly DependencyProperty CanGoBackProperty =
		CanGoBackPropertyKey.DependencyProperty;

	/// <summary>True when <see cref="GoBack"/> is possible.</summary>
	public bool CanGoBack => (bool)(GetValue(CanGoBackProperty) ?? false);

	internal static readonly DependencyPropertyKey CanGoForwardPropertyKey =
		DependencyProperty.RegisterReadOnly(
			nameof(CanGoForward), typeof(bool), typeof(WebBrowser),
			new PropertyMetadata(false));

	/// <summary>Read-only DP — true when forward-navigation is possible.</summary>
	public static readonly DependencyProperty CanGoForwardProperty =
		CanGoForwardPropertyKey.DependencyProperty;

	/// <summary>True when <see cref="GoForward"/> is possible.</summary>
	public bool CanGoForward => (bool)(GetValue(CanGoForwardProperty) ?? false);

	// ── GTK backing widget ────────────────────────────────────────────────────

	Gtk.Box? _container;
	Gtk.Label? _placeholder;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_container = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		_placeholder = new Gtk.Label("(web content in external window)");
		_container.Append(_placeholder);
		return _container;
	}

	internal void UpdatePlaceholder()
	{
		if (_placeholder == null) return;
		var uri = HistoryCurrent();
		_placeholder.Text = uri != null
			? $"WebBrowser: {uri}"
			: "(web content in external window)";
	}

	// ── Lifecycle ─────────────────────────────────────────────────────────────

	/// <summary>Disposes the host; tears down the native Photino window deterministically.</summary>
	public void Dispose()
	{
		if (_host != null)
		{
			_host.LoadCompleted -= OnHostLoadCompleted;
			_host.Dispose();
			_host = null;
		}
	}
}