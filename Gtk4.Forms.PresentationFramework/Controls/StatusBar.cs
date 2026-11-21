namespace System.Windows.Controls.Primitives;

/// <summary>
/// A horizontal bar that hosts status items, typically at the bottom of a window.
/// Backed by a horizontal <see cref="Gtk.Box"/>.
/// Extends <see cref="ItemsControl"/>: add items via <see cref="ItemsControl.ItemsSource"/>
/// or the <c>Items</c> collection.
/// </summary>
public partial class StatusBar : ItemsControl
{
	Gtk.Box? _statusBox;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateItemsWidget()
	{
		_statusBox = new Gtk.Box(Gtk.Orientation.Horizontal, 4);
		return _statusBox;
	}

	/// <inheritdoc/>
	protected override void AddItemWidget(Gtk.Widget w)
	{
		w.Hexpand = false;
		_statusBox!.Append(w);
	}

	/// <inheritdoc/>
	protected override void RemoveItemWidget(Gtk.Widget w) => _statusBox!.Remove(w);
}