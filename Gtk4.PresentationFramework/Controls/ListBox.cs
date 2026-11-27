using System.Collections.Generic;
using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>
/// An items control with configurable selection, backed by <see cref="Gtk.ListBox"/>.
/// <para>
/// Each item is wrapped in a <see cref="ListBoxItem"/>. Single selection maps to
/// <see cref="SelectedItem"/> / <see cref="SelectedIndex"/>;
/// multi-selection additionally populates <see cref="SelectedItems"/>.
/// <see cref="SelectionChanged"/> is raised on every change.
/// </para>
/// </summary>
public partial class ListBox : Selector
{
	internal override DependencyProperty SelectedItemDependencyProperty => SelectedItemProperty;

	/// <inheritdoc/>
	internal override bool ImplicitSynchronization => SelectionMode == SelectionMode.Single;

	// ── SelectedIndex DP ──────────────────────────────────────────────────────

	/// <summary>Zero-based index of the first selected item; -1 when nothing selected.</summary>
	public static readonly DependencyProperty SelectedIndexProperty =
		DependencyProperty.Register(
			nameof(SelectedIndex),
			typeof(int),
			typeof(ListBox),
			new FrameworkPropertyMetadata(-1,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnSelectedIndexChanged));

	static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ListBox)d).ApplySelectedIndex((int?)e.NewValue ?? -1);
	}

	/// <summary>Gets or sets the selected index (first selected item in multi-select).</summary>
	public int SelectedIndex
	{
		get => (int?)GetValue(SelectedIndexProperty) ?? -1;
		set => SetValue(SelectedIndexProperty, value);
	}

	// ── SelectedItem DP ───────────────────────────────────────────────────────

	/// <summary>The first selected item (or the only selected item in Single mode).</summary>
	public static readonly DependencyProperty SelectedItemProperty =
		DependencyProperty.Register(
			nameof(SelectedItem),
			typeof(object),
			typeof(ListBox),
			new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

	static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var lb = (ListBox)d;
		lb.ApplySelectedItem(e.OldValue, e.NewValue);
		lb.RefreshSelectedValue();
	}

	/// <summary>Gets or sets the first (or only) selected item.</summary>
	public object? SelectedItem
	{
		get => GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
	}

	// ── GTK backing ──────────────────────────────────────────────────────────

	internal Gtk.ListBox? _gtkListBox;
	internal readonly List<ListBoxItem> _containers = new List<ListBoxItem>();

	/// <summary>
	/// Re-entrant guard: when set, DP callbacks and GTK handlers suppress
	/// further cross-sync and do NOT raise SelectionChanged a second time.
	/// </summary>
	bool _updating;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateItemsWidget()
	{
		_gtkListBox = new Gtk.ListBox();
		_gtkListBox.SelectionMode = ToGtkSelectionMode(SelectionMode);
		_gtkListBox.RowSelected += OnGtkRowSelected;
		_gtkListBox.SelectedRowsChanged += OnGtkSelectedRowsChanged;
		_gtkListBox.HeaderFunc = UpdateRowHeader;
		return _gtkListBox;
	}

	/// <inheritdoc/>
	protected override UIElement GetContainerForItem(object? item)
	{
		var lbi = new ListBoxItem { Content = item };
		ConfigurePresenter(lbi, item);
		PrepareContainer(lbi, item);
		_containers.Add(lbi);
		return lbi;
	}

	/// <inheritdoc/>
	protected override void AddItemWidget(Gtk.Widget w)
	{
		AttachPendingHeader(w);
		_gtkListBox!.Append(w);
	}

	/// <inheritdoc/>
	protected override void RemoveItemWidget(Gtk.Widget w) => _gtkListBox!.Remove(w);

	/// <inheritdoc/>
	protected override void OnItemsRebuilt()
	{
		_containers.Clear();

		// Preserve selections that are still present in Items after a rebuild.
		var prevSelected = new List<object?>(_selectedItems);
		var surviving = new List<object?>();
		foreach (var item in prevSelected)
		{
			if (Items.IndexOf(item) >= 0)
				surviving.Add(item);
		}

		_updating = true;
		try
		{
			_selectedItems.Clear();
			_selectedItems.AddRange(surviving);
			var primary = surviving.Count > 0 ? surviving[0] : null;
			var primaryIdx = primary != null ? Items.IndexOf(primary) : -1;
			SetValue(SelectedIndexProperty, primaryIdx);
			SetValue(SelectedItemProperty, primary);
		}
		finally { _updating = false; }
	}

	// ── GTK event handlers ────────────────────────────────────────────────────

	// Single-select path: RowSelected fires when one row changes.
	void OnGtkRowSelected(object? sender, Gtk.RowSelectedArgs e)
	{
		if (_updating) return;
		if (SelectionMode != SelectionMode.Single) return; // multi handled by SelectedRowsChanged

		var row = e.Row;
		var oldItem = SelectedItem;

		if (row == null)
		{
			SetSelectionCore(-1, null, oldItem);
			return;
		}

		int idx = FindContainerIndex(row);
		if (idx < 0) return;

		var newItem = idx < Items.Count ? Items[idx] : null;
		SetSelectionCore(idx, newItem, oldItem);
	}

	// Multi-select path: fires whenever the selection set changes.
	void OnGtkSelectedRowsChanged(object? sender, EventArgs e)
	{
		if (_updating) return;
		if (SelectionMode == SelectionMode.Single) return; // handled by RowSelected

		SyncSelectedItemsFromGtk();
	}

	int FindContainerIndex(Gtk.ListBoxRow row)
	{
		for (int i = 0; i < _containers.Count; i++)
			if (ReferenceEquals(_containers[i].Row, row)) return i;
		return -1;
	}

}
