using System;
using System.Windows.Navigation;

namespace System.Windows.Controls;

public partial class WebBrowser
{
	// ── Events ────────────────────────────────────────────────────────────────

	/// <summary>Raised before navigation; set <c>Cancel = true</c> to abort.</summary>
	public event EventHandler<NavigatingCancelEventArgs>? Navigating;

	/// <summary>Raised after the host accepts the navigation request.</summary>
	public event NavigatedEventHandler? Navigated;

	/// <summary>Raised when the page finishes loading.</summary>
	public event LoadCompletedEventHandler? LoadCompleted;

	// ── B1: single-fire LoadCompleted guard ───────────────────────────────────

	/// <summary>
	/// Set when navigation is dispatched to the host.  Cleared by
	/// <see cref="OnHostLoadCompleted"/> once the host confirms.  When the host
	/// never confirms (e.g. the fake with no SimulateLoadCompleted call), the
	/// control fires <see cref="LoadCompleted"/> once from the navigation method
	/// at dispatch time.
	/// </summary>
	bool _loadPending;

	// ── Navigation methods ────────────────────────────────────────────────────

	/// <summary>Navigates to <paramref name="source"/>.</summary>
	public void Navigate(Uri source)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		NavigateCore(source, NavigationMode.New);
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
		LoadCompleted?.Invoke(this, new NavigationEventArgs(null, null, null, this, true));
	}

	/// <summary>Navigates back; does nothing when <see cref="CanGoBack"/> is false.</summary>
	public void GoBack()
	{
		if (!CanGoBack) return;
		var target = PeekBack();
		var args = new NavigatingCancelEventArgs(null, target, NavigationMode.Back);
		Navigating?.Invoke(this, args);
		if (args.Cancel) return;

		var uri = HistoryGoBack();
		Host.GoBack();
		CommitNavigation(uri, fromJournal: true);
	}

	/// <summary>Navigates forward; does nothing when <see cref="CanGoForward"/> is false.</summary>
	public void GoForward()
	{
		if (!CanGoForward) return;
		var target = PeekForward();
		var args = new NavigatingCancelEventArgs(null, target, NavigationMode.Forward);
		Navigating?.Invoke(this, args);
		if (args.Cancel) return;

		var uri = HistoryGoForward();
		Host.GoForward();
		CommitNavigation(uri, fromJournal: true);
	}

	/// <summary>Reloads the current source URI.</summary>
	public void Refresh()
	{
		var uri = HistoryCurrent();
		if (uri == null) return;
		var args = new NavigatingCancelEventArgs(null, uri, NavigationMode.Refresh);
		Navigating?.Invoke(this, args);
		if (args.Cancel) return;

		Host.Refresh();
		CommitNavigation(uri, fromJournal: true);
	}

	// ── Internal navigation core ──────────────────────────────────────────────

	void NavigateCore(Uri uri, NavigationMode mode)
	{
		var args = new NavigatingCancelEventArgs(null, uri, mode);
		Navigating?.Invoke(this, args);
		if (args.Cancel) return;

		HistoryPush(uri);
		_loadPending = true;
		Host.Navigate(uri);

		// Write Source DP without re-triggering OnSourceDpChanged.
		_suppressSourceCallback = true;
		try { SetValue(SourceProperty, uri); }
		finally { _suppressSourceCallback = false; }

		UpdatePlaceholder();
		Navigated?.Invoke(this, new NavigationEventArgs(uri, null, null, this, true));

		// Fire LoadCompleted only when the host won't confirm via its event.
		// _loadPending stays true until OnHostLoadCompleted fires.
		// For hosts that never signal, fire immediately (exactly once).
		// When the host confirms synchronously, _loadPending is already false here.
		if (_loadPending)
		{
			_loadPending = false;
			LoadCompleted?.Invoke(this, new NavigationEventArgs(uri, null, null, this, true));
		}
	}

	void CommitNavigation(Uri uri, bool fromJournal)
	{
		_suppressSourceCallback = true;
		try { SetValue(SourceProperty, uri); }
		finally { _suppressSourceCallback = false; }

		UpdatePlaceholder();

		if (!fromJournal)
			return;

		_loadPending = true;
		Navigated?.Invoke(this, new NavigationEventArgs(uri, null, null, this, true));

		// Same single-fire logic as NavigateCore.
		if (_loadPending)
		{
			_loadPending = false;
			LoadCompleted?.Invoke(this, new NavigationEventArgs(uri, null, null, this, true));
		}
	}

	void OnHostLoadCompleted(Uri? uri)
	{
		// Host confirmed page load.  If we already fired optimistically
		// (_loadPending == false), do not fire again.
		if (!_loadPending) return;
		_loadPending = false;
		var effective = uri ?? HistoryCurrent();
		LoadCompleted?.Invoke(this, new NavigationEventArgs(effective, null, null, this, true));
	}
}