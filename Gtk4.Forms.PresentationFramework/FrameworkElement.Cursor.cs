using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// <see cref="Cursor"/> dependency property on <see cref="FrameworkElement"/>.
/// <para>
/// When set and the backing <see cref="UIElement.Widget"/> is already created the
/// GTK cursor is applied immediately via <c>Gtk.Widget.CursorFromName</c>
/// (<c>gtk_widget_set_cursor_from_name</c>). When set before widget creation the
/// value is stored and applied in <see cref="UIElement.OnWidgetCreated"/>.
/// </para>
/// </summary>
public abstract partial class FrameworkElement
{
	// ── Cursor DP ────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="Cursor"/> dependency property.
	/// Default: <c>null</c> (inherits the system default).
	/// </summary>
	public static readonly DependencyProperty CursorProperty =
		DependencyProperty.Register(
			nameof(Cursor),
			typeof(Cursor),
			typeof(FrameworkElement),
			new PropertyMetadata(null, OnCursorChanged));

	static void OnCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FrameworkElement fe && fe.IsWidgetCreated)
			fe.ApplyCursor(fe.Widget);
	}

	/// <summary>
	/// Gets or sets the mouse cursor displayed when the pointer is over this element.
	/// Set to <c>null</c> to inherit the cursor from the parent / system default.
	/// </summary>
	public Cursor? Cursor
	{
		get => GetValue(CursorProperty) as Cursor;
		set => SetValue(CursorProperty, value);
	}

	// ── GTK application ──────────────────────────────────────────────────────

	// Implements the partial hook declared in FrameworkElement.Popups.cs.
	partial void ApplyCursorOnCreated(Gtk.Widget widget) => ApplyCursor(widget);

	/// <summary>
	/// Pushes the current <see cref="Cursor"/> value onto the backing GTK widget
	/// via <c>gtk_widget_set_cursor_from_name</c>.
	/// </summary>
	void ApplyCursor(Gtk.Widget widget)
	{
		var cursor = GetValue(CursorProperty) as Cursor;
		// CursorFromName is a write-only string property on Gtk.Widget that calls
		// gtk_widget_set_cursor_from_name; passing null resets to the default cursor.
		widget.CursorFromName = cursor?.GdkName!;
	}
}
