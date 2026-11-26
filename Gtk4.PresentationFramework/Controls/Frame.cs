using System;
using System.Windows.Navigation;

namespace System.Windows.Controls;

/// <summary>
/// A navigable content host with its own back/forward journal.
/// <see cref="Source"/> triggers URI navigation; <see cref="Content"/> reflects the
/// current page. The displayed widget is swapped inside a <see cref="Gtk.Box"/>
/// container on each navigation.
/// </summary>
public partial class Frame : ContentControl
{
	// ── NavigationService ─────────────────────────────────────────────────────

	/// <summary>The navigation service owned by this frame.</summary>
	public NavigationService NavigationService { get; } = new NavigationService();

	/// <summary>Raised after navigation, before <see cref="LoadCompleted"/>; the sender is this frame.</summary>
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

	// ── CanGoBack / CanGoForward ──────────────────────────────────────────────

	/// <summary>True when <see cref="GoBack"/> is possible.</summary>
	public bool CanGoBack => NavigationService.CanGoBack;

	/// <summary>True when <see cref="GoForward"/> is possible.</summary>
	public bool CanGoForward => NavigationService.CanGoForward;

	// ── Source DP ─────────────────────────────────────────────────────────────

	/// <summary>URI of the current page.</summary>
	public static readonly DependencyProperty SourceProperty =
		DependencyProperty.Register(
			nameof(Source),
			typeof(Uri),
			typeof(Frame),
			new PropertyMetadata(null, OnSourceChanged));

	static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var frame = (Frame)d;
		if (e.NewValue is Uri uri)
			frame.NavigationService.Navigate(uri);
	}

	/// <summary>Gets or sets the current source URI.</summary>
	public Uri? Source
	{
		get => (Uri?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	// ── Navigate helpers ──────────────────────────────────────────────────────

	/// <summary>Navigates to <paramref name="content"/>.</summary>
	public bool Navigate(object content) => NavigationService.Navigate(content);

	/// <summary>Navigates to <paramref name="uri"/>.</summary>
	public bool Navigate(Uri uri) => NavigationService.Navigate(uri);

	/// <summary>Goes back one entry in the journal.</summary>
	public void GoBack() => NavigationService.GoBack();

	/// <summary>Goes forward one entry in the journal.</summary>
	public void GoForward() => NavigationService.GoForward();

	// ── GTK backing ───────────────────────────────────────────────────────────

	Gtk.Box? _container;
	Gtk.Widget? _currentWidget;

	/// <summary>Initialises the frame and wires content-change to widget swaps.</summary>
	public Frame()
	{
		NavigationService.Host = this;
		NavigationService.ContentChanged += OnNavigationContentChanged;
	}

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_container = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		// If a navigation already happened before widget creation, show it now.
		SwapWidget(NavigationService.Content);
		return _container;
	}

	void OnNavigationContentChanged(object? sender, object? newContent)
	{
		// Reflect Content DP so callers can read it.
		SetValue(ContentProperty, newContent);

		if (_container == null) return;
		SwapWidget(newContent);
		Dispatcher.BeginInvoke(
			new Action(() => OnContentRendered(EventArgs.Empty)), System.Windows.Threading.DispatcherPriority.Loaded);
	}

	void SwapWidget(object? content)
	{
		if (_currentWidget != null)
		{
			_container!.Remove(_currentWidget);
			_currentWidget = null;
		}

		UIElement? el = content as UIElement;
		if (el != null)
		{
			_currentWidget = el.Widget;
			_container!.Add(_currentWidget);
		}
	}
}