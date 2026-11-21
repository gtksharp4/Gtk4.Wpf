using System.Windows.Navigation;

namespace System.Windows.Controls;

/// <summary>
/// A navigable content host. Hosted inside a <see cref="Frame"/> or
/// <see cref="NavigationWindow"/>; its <see cref="NavigationService"/> is injected
/// by the host immediately after navigation.
/// Backed by a <see cref="Gtk.Box"/> that hosts its <see cref="Content"/> widget.
/// </summary>
public partial class Page : FrameworkElement
{
	// ── Content DP ───────────────────────────────────────────────────────────

	/// <summary>The page content.</summary>
	public static readonly DependencyProperty ContentProperty =
		DependencyProperty.Register(
			nameof(Content),
			typeof(object),
			typeof(Page),
			new PropertyMetadata(null, OnContentChanged));

	static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Page)d).UpdateContent(e.OldValue, e.NewValue);
	}

	/// <summary>Gets or sets the page content.</summary>
	public object? Content
	{
		get => GetValue(ContentProperty);
		set => SetValue(ContentProperty, value);
	}

	// ── Title / WindowTitle ──────────────────────────────────────────────────

	/// <summary>Title shown in breadcrumbs / navigation UI.</summary>
	public string Title { get; set; } = string.Empty;

	/// <summary>Sets the host window's title when this page is current.</summary>
	public string WindowTitle { get; set; } = string.Empty;

	// ── NavigationService ─────────────────────────────────────────────────────

	/// <summary>
	/// The <see cref="NavigationService"/> provided by the hosting
	/// <see cref="Frame"/> or <see cref="NavigationWindow"/>. Set automatically
	/// after navigation completes.
	/// </summary>
	public NavigationService? NavigationService { get; internal set; }

	// ── GTK backing ───────────────────────────────────────────────────────────

	Gtk.Box? _box;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_box = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		ApplyContent(null, Content);
		return _box;
	}

	void UpdateContent(object? oldContent, object? newContent)
	{
		if (oldContent is UIElement oldEl)
			oldEl.LogicalParent = null;
		if (newContent is UIElement newEl)
			newEl.LogicalParent = this;

		if (_box == null) return;
		ApplyContent(oldContent, newContent);
	}

	void ApplyContent(object? oldContent, object? newContent)
	{
		if (oldContent is UIElement old)
			_box!.Remove(old.Widget);

		if (newContent is UIElement newEl)
			_box!.Add(newEl.Widget);
	}
}