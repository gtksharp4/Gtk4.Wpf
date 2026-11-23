using System;
using System.Collections.Generic;
using System.Reflection;

namespace System.Windows.Controls;

public partial class DataGrid
{
	IReadOnlyList<DataGridColumn> GetEffectiveColumns()
	{
		if (Columns.Count > 0) return Columns;
		if (!AutoGenerateColumns) return Array.Empty<DataGridColumn>();
		return _autoColumns ??= GenerateColumns();
	}

	// Reflects the public readable properties of the first item. Kept until the items change, so the columns
	// (and the cells that name them) stay the same objects between calls.
	List<DataGridColumn> GenerateColumns()
	{
		object? first = null;
		if (ItemsSource != null)
			foreach (var item in ItemsSource) { first = item; break; }

		var cols = new List<DataGridColumn>();
		if (first == null) return cols;
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
		_autoColumns = null;
		_containersDirty = true;

		if (_grid != null) RebuildGrid();
		ExistingAutomationPeer?.NotifyChildrenChanged();
	}

	/// <summary>
	/// Rebuilds the GTK grid from the current <see cref="_rows"/> snapshot.
	/// Called by <see cref="Rebuild"/> and by <c>ApplySort</c> after a sort.
	/// </summary>
	internal void RebuildGrid()
	{
		if (_grid == null) return;

		// Detach all children from the grid.
		var toRemove = new List<Gtk.Widget>();
		for (var child = _grid.FirstChild; child != null; child = child.NextSibling)
			toRemove.Add(child);
		foreach (var w in toRemove)
			_grid.Remove(w);

		var cols = GetEffectiveColumns();
		if (cols.Count == 0) return;

		// Header row (with sort click handlers). Each header is a label: DataGridColumnHeader is object model only.
		for (int c = 0; c < cols.Count; c++)
		{
			var col = cols[c];
			var hdr = new Gtk.Label(col.Header?.ToString() ?? string.Empty);
			hdr.Hexpand = true;
			hdr.AddCssClass("datagrid-header");
			_grid.Attach(hdr, c, 0, 1, 1);
			AttachHeaderClick(hdr, col);
		}

		// Data rows: each GTK cell is the widget of a DataGridCell hosting a TextBlock.
		var rows = RowContainers;
		for (int r = 0; r < rows.Count; r++)
		{
			var cells = rows[r].Presenter!.Cells;
			for (int c = 0; c < cells.Count; c++)
			{
				var cell = cells[c].Widget;
				cell.Hexpand = true;
				cell.AddCssClass("datagrid-cell");
				_grid.Attach(cell, c, r + 1, 1, 1);
			}
			// Row click for selection: the gesture sits on the first cell, once per cell.
			if (cells.Count > 0 && !cells[0].RowClickWired)
			{
				cells[0].RowClickWired = true;
				var capturedItem = rows[r].Item;
				var click = new Gtk.GestureClick();
				click.Pressed += (s, e) => SelectRow(capturedItem);
				cells[0].Widget.AddController(click);
			}
		}
	}
}
