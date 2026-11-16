using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Windows.Controls
{
	/// <summary>
	/// A tabular data control.
	/// <para>
	/// <strong>Phase-1 scope (functional minimal):</strong>
	/// <list type="bullet">
	///   <item>Supported: <see cref="ItemsSource"/>, explicit <see cref="Columns"/> (
	///     <see cref="DataGridTextColumn"/>), <see cref="AutoGenerateColumns"/> (reflects
	///     public readable properties), <see cref="SelectedItem"/>.</item>
	///   <item>Not supported: cell editing, sorting, filtering, grouping, row details,
	///     frozen columns, row/column headers resize, virtualization, copy/paste.</item>
	/// </list>
	/// </para>
	/// GTK4 backing: a <see cref="Gtk.ScrolledWindow"/> wrapping a <see cref="Gtk.Grid"/>
	/// of <see cref="Gtk.Label"/> cells. Each row is marked with a
	/// <see cref="Gtk.GestureClick"/> for selection.
	/// </summary>
	public class DataGrid : Control
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
			=> ((DataGrid)d).Rebuild();

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
				new PropertyMetadata(null, OnSelectedItemChanged));

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

		Gtk.ScrolledWindow? _scroll;
		Gtk.Grid? _grid;
		readonly List<object?> _rows = new List<object?>();
		bool _updating;

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

		IReadOnlyList<DataGridColumn> GetEffectiveColumns()
		{
			if (Columns.Count > 0) return Columns;
			if (!AutoGenerateColumns) return Array.Empty<DataGridColumn>();

			// Reflect public readable properties of first item.
			object? first = null;
			if (ItemsSource != null)
				foreach (var item in ItemsSource) { first = item; break; }

			if (first == null) return Array.Empty<DataGridColumn>();

			var cols = new List<DataGridColumn>();
			foreach (var prop in first.GetType().GetProperties(
				BindingFlags.Public | BindingFlags.Instance))
			{
				if (!prop.CanRead) continue;
				cols.Add(new DataGridTextColumn
				{
					Header = prop.Name,
					Binding = new Windows.Data.Binding(prop.Name),
				});
			}
			return cols;
		}

		void Rebuild()
		{
			_rows.Clear();
			if (ItemsSource != null)
				foreach (var item in ItemsSource)
					_rows.Add(item);

			if (_grid == null) return;

			// Detach all children from the grid.
			var toRemove = new List<Gtk.Widget>();
			for (var child = _grid.FirstChild; child != null; child = child.NextSibling)
				toRemove.Add(child);
			foreach (var w in toRemove)
				_grid.Remove(w);

			var cols = GetEffectiveColumns();
			if (cols.Count == 0) return;

			// Header row.
			for (int c = 0; c < cols.Count; c++)
			{
				var hdr = new Gtk.Label(cols[c].Header?.ToString() ?? string.Empty);
				hdr.Hexpand = true;
				hdr.AddCssClass("datagrid-header");
				_grid.Attach(hdr, c, 0, 1, 1);
			}

			// Data rows.
			int row = 1;
			foreach (var item in _rows)
			{
				for (int c = 0; c < cols.Count; c++)
				{
					var cell = new Gtk.Label(cols[c].GetCellText(item));
					cell.Hexpand = true;
					cell.Xalign = 0.0f;
					cell.AddCssClass("datagrid-cell");
					_grid.Attach(cell, c, row, 1, 1);
				}
				// Row click for selection — attach gesture to first cell.
				var firstCell = _grid.GetChildAt(0, row);
				if (firstCell != null)
				{
					var capturedItem = item;
					var click = new Gtk.GestureClick();
					click.Pressed += (s, e) => SelectRow(capturedItem);
					firstCell.AddController(click);
				}
				row++;
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
}
