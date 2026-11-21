using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>
/// The automation peer of a <see cref="DataGrid"/>: grid, table and selection patterns over its header cells,
/// rows and cells. Unlike WPF, <see cref="DataGrid"/> is not an <c>ItemsControl</c> here, so the row and cell
/// peers derive from <see cref="GridPartAutomationPeer"/> instead of <c>ItemAutomationPeer</c>, and rows
/// are cached per item reference (a repeated item shares one peer).
/// </summary>
public class DataGridAutomationPeer : FrameworkElementAutomationPeer, IGridProvider, ITableProvider,
	ISelectionProvider
{
	readonly Dictionary<object, DataGridItemAutomationPeer> _rows =
		new Dictionary<object, DataGridItemAutomationPeer>();
	readonly Dictionary<int, DataGridColumnHeaderAutomationPeer> _headers =
		new Dictionary<int, DataGridColumnHeaderAutomationPeer>();

	public DataGridAutomationPeer(DataGrid owner) : base(owner) { }

	internal DataGrid Grid => (DataGrid)Owner;

	public override object? GetPattern(PatternInterface patternInterface)
	{
		switch (patternInterface)
		{
			case PatternInterface.Grid:
			case PatternInterface.Table:
			case PatternInterface.Selection: return this;
			default: return base.GetPattern(patternInterface);
		}
	}

	protected override string GetClassNameCore() => "DataGrid";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.DataGrid;

	internal DataGridItemAutomationPeer RowPeer(object item)
	{
		if (!_rows.TryGetValue(item, out var peer)) _rows[item] = peer = new DataGridItemAutomationPeer(item, this);
		return peer;
	}

	internal DataGridColumnHeaderAutomationPeer HeaderPeer(int column)
	{
		if (!_headers.TryGetValue(column, out var peer))
			_headers[column] = peer = new DataGridColumnHeaderAutomationPeer(column, this);
		return peer;
	}

	internal List<DataGridColumnHeaderAutomationPeer> HeaderPeers()
	{
		var result = new List<DataGridColumnHeaderAutomationPeer>();
		for (int i = 0; i < Grid.AutomationColumns.Count; i++) result.Add(HeaderPeer(i));
		return result;
	}

	// The column headers, then one peer per row.
	protected override List<AutomationPeer>? GetChildrenCore()
	{
		var result = new List<AutomationPeer>(HeaderPeers());
		var live = new HashSet<object>();
		foreach (var item in Grid.Items)
			if (item != null && live.Add(item)) result.Add(RowPeer(item));
		foreach (var stale in new List<object>(_rows.Keys))
			if (!live.Contains(stale)) _rows.Remove(stale);
		return result.Count == 0 ? null : result;
	}

	int IGridProvider.RowCount => Grid.Items.Count;

	int IGridProvider.ColumnCount => Grid.AutomationColumns.Count;

	IRawElementProviderSimple IGridProvider.GetItem(int row, int column)
	{
		if (row < 0 || row >= Grid.Items.Count) throw new ArgumentOutOfRangeException(nameof(row));
		if (column < 0 || column >= Grid.AutomationColumns.Count) throw new ArgumentOutOfRangeException(nameof(column));
		var item = Grid.Items[row] ?? throw new ArgumentOutOfRangeException(nameof(row));
		return ProviderFromPeer(RowPeer(item).CellPeer(column));
	}

	RowOrColumnMajor ITableProvider.RowOrColumnMajor => RowOrColumnMajor.RowMajor;

	IRawElementProviderSimple[] ITableProvider.GetColumnHeaders()
	{
		var headers = HeaderPeers();
		var result = new IRawElementProviderSimple[headers.Count];
		for (int i = 0; i < result.Length; i++) result[i] = ProviderFromPeer(headers[i]);
		return result;
	}

	// The layer has row headers of none.
	IRawElementProviderSimple[] ITableProvider.GetRowHeaders() => new IRawElementProviderSimple[0];

	bool ISelectionProvider.CanSelectMultiple => false;

	bool ISelectionProvider.IsSelectionRequired => false;

	IRawElementProviderSimple[] ISelectionProvider.GetSelection() =>
		Grid.AutomationRowIndex(Grid.SelectedItem) >= 0
			? new[] { ProviderFromPeer(RowPeer(Grid.SelectedItem!)) } : new IRawElementProviderSimple[0];

	internal override void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property != DataGrid.SelectedItemProperty) return;
		if (e.OldValue != null && Grid.AutomationRowIndex(e.NewValue) < 0
			&& _rows.TryGetValue(e.OldValue, out var old))
			old.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection);
		if (Grid.AutomationRowIndex(e.NewValue) >= 0)
			RowPeer(e.NewValue!).RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
	}
}
