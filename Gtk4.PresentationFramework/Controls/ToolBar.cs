namespace System.Windows.Controls;

/// <summary>
/// A horizontal toolbar backed by <see cref="Gtk.Box"/> (horizontal).
/// Hosts any <see cref="UIElement"/>s — typically <see cref="Button"/>s or
/// <see cref="Separator"/>s.
/// <para>
/// GTK-widget choice: <c>Gtk.ActionBar</c> provides start/center/end sections but
/// requires individual packing with <c>PackStart</c>/<c>PackEnd</c>. A horizontal
/// <c>Gtk.Box</c> is a simpler faithful container for this phase; <c>Gtk.ActionBar</c>
/// integration is deferred.
/// </para>
/// </summary>
public partial class ToolBar : HeaderedItemsControl
{
	Gtk.Box? _box;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateItemsWidget()
	{
		_box = new Gtk.Box(Gtk.Orientation.Horizontal, 4);
		return _box;
	}

	/// <inheritdoc/>
	protected override void AddItemWidget(Gtk.Widget w) => _box!.Add(w);

	/// <inheritdoc/>
	protected override void RemoveItemWidget(Gtk.Widget w) => _box!.Remove(w);
}