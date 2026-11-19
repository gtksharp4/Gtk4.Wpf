using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Controls;

/// <summary>
/// A tabular data control.
/// <para>
/// <strong>Phase-1 scope (functional minimal):</strong>
/// <list type="bullet">
///   <item>Supported: <see cref="ItemsSource"/>, explicit <see cref="Columns"/> (
///     <see cref="DataGridTextColumn"/>), <see cref="AutoGenerateColumns"/> (reflects
///     public readable properties), <see cref="SelectedItem"/>.</item>
///   <item>Not supported: cell editing, filtering, grouping, row details,
///     frozen columns, row/column headers resize, virtualization, copy/paste.</item>
/// </list>
/// </para>
/// GTK4 backing: a <see cref="Gtk.ScrolledWindow"/> wrapping a <see cref="Gtk.Grid"/>
/// of <see cref="Gtk.Label"/> cells. Each row is marked with a
/// <see cref="Gtk.GestureClick"/> for selection. Column generation split into
/// <c>DataGrid.Columns.cs</c>.
/// </summary>
public partial class DataGrid : Control
{
	// ── ItemsSource DP ────────────────────────────────────────────────────────

	/// <summary>The collection of rows to display.</summary>
	public static readonly DependencyProperty ItemsSourceProperty =
		DependencyProperty.Register(
			nameof(ItemsSource),
			typeof(IEnumerable),
			typeof(DataGrid),
			new PropertyMetadata(null, OnItemsSourceChanged));

	static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var dg = (DataGrid)d;
		dg.SubscribeCollectionChanged(e.NewValue as IEnumerable);
		dg.Rebuild();
	}

	/// <summary>Gets or sets the items source.</summary>
	public IEnumerable? ItemsSource
	{
		get => (IEnumerable?)GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	// ── Columns ──────────────────────────────────────────────────────────────

	/// <summary>Column definitions (empty = auto-generate when AutoGenerateColumns is true).</summary>
	public ObservableCollection<DataGridColumn> Columns { get; } =
		new ObservableCollection<DataGridColumn>();

	// ── AutoGenerateColumns DP ────────────────────────────────────────────────

	/// <summary>
	/// When <c>true</c> and <see cref="Columns"/> is empty, one
	/// <see cref="DataGridTextColumn"/> is generated per public readable property
	/// of the item type.
	/// </summary>
	public static readonly DependencyProperty AutoGenerateColumnsProperty =
		DependencyProperty.Register(
			nameof(AutoGenerateColumns),
			typeof(bool),
			typeof(DataGrid),
			new PropertyMetadata(true, OnItemsSourceChanged));

	/// <summary>Gets or sets automatic column generation.</summary>
	public bool AutoGenerateColumns
	{
		get => (bool?)GetValue(AutoGenerateColumnsProperty) ?? true;
		set => SetValue(AutoGenerateColumnsProperty, value);
	}

	// ── SelectedItem DP ───────────────────────────────────────────────────────

	/// <summary>The currently selected row item.</summary>
	public static readonly DependencyProperty SelectedItemProperty =
		DependencyProperty.Register(
			nameof(SelectedItem),
			typeof(object),
			typeof(DataGrid),
			new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

	static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((DataGrid)d).ApplySelectedItem(e.NewValue);

	/// <summary>Gets or sets the selected row item.</summary>
	public object? SelectedItem
	{
		get => GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
	}

	/// <summary>Raised when <see cref="SelectedItem"/> changes.</summary>
	public event EventHandler? SelectionChanged;

	// ── GTK backing ──────────────────────────────────────────────────────────

	internal Gtk.ScrolledWindow? _scroll;
	internal Gtk.Grid? _grid;
	internal readonly List<object?> _rows = new List<object?>();
	bool _updating;

	// INPC subscription for the current ItemsSource (R8).
	System.Collections.Specialized.NotifyCollectionChangedEventHandler? _collectionChangedHandler;
	IEnumerable? _prevItemsSource;

	/// <summary>Snapshot of items (populated when <see cref="ItemsSource"/> is set).</summary>
	public IReadOnlyList<object?> Items => _rows;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_scroll = new Gtk.ScrolledWindow();
		_grid = new Gtk.Grid();
		_grid.RowHomogeneous = false;
		_grid.ColumnHomogeneous = false;
		_scroll.Child = _grid;
		Columns.CollectionChanged += (s, e) => Rebuild();
		Rebuild();
		return _scroll;
	}

	void SubscribeCollectionChanged(IEnumerable? source)
	{
		if (_collectionChangedHandler != null
		    && _prevItemsSource is System.Collections.Specialized.INotifyCollectionChanged old)
			old.CollectionChanged -= _collectionChangedHandler;
		_collectionChangedHandler = null;
		_prevItemsSource = source;
		if (source is System.Collections.Specialized.INotifyCollectionChanged incc)
		{
			_collectionChangedHandler = (s, e) => Rebuild();
			incc.CollectionChanged += _collectionChangedHandler;
		}
	}

	void SelectRow(object? item)
	{
		if (_updating) return;
		_updating = true;
		try { SetValue(SelectedItemProperty, item); }
		finally { _updating = false; }
		SelectionChanged?.Invoke(this, EventArgs.Empty);
	}

	void ApplySelectedItem(object? item)
	{
		// Visual highlight is Phase-2 (CSS row class); DP is updated.
	}
}