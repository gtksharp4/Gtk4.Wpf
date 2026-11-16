namespace System.Windows.Controls
{
	/// <summary>
	/// Phase-0 Grid: backs with a <see cref="Gtk.Box"/> (vertical) so children stack
	/// top-to-bottom. Full row/column definitions are Phase-1 work.
	/// </summary>
	public class Grid : Panel
	{
		Gtk.Box? _box;

		/// <inheritdoc/>
		protected override bool IsWidgetCreated => _box != null;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_box = new Gtk.Box(Gtk.Orientation.Vertical, 0);
			// Pack any children added before the widget was created.
			foreach (var child in Children)
				_box.Add(child.Widget);
			return _box;
		}

		/// <inheritdoc/>
		protected override void AddChildWidget(UIElement child)
		{
			_box!.Add(child.Widget);
		}

		/// <inheritdoc/>
		protected override void RemoveChildWidget(UIElement child)
		{
			_box!.Remove(child.Widget);
		}
	}
}
