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

		// Header row (with sort click handlers).
		for (int c = 0; c < cols.Count; c++)
		{
			var col = cols[c];
			var hdr = new Gtk.Label(col.Header?.ToString() ?? string.Empty);
			hdr.Hexpand = true;
			hdr.AddCssClass("datagrid-header");
			_grid.Attach(hdr, c, 0, 1, 1);
			AttachHeaderClick(hdr, col);
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
}