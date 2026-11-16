using System;
using System.Windows.Controls;

namespace System.Windows.Navigation
{
	/// <summary>
	/// Navigation service: back/forward journal, <see cref="Navigate"/> overloads, and
	/// <see cref="Navigating"/>/<see cref="Navigated"/>/<see cref="NavigationStopped"/> events.
	/// </summary>
	/// <remarks>
	/// URI navigation is resolved via <see cref="Resolver"/> (default: <see cref="DictionaryContentResolver"/>).
	/// Register page instances with <see cref="RegisterContent"/> — a runtime XAML loader
	/// is not available; the markup compiler compiles pages to <c>InitializeComponent</c>.
	/// </remarks>
	public sealed class NavigationService
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
		public event EventHandler<NavigatingCancelEventArgs>? Navigating;

		/// <summary>Raised after navigation completes.</summary>
		public event EventHandler<NavigatedEventArgs>? Navigated;

		/// <summary>Raised when navigation is cancelled via <see cref="NavigatingCancelEventArgs.Cancel"/>.</summary>
		public event EventHandler<NavigatingCancelEventArgs>? NavigationStopped;

		// ── Navigate ──────────────────────────────────────────────────────────────

		/// <summary>Navigate to an object (a <see cref="Page"/>, any <see cref="UIElement"/>, or other).</summary>
		/// <returns><c>true</c> if navigation proceeded; <c>false</c> if cancelled.</returns>
		public bool Navigate(object content) => NavigateCore(content, null);

		/// <summary>Navigate to <paramref name="uri"/>, resolved via <see cref="Resolver"/>.</summary>
		/// <returns><c>true</c> if navigation proceeded; <c>false</c> if cancelled.</returns>
		public bool Navigate(Uri uri)
		{
			if (uri == null) throw new ArgumentNullException(nameof(uri));
			var resolved = Resolver.Resolve(uri);
			return NavigateCore(resolved, uri);
		}

		/// <summary>Go back to the previous journal entry.</summary>
		/// <exception cref="InvalidOperationException">If <see cref="CanGoBack"/> is false.</exception>
		public void GoBack()
		{
			var prev = _journal.GoBack();
			CommitContent(prev, null);
		}

		/// <summary>Go forward to the next journal entry.</summary>
		/// <exception cref="InvalidOperationException">If <see cref="CanGoForward"/> is false.</exception>
		public void GoForward()
		{
			var next = _journal.GoForward();
			CommitContent(next, null);
		}

		// ── Internal helpers ──────────────────────────────────────────────────────

		bool NavigateCore(object? content, Uri? uri)
		{
			var cancelling = new NavigatingCancelEventArgs(content, uri);
			Navigating?.Invoke(this, cancelling);
			if (cancelling.Cancel)
			{
				NavigationStopped?.Invoke(this, cancelling);
				return false;
			}

			_journal.Push(content);
			CommitContent(content, uri);
			return true;
		}

		void CommitContent(object? content, Uri? uri)
		{
			Content = content;
			// Wire NavigationService on Page.
			if (content is Page page)
				page.NavigationService = this;
			ContentChanged?.Invoke(this, content);
			Navigated?.Invoke(this, new NavigatedEventArgs(content, uri));
		}

		/// <summary>
		/// Raised when <see cref="Content"/> changes. Used internally by
		/// <see cref="System.Windows.Controls.Frame"/> and <see cref="NavigationWindow"/>
		/// to swap the displayed widget.
		/// </summary>
		internal event EventHandler<object?>? ContentChanged;
	}
}
