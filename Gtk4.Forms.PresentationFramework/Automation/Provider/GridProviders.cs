namespace System.Windows.Automation.Provider;

/// <summary>Supports controls that lay out their items in a grid of rows and columns.</summary>
public interface IGridProvider
{
	int RowCount { get; }

	int ColumnCount { get; }

	IRawElementProviderSimple GetItem(int row, int column);
}

/// <summary>Supports the items of a grid.</summary>
public interface IGridItemProvider
{
	int Row { get; }

	int Column { get; }

	int RowSpan { get; }

	int ColumnSpan { get; }

	IRawElementProviderSimple ContainingGrid { get; }
}

/// <summary>Supports grids that have row and column headers.</summary>
public interface ITableProvider : IGridProvider
{
	RowOrColumnMajor RowOrColumnMajor { get; }

	IRawElementProviderSimple[] GetRowHeaders();

	IRawElementProviderSimple[] GetColumnHeaders();
}

/// <summary>Supports the items of a table.</summary>
public interface ITableItemProvider : IGridItemProvider
{
	IRawElementProviderSimple[] GetRowHeaderItems();

	IRawElementProviderSimple[] GetColumnHeaderItems();
}
