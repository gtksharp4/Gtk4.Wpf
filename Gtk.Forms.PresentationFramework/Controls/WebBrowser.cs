using System;
using System.Windows.Navigation;

namespace System.Windows.Controls
{
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
		/// </summary>
		public static Func<IWebBrowserHost> HostFactory { get; set; } =
			() => new PhotinoWebBrowserHost();

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

		// ── Events ────────────────────────────────────────────────────────────────

		/// <summary>Raised before navigation; set <c>Cancel = true</c> to abort.</summary>
		public event EventHandler<NavigatingCancelEventArgs>? Navigating;

		/// <summary>Raised after the host accepts the navigation request.</summary>
		public event EventHandler<NavigatedEventArgs>? Navigated;

		/// <summary>Raised when the page finishes loading.</summary>
		public event EventHandler<NavigatedEventArgs>? LoadCompleted;

		// ── Navigation methods ────────────────────────────────────────────────────

		/// <summary>Navigates to <paramref name="source"/>.</summary>
		public void Navigate(Uri source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			NavigateCore(source);
		}

		/// <summary>
		/// WPF overload with extra parameters accepted for source compatibility.
		/// <paramref name="targetFrameName"/>, <paramref name="postData"/> and
		/// <paramref name="additionalHeaders"/> are ignored; only the URI is acted upon.
		/// </summary>
		public void Navigate(
			Uri source,
			string? targetFrameName,
			byte[]? postData,
			string? additionalHeaders)
		{
			Navigate(source);
		}

		/// <summary>Loads raw <paramref name="html"/>; fires <see cref="LoadCompleted"/>.</summary>
		public void NavigateToString(string html)
		{
			if (html == null) throw new ArgumentNullException(nameof(html));
			Host.NavigateToString(html);
			// Best-effort: fire LoadCompleted immediately (no URI for string navigations).
			LoadCompleted?.Invoke(this, new NavigatedEventArgs(null, null));
		}

		/// <summary>Navigates back; does nothing when <see cref="CanGoBack"/> is false.</summary>
		public void GoBack()
		{
			if (!CanGoBack) return;
			var uri = HistoryGoBack();
			Host.GoBack();
			CommitNavigation(uri, fromJournal: true);
		}

		/// <summary>Navigates forward; does nothing when <see cref="CanGoForward"/> is false.</summary>
		public void GoForward()
		{
			if (!CanGoForward) return;
			var uri = HistoryGoForward();
			Host.GoForward();
			CommitNavigation(uri, fromJournal: true);
		}

		/// <summary>Reloads the current source URI.</summary>
		public void Refresh()
		{
			var uri = HistoryCurrent();
			if (uri == null) return;
			Host.Refresh();
			// Re-fire events for the same URI.
			CommitNavigation(uri, fromJournal: true);
		}

		/// <summary>
		/// Evaluates <paramref name="scriptName"/> in the current page.
		/// Routes to <see cref="IWebBrowserHost.ExecuteScript"/>; through the default
		/// Photino host this returns <c>null</c> (see <see cref="IWebBrowserHost"/>).
		/// </summary>
		public object? InvokeScript(string scriptName, params object[] args)
		{
			var script = args.Length == 0
				? scriptName
				: $"{scriptName}({string.Join(",", args)})";
			return Host.ExecuteScript(script);
		}

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

		void UpdatePlaceholder()
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

		// ── Internal helpers ──────────────────────────────────────────────────────

		void NavigateCore(Uri uri)
		{
			var args = new NavigatingCancelEventArgs(null, uri, NavigationMode.New);
			Navigating?.Invoke(this, args);
			if (args.Cancel) return;

			HistoryPush(uri);
			Host.Navigate(uri);

			// Write Source DP without re-triggering OnSourceDpChanged.
			_suppressSourceCallback = true;
			try { SetValue(SourceProperty, uri); }
			finally { _suppressSourceCallback = false; }

			UpdatePlaceholder();
			Navigated?.Invoke(this, new NavigatedEventArgs(null, uri));

			// Best-effort immediate LoadCompleted — the host also raises it via
			// OnHostLoadCompleted when the page actually finishes loading.
			LoadCompleted?.Invoke(this, new NavigatedEventArgs(null, uri));
		}

		void CommitNavigation(Uri uri, bool fromJournal)
		{
			_suppressSourceCallback = true;
			try { SetValue(SourceProperty, uri); }
			finally { _suppressSourceCallback = false; }

			UpdatePlaceholder();

			if (!fromJournal)
				return;

			Navigated?.Invoke(this, new NavigatedEventArgs(null, uri));
			LoadCompleted?.Invoke(this, new NavigatedEventArgs(null, uri));
		}

		void OnHostLoadCompleted(Uri? uri)
		{
			// Host signalled page load; fire LoadCompleted with the host-reported URI.
			// NavigateCore already fired it optimistically, but this confirms real completion.
			var effective = uri ?? HistoryCurrent();
			LoadCompleted?.Invoke(this, new NavigatedEventArgs(null, effective));
		}
	}
}
