using System;
using System.Windows.Controls;

namespace System.Windows.Navigation;

/// <summary>
/// A <see cref="Window"/> that is also a navigation host. Owns a
/// <see cref="NavigationService"/> whose <see cref="NavigationService.Content"/> is
/// displayed in the window's content area. Reuses <see cref="Window"/>'s GTK hosting.
/// </summary>
public partial class NavigationWindow : Window
{
	// ── NavigationService ─────────────────────────────────────────────────────

	/// <summary>The navigation service owned by this window.</summary>
	public NavigationService NavigationService { get; } = new NavigationService();

	/// <summary>Raised after navigation, before <see cref="LoadCompleted"/>; the sender is this window.</summary>
	public event NavigatedEventHandler Navigated
	{
		add => NavigationService.Navigated += value;
		remove => NavigationService.Navigated -= value;
	}

	/// <summary>Raised once the navigated content is loaded.</summary>
	public event LoadCompletedEventHandler LoadCompleted
	{
		add => NavigationService.LoadCompleted += value;
		remove => NavigationService.LoadCompleted -= value;
	}

	// ── Journal pass-throughs ─────────────────────────────────────────────────

	/// <summary>True when <see cref="GoBack"/> is possible.</summary>
	public bool CanGoBack => NavigationService.CanGoBack;

	/// <summary>True when <see cref="GoForward"/> is possible.</summary>
	public bool CanGoForward => NavigationService.CanGoForward;

	// ── Navigate helpers ──────────────────────────────────────────────────────

	/// <summary>Navigates to <paramref name="content"/>.</summary>
	public bool Navigate(object content) => NavigationService.Navigate(content);

	/// <summary>Navigates to <paramref name="uri"/>.</summary>
	public bool Navigate(Uri uri) => NavigationService.Navigate(uri);

	/// <summary>Goes back one entry in the journal.</summary>
	public void GoBack() => NavigationService.GoBack();

	/// <summary>Goes forward one entry in the journal.</summary>
	public void GoForward() => NavigationService.GoForward();

	/// <summary>
	/// Initialises the window and wires navigation content to <see cref="Window.Content"/>.
	/// </summary>
	public NavigationWindow()
	{
		NavigationService.Host = this;
		NavigationService.ContentChanged += OnContentChanged;
	}

	void OnContentChanged(object? sender, object? newContent)
	{
		Content = newContent;
		SyncSource();
		// If navigated to a Page, push its WindowTitle into the window title bar.
		if (newContent is Page p && PageTitle(p) is { Length: > 0 } title)
			Title = title;
	}

	static string? PageTitle(Page p) => !string.IsNullOrEmpty(p.WindowTitle) ? p.WindowTitle : p.Title;
}
