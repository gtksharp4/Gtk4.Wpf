using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a row of a <see cref="DataGrid"/>.</summary>
public class DataGridItemAutomationPeer : GridPartAutomationPeer, ISelectionItemProvider
{
	readonly Dictionary<int, DataGridCellItemAutomationPeer> _cells =
		new Dictionary<int, DataGridCellItemAutomationPeer>();

	internal DataGridItemAutomationPeer(object item, DataGridAutomationPeer grid) : base(grid)
	{
		Item = item;
	}

	/// <summary>The data item the row shows.</summary>
	public object Item { get; }

	DataGridAutomationPeer Peer => (DataGridAutomationPeer)GridPeer;

	internal int RowIndex => Peer.Grid.AutomationRowIndex(Item);

	internal DataGridCellItemAutomationPeer CellPeer(int column)
	{
		if (!_cells.TryGetValue(column, out var cell))
			_cells[column] = cell = new DataGridCellItemAutomationPeer(this, column);
		return cell;
	}

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.SelectionItem ? this : null;

	protected override string GetClassNameCore() => "DataGridRow";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.DataItem;

	protected override string GetNameCore() => Item.ToString() ?? string.Empty;

	protected override List<AutomationPeer>? GetChildrenCore()
	{
		int count = Peer.Grid.AutomationColumns.Count;
		if (count == 0) return null;
		var result = new List<AutomationPeer>(count);
		for (int i = 0; i < count; i++) result.Add(CellPeer(i));
		return result;
	}

	internal override Gtk.Widget? PartWidget => Peer.Grid.AutomationCellWidget(RowIndex, 0);

	// A row spans its cells; the first cell's top-left and the last cell's right edge bound it.
	protected override Rect GetBoundingRectangleCore()
	{
		int count = Peer.Grid.AutomationColumns.Count;
		var first = PeerGeometry.Bounds(Peer.Grid.AutomationCellWidget(RowIndex, 0));
		var last = PeerGeometry.Bounds(Peer.Grid.AutomationCellWidget(RowIndex, count - 1));
		if (first.IsEmpty || last.IsEmpty) return first;
		return new Rect(first.X, first.Y, last.X + last.Width - first.X, Math.Max(first.Height, last.Height));
	}

	bool ISelectionItemProvider.IsSelected => Equals(Peer.Grid.SelectedItem, Item);

	IRawElementProviderSimple ISelectionItemProvider.SelectionContainer => ProviderFromPeer(GridPeer);

	void ISelectionItemProvider.Select()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Peer.Grid.AutomationSelectRow(Item);
	}

	void ISelectionItemProvider.AddToSelection()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		var selected = Peer.Grid.SelectedItem;
		if (selected != null && !Equals(selected, Item))
			throw new InvalidOperationException("The grid allows a single selection.");
		Peer.Grid.AutomationSelectRow(Item);
	}

	void ISelectionItemProvider.RemoveFromSelection()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (Equals(Peer.Grid.SelectedItem, Item)) Peer.Grid.AutomationSelectRow(null);
	}
}

/// <summary>The automation peer of a cell of a <see cref="DataGrid"/>; the cell text is read-only.</summary>
public class DataGridCellItemAutomationPeer : GridPartAutomationPeer, IGridItemProvider, ITableItemProvider,
	IValueProvider
{
	readonly DataGridItemAutomationPeer _row;
	readonly int _column;

	internal DataGridCellItemAutomationPeer(DataGridItemAutomationPeer row, int column) : base(row.GridPeer)
	{
		_row = row;
		_column = column;
	}

	DataGrid Grid => ((DataGridAutomationPeer)GridPeer).Grid;

	internal override AutomationPeer? ResolveParent() => _row;

	internal override Gtk.Widget? PartWidget => Grid.AutomationCellWidget(_row.RowIndex, _column);

	public override object? GetPattern(PatternInterface patternInterface)
	{
		switch (patternInterface)
		{
			case PatternInterface.GridItem:
			case PatternInterface.TableItem:
			case PatternInterface.Value: return this;
			default: return null;
		}
	}

	protected override string GetClassNameCore() => "DataGridCell";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.DataItem;

	protected override string GetNameCore() => Text;

	string Text => Grid.AutomationColumns[_column].GetCellText(_row.Item);

	int IGridItemProvider.Row => _row.RowIndex;

	int IGridItemProvider.Column => _column;

	int IGridItemProvider.RowSpan => 1;

	int IGridItemProvider.ColumnSpan => 1;

	IRawElementProviderSimple IGridItemProvider.ContainingGrid => ProviderFromPeer(GridPeer);

	IRawElementProviderSimple[] ITableItemProvider.GetColumnHeaderItems() =>
		new[] { ProviderFromPeer(((DataGridAutomationPeer)GridPeer).HeaderPeer(_column)) };

	IRawElementProviderSimple[] ITableItemProvider.GetRowHeaderItems() => new IRawElementProviderSimple[0];

	bool IValueProvider.IsReadOnly => true;

	string IValueProvider.Value => Text;

	void IValueProvider.SetValue(string value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		if (!IsEnabled()) throw new ElementNotEnabledException();
		throw new InvalidOperationException("The grid does not support editing cells.");
	}
}

/// <summary>The automation peer of a column header of a <see cref="DataGrid"/>.</summary>
public class DataGridColumnHeaderAutomationPeer : GridPartAutomationPeer
{
	readonly int _column;

	internal DataGridColumnHeaderAutomationPeer(int column, DataGridAutomationPeer grid) : base(grid)
	{
		_column = column;
	}

	internal override Gtk.Widget? PartWidget => ((DataGridAutomationPeer)GridPeer).Grid.AutomationHeaderWidget(_column);

	public override object? GetPattern(PatternInterface patternInterface) => null;

	protected override string GetClassNameCore() => "DataGridColumnHeader";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.HeaderItem;

	protected override string GetNameCore() =>
		((DataGridAutomationPeer)GridPeer).Grid.AutomationColumns[_column].Header?.ToString() ?? string.Empty;
}
