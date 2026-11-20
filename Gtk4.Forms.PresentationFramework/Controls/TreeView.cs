namespace System.Windows.Controls;

/// <summary>
/// A hierarchical tree control backed by a vertical <see cref="Gtk.Box"/> of
/// <see cref="TreeViewItem"/> nodes (each backed by a <see cref="Gtk.Expander"/>).
/// <para>
/// GTK-widget choice: <c>Gtk.TreeView</c>/<c>Gtk.ListView</c>+<c>Gtk.TreeExpander</c>
/// require a full <c>Gio.ListModel</c> tree model. The nested-Expander/Box approach
/// directly hosts <see cref="UIElement"/> children and is faithful for this phase.
/// </para>
/// </summary>
public partial class TreeView : ItemsControl
{
	/// <summary>Raised when <see cref="SelectedItem"/> changes.</summary>
	public event EventHandler<SelectionChangedEventArgs>? SelectedItemChanged;

	// ── SelectedItem DP ───────────────────────────────────────────────────────

	/// <summary>The currently selected <see cref="TreeViewItem"/>.</summary>
	public static readonly DependencyProperty SelectedItemProperty =
		DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(TreeView),
			new PropertyMetadata(null, OnSelectedItemChanged));

	static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var tv = (TreeView)d;
		if (tv._updating) return;
		tv._updating = true;
		try
		{
			// Deselect old item, select new one.
			if (e.OldValue is TreeViewItem oldTvi)
				oldTvi.IsSelected = false;
			if (e.NewValue is TreeViewItem newTvi)
				newTvi.IsSelected = true;
		}
		finally { tv._updating = false; }
		tv.SelectedItemChanged?.Invoke(tv,
			SelectionChangedEventArgs.ForItems(
			Primitives.Selector.SelectionChangedEvent, tv, e.OldValue, e.NewValue));
	}

	/// <summary>Gets or sets the selected item.</summary>
	public object? SelectedItem
	{
		get => GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
	}

	// ── GTK backing ───────────────────────────────────────────────────────────

	Gtk.Box? _box;
	bool _updating;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateItemsWidget()
	{
		_box = new Gtk.Box(Gtk.Orientation.Vertical, 2);
		return _box;
	}

	/// <inheritdoc/>
	protected override void AddItemWidget(Gtk.Widget w) => _box!.Add(w);

	/// <inheritdoc/>
	protected override void RemoveItemWidget(Gtk.Widget w) => _box!.Remove(w);

	void OnItemSelected(object? sender, EventArgs e)
	{
		if (_updating) return;
		if (sender is not TreeViewItem tvi) return;
		var oldItem = SelectedItem;
		if (ReferenceEquals(oldItem, tvi)) return;
		_updating = true;
		try
		{
			if (oldItem is TreeViewItem old) old.IsSelected = false;
			SetValue(SelectedItemProperty, tvi);
		}
		finally { _updating = false; }
		SelectedItemChanged?.Invoke(this, SelectionChangedEventArgs.ForItems(
			Primitives.Selector.SelectionChangedEvent, this, oldItem, tvi));
	}
}