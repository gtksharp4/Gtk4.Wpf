using System.Collections.Generic;
using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class DataGrid
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new DataGridAutomationPeer(this);

	internal IReadOnlyList<DataGridColumn> AutomationColumns => GetEffectiveColumns();

	// The label GTK shows for a cell; row 0 of the GTK grid is the header row.
	internal Gtk.Widget? AutomationCellWidget(int row, int column) => _grid?.GetChildAt(column, row + 1);

	internal Gtk.Widget? AutomationHeaderWidget(int column) => _grid?.GetChildAt(column, 0);

	internal int AutomationRowIndex(object? item) => item == null ? -1 : _rows.IndexOf(item);


	internal void AutomationSelectRow(object? item) => SelectRow(item);
}
