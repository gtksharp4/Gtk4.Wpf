using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>Specifies when a scroll bar is shown.</summary>
public enum ScrollBarVisibility
{
	/// <summary>The scroll bar is not shown; content clips.</summary>
	Disabled,
	/// <summary>The scroll bar is shown only when needed.</summary>
	Auto,
	/// <summary>The scroll bar is always hidden (but content may still scroll).</summary>
	Hidden,
	/// <summary>The scroll bar is always visible.</summary>
	Visible,
}

/// <summary>
/// A control that provides a scrollable view of its content, backed by
/// <see cref="Gtk.ScrolledWindow"/>.
/// </summary>
[ContentProperty("Content")]
public partial class ScrollViewer : ContentControl
{
	// ── HorizontalScrollBarVisibility DP ────────────────────────────────────

	/// <summary>Visibility of the horizontal scroll bar.</summary>
	public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
		DependencyProperty.Register(
			nameof(HorizontalScrollBarVisibility),
			typeof(ScrollBarVisibility),
			typeof(ScrollViewer),
			new PropertyMetadata(ScrollBarVisibility.Disabled, OnScrollBarVisibilityChanged),
			IsValidScrollBarVisibility);

	/// <summary>Gets or sets the horizontal scroll bar visibility.</summary>
	public ScrollBarVisibility HorizontalScrollBarVisibility
	{
		get => (ScrollBarVisibility?)GetValue(HorizontalScrollBarVisibilityProperty)
		       ?? ScrollBarVisibility.Disabled;
		set => SetValue(HorizontalScrollBarVisibilityProperty, value);
	}

	// ── VerticalScrollBarVisibility DP ──────────────────────────────────────

	/// <summary>Visibility of the vertical scroll bar.</summary>
	public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
		DependencyProperty.Register(
			nameof(VerticalScrollBarVisibility),
			typeof(ScrollBarVisibility),
			typeof(ScrollViewer),
			new PropertyMetadata(ScrollBarVisibility.Visible, OnScrollBarVisibilityChanged),
			IsValidScrollBarVisibility);

	/// <summary>Gets or sets the vertical scroll bar visibility.</summary>
	public ScrollBarVisibility VerticalScrollBarVisibility
	{
		get => (ScrollBarVisibility?)GetValue(VerticalScrollBarVisibilityProperty)
		       ?? ScrollBarVisibility.Visible;
		set => SetValue(VerticalScrollBarVisibilityProperty, value);
	}

	/// <summary>
	/// Gets the horizontal scroll bar visibility set on <paramref name="element"/>.
	/// Only ScrollViewer, TextBox and RichTextBox apply this value; on other elements (ListBox, DataGrid,
	/// ItemsControl) it is stored but has no effect.
	/// </summary>
	public static ScrollBarVisibility GetHorizontalScrollBarVisibility(DependencyObject element)
		=> (ScrollBarVisibility)(Require(element).GetValue(HorizontalScrollBarVisibilityProperty)
			?? ScrollBarVisibility.Disabled);

	/// <summary>
	/// Sets the horizontal scroll bar visibility on <paramref name="element"/>.
	/// Only ScrollViewer, TextBox and RichTextBox apply this value; on other elements (ListBox, DataGrid,
	/// ItemsControl) it is stored but has no effect.
	/// </summary>
	public static void SetHorizontalScrollBarVisibility(DependencyObject element, ScrollBarVisibility value)
		=> Require(element).SetValue(HorizontalScrollBarVisibilityProperty, value);

	/// <summary>
	/// Gets the vertical scroll bar visibility set on <paramref name="element"/>.
	/// Only ScrollViewer, TextBox and RichTextBox apply this value; on other elements (ListBox, DataGrid,
	/// ItemsControl) it is stored but has no effect.
	/// </summary>
	public static ScrollBarVisibility GetVerticalScrollBarVisibility(DependencyObject element)
		=> (ScrollBarVisibility)(Require(element).GetValue(VerticalScrollBarVisibilityProperty)
			?? ScrollBarVisibility.Visible);

	/// <summary>
	/// Sets the vertical scroll bar visibility on <paramref name="element"/>.
	/// Only ScrollViewer, TextBox and RichTextBox apply this value; on other elements (ListBox, DataGrid,
	/// ItemsControl) it is stored but has no effect.
	/// </summary>
	public static void SetVerticalScrollBarVisibility(DependencyObject element, ScrollBarVisibility value)
		=> Require(element).SetValue(VerticalScrollBarVisibilityProperty, value);

	static DependencyObject Require(DependencyObject element)
		=> element ?? throw new ArgumentNullException(nameof(element));

	static bool IsValidScrollBarVisibility(object? v) =>
		v is ScrollBarVisibility s && s >= ScrollBarVisibility.Disabled && s <= ScrollBarVisibility.Visible;

	static void OnScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as ScrollViewer)?.ApplyScrollPolicy();
	}

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.ScrolledWindow _scroll = null!;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_scroll = new Gtk.ScrolledWindow();
		ApplyScrollPolicy();
		if (Content is UIElement child)
			_scroll.Child = child.Widget;
		return _scroll;
	}

	/// <inheritdoc/>
	protected override void OnWidgetCreated(Gtk.Widget widget)
	{
		base.OnWidgetCreated(widget);
		AttachAdjustmentHandlers();
	}

	/// <inheritdoc/>
	protected override Gtk.Box ContentContainer =>
		throw new InvalidOperationException("ScrollViewer manages its own content.");

	/// <inheritdoc/>
	protected override Gtk.Label ContentLabel =>
		throw new InvalidOperationException("ScrollViewer manages its own content.");

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == ContentProperty && _scroll != null)
		{
			// Remove old child.
			if (e.OldValue is UIElement old && _scroll.Child != null)
				_scroll.Child = null;
			// Add new child.
			if (e.NewValue is UIElement newEl)
				_scroll.Child = newEl.Widget;
		}
	}

	void ApplyScrollPolicy()
	{
		if (_scroll == null) return;
		_scroll.HscrollbarPolicy = ViewerPolicy(HorizontalScrollBarVisibility);
		_scroll.VscrollbarPolicy = ViewerPolicy(VerticalScrollBarVisibility);
	}

	// Disabled clips the content to the viewport like WPF; Never would force the window to the content's minimum.
	static Gtk.PolicyType ViewerPolicy(ScrollBarVisibility v)
		=> v == ScrollBarVisibility.Disabled ? Gtk.PolicyType.External : ToGtkPolicy(v);

	internal static Gtk.PolicyType ToGtkPolicy(ScrollBarVisibility v) => v switch
	{
		ScrollBarVisibility.Disabled => Gtk.PolicyType.Never,
		ScrollBarVisibility.Hidden   => Gtk.PolicyType.External,
		ScrollBarVisibility.Visible  => Gtk.PolicyType.Always,
		_                            => Gtk.PolicyType.Automatic,
	};
}