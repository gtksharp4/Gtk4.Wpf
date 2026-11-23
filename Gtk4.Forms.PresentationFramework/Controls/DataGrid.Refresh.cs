using System.Collections.Generic;
using System.Collections.Specialized;

namespace System.Windows.Controls;

public partial class DataGrid
{
	readonly List<DataGridColumn> _ownedColumns = new List<DataGridColumn>();

	void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		_containersDirty = true;
		if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			foreach (var old in _ownedColumns)
				if (_autoColumns == null || !_autoColumns.Contains(old)) old.DataGridOwner = null;
			_ownedColumns.RemoveAll(c => _autoColumns == null || !_autoColumns.Contains(c));
			foreach (var column in Columns) Own(column);
			return;
		}
		if (e.OldItems != null)
			foreach (DataGridColumn old in e.OldItems) Disown(old);
		if (e.NewItems != null)
			foreach (DataGridColumn added in e.NewItems) Own(added);
	}

	void Own(DataGridColumn column)
	{
		column.DataGridOwner = this;
		if (!_ownedColumns.Contains(column)) _ownedColumns.Add(column);
	}

	void Disown(DataGridColumn column)
	{
		column.DataGridOwner = null;
		_ownedColumns.Remove(column);
	}

	// Re-applies the style and read-only state in place, so the built cells and headers keep their identity.
	internal void RefreshCellsAndHeaders()
	{
		if (_rowContainers.Count == 0 && _headerContainers.Count == 0) return;
		foreach (var row in _rowContainers)
			if (row.Presenter != null)
				foreach (var cell in row.Presenter.Cells) ApplyCellSettings(cell);
		foreach (var header in _headerContainers) ApplyHeaderSettings(header);
	}

	internal void ApplyCellSettings(DataGridCell cell)
	{
		var column = cell.Column!;
		cell.IsReadOnly = column.IsReadOnly || IsReadOnly;
		ApplyStyle(cell, column.CellStyle ?? CellStyle);
	}

	void ApplyHeaderSettings(DataGridColumnHeader header) =>
		ApplyStyle(header, header.Column!.HeaderStyle ?? ColumnHeaderStyle);

	static void ApplyStyle(FrameworkElement element, Style? style)
	{
		if (style != null)
		{
			element.Style = style;
			return;
		}
		element.ClearValue(FrameworkElement.StyleProperty);
		element.ApplyImplicitStyle();
	}
}
