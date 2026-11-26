namespace System.Windows.Controls;

/// <summary>
/// A horizontal menu bar hosting <see cref="MenuItem"/>s, backed by a horizontal
/// <see cref="Gtk.Box"/> of <see cref="Gtk.MenuButton"/>s.
/// <para>
/// GTK-widget choice: <c>Gtk.PopoverMenuBar</c> requires a full <c>Gio.Menu</c> model
/// and cannot host arbitrary child widgets. A <c>Gtk.Box</c> of <c>Gtk.MenuButton</c>s
/// (one per top-level <see cref="MenuItem"/>) is simpler and equally functional for this
/// phase. <c>Gtk.PopoverMenuBar</c> integration is a future phase.
/// </para>
/// </summary>
public partial class Menu : ItemsControl
{
	Gtk.Box? _bar;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateItemsWidget()
	{
		_bar = new Gtk.Box(Gtk.Orientation.Horizontal, 4);
		return _bar;
	}

	/// <inheritdoc/>
	protected override UIElement GetContainerForItem(object? item)
	{
		// Accept MenuItem directly; wrap plain objects in a default MenuItem.
		if (item is MenuItem mi) return mi;
		return new MenuItem { Header = item?.ToString() ?? string.Empty };
	}

	/// <inheritdoc/>
	protected override void AddItemWidget(Gtk.Widget w) => _bar!.Add(w);

	/// <inheritdoc/>
	protected override void RemoveItemWidget(Gtk.Widget w) => _bar!.Remove(w);
}