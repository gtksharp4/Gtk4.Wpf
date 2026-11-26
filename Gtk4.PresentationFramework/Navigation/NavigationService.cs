using System;
using System.Windows.Controls;

namespace System.Windows.Navigation;

/// <summary>
/// Navigation service: back/forward journal, <see cref="Navigate"/> overloads, and
/// <see cref="Navigating"/>/<see cref="Navigated"/>/<see cref="NavigationStopped"/> events.
/// </summary>
/// <remarks>
/// URI navigation is resolved via <see cref="Resolver"/> (default: <see cref="DictionaryContentResolver"/>).
/// Register page instances with <see cref="RegisterContent"/>; compiled pages are produced by the markup
/// compiler's <c>InitializeComponent</c>.
/// </remarks>
public sealed partial class NavigationService
{
	readonly NavigationJournal _journal = new NavigationJournal();

	// ── Resolver ─────────────────────────────────────────────────────────────

	/// <summary>Resolves a URI to a navigation-target object.</summary>
	public IContentResolver Resolver { get; set; } = new DictionaryContentResolver();

	/// <summary>
	/// Registers <paramref name="content"/> so <see cref="Navigate(Uri)"/> can resolve
	/// <paramref name="uri"/> to it. Shortcut for <see cref="DictionaryContentResolver.Registrations"/>.
	/// </summary>
	public void RegisterContent(Uri uri, object content)
	{
		if (Resolver is DictionaryContentResolver dr)
			dr.Registrations[uri] = content;
		else
			throw new InvalidOperationException(
				"Resolver is not a DictionaryContentResolver; register directly on your custom resolver.");
	}

	// ── Current content ───────────────────────────────────────────────────────

	/// <summary>The currently displayed content.</summary>
	public object? Content { get; private set; }

	// ── Journal ───────────────────────────────────────────────────────────────

	/// <summary>True if there is a previous entry to navigate back to.</summary>
	public bool CanGoBack => _journal.CanGoBack;

	/// <summary>True if there is a next entry to navigate forward to.</summary>
	public bool CanGoForward => _journal.CanGoForward;

	// ── Events ────────────────────────────────────────────────────────────────

	/// <summary>Raised before navigation occurs. Set <c>Cancel</c> to abort.</summary>
	public event NavigatingCancelEventHandler? Navigating;

	/// <summary>Raised after the content changed, before <see cref="LoadCompleted"/>.</summary>
	public event NavigatedEventHandler? Navigated;

	/// <summary>Raised once the navigated content is loaded; follows <see cref="Navigated"/>.</summary>
	public event LoadCompletedEventHandler? LoadCompleted;

	/// <summary>Raised when a navigation is cancelled or <see cref="StopLoading"/> is called.</summary>
	public event NavigationStoppedEventHandler? NavigationStopped;

	// ── Navigate ──────────────────────────────────────────────────────────────

	/// <summary>Navigate to an object (a <see cref="Page"/>, any <see cref="UIElement"/>, or other).</summary>
	/// <returns><c>true</c> if navigation proceeded; <c>false</c> if cancelled.</returns>
	public bool Navigate(object content) => NavigateCore(content, null, null);

	/// <summary>Navigate to an object, passing <paramref name="extraData"/> to the event arguments.</summary>
	public bool Navigate(object content, object? extraData) => NavigateCore(content, null, extraData);

	/// <summary>Navigate to <paramref name="uri"/>, resolved via <see cref="Resolver"/>.</summary>
	/// <returns><c>true</c> if navigation proceeded; <c>false</c> if cancelled.</returns>
	public bool Navigate(Uri uri) => Navigate(uri, null);

	/// <summary>Navigate to <paramref name="uri"/>, passing <paramref name="extraData"/> to the event arguments.</summary>
	public bool Navigate(Uri uri, object? extraData)
	{
		if (uri == null) throw new ArgumentNullException(nameof(uri));
		return NavigateCore(Resolve(uri), uri, extraData);
	}

	/// <summary>
	/// Go back to the previous journal entry.
	/// Raises <see cref="Navigating"/> (with <see cref="NavigationMode.Back"/>) before
	/// moving the cursor; if the handler sets <c>Cancel</c> the cursor is not moved.
	/// Does nothing (no exception) when <see cref="CanGoBack"/> is false.
	/// </summary>
	public void GoBack()
	{
		if (!CanGoBack) return;
		JournalNavigate(NavigationMode.Back, () => _journal.GoBack());
	}

	/// <summary>
	/// Go forward to the next journal entry.
	/// Raises <see cref="Navigating"/> (with <see cref="NavigationMode.Forward"/>) before
	/// moving the cursor; if the handler sets <c>Cancel</c> the cursor is not moved.
	/// Does nothing (no exception) when <see cref="CanGoForward"/> is false.
	/// </summary>
	public void GoForward()
	{
		if (!CanGoForward) return;
		JournalNavigate(NavigationMode.Forward, () => _journal.GoForward());
	}

	// ── Internal helpers ──────────────────────────────────────────────────────

	// The frame or window that owns this service; reported as the event sender and Navigator.
	internal object? Host { get; set; }

	bool NavigateCore(object? content, Uri? uri, object? extraData)
	{
		if (!RaiseNavigating(new NavigatingCancelEventArgs(content, uri, NavigationMode.New))) return false;

		if (content is PageFunctionBase pageFunction && Content != null && !ReferenceEquals(Content, pageFunction))
			pageFunction.Caller = Content;
		_journal.Push(content);
		CommitContent(content, uri, extraData);
		if (content is PageFunctionBase { Started: false } starting)
		{
			starting.Started = true;
			starting.InvokeStart();
		}
		return true;
	}

	/// <summary>
	/// Raises Navigating with the given <paramref name="mode"/> before moving the
	/// journal cursor via <paramref name="advance"/>. Honors Cancel: if the handler
	/// cancels, the cursor is not moved.
	/// </summary>
	void JournalNavigate(NavigationMode mode, Func<object?> advance)
	{
		// Peek at the target content (the entry we'd move to) for the event args.
		// We pass null content for the peek — WPF does the same for back/forward.
		if (!RaiseNavigating(new NavigatingCancelEventArgs(null, null, mode))) return;

		var content = advance();
		CommitContent(content, content == null ? null : SourceOf(content), null);
	}

	// Returns from a PageFunction to its caller: Back-style when the entry is dropped, a new entry otherwise.
	internal void ReturnTo(PageFunctionBase pageFunction, object caller, bool removeFromJournal)
	{
		if (!removeFromJournal || !ReferenceEquals(_journal.Current, pageFunction))
		{
			NavigateCore(caller, SourceOf(caller), null);
			return;
		}
		if (!RaiseNavigating(new NavigatingCancelEventArgs(caller, SourceOf(caller), NavigationMode.Back))) return;
		_journal.RemoveCurrent();
		ForgetSource(pageFunction);
		if (!ReferenceEquals(_journal.Current, caller)) _journal.Push(caller);
		CommitContent(caller, SourceOf(caller), null);
	}

	void CommitContent(object? content, Uri? uri, object? extraData)
	{
		Content = content;
		CurrentSource = uri;
		RememberSource(content, uri);
		// Wire NavigationService on Page.
		if (content is Page page)
			page.NavigationService = this;
		ContentChanged?.Invoke(this, content);
		object navigator = Host ?? this;
		var args = new NavigationEventArgs(uri, content, extraData, navigator, true);
		Navigated?.Invoke(navigator, args);
		LoadCompleted?.Invoke(navigator, args);
	}

	/// <summary>
	/// Raised when <see cref="Content"/> changes. Used internally by
	/// <see cref="System.Windows.Controls.Frame"/> and <see cref="NavigationWindow"/>
	/// to swap the displayed widget.
	/// </summary>
	internal event EventHandler<object?>? ContentChanged;
}