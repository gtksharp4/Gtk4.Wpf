namespace System.Windows.Controls;

/// <summary>Identifies a cell of a <see cref="DataGrid"/> by its row item and column.</summary>
public struct DataGridCellInfo : IEquatable<DataGridCellInfo>
{
	readonly object? _item;
	readonly DataGridColumn? _column;

	/// <summary>Creates the info of <paramref name="cell"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="cell"/> is null.</exception>
	public DataGridCellInfo(DataGridCell cell)
	{
		if (cell == null) throw new ArgumentNullException(nameof(cell));
		_item = cell.RowItem;
		_column = cell.Column;
	}

	/// <summary>Creates the info of the cell of <paramref name="item"/> in <paramref name="column"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="column"/> is null.</exception>
	public DataGridCellInfo(object item, DataGridColumn column)
	{
		_item = item;
		_column = column ?? throw new ArgumentNullException(nameof(column));
	}

	/// <summary>The row item, or <see cref="DependencyProperty.UnsetValue"/> for an invalid info.</summary>
	public object Item => _column != null ? _item! : DependencyProperty.UnsetValue;

	/// <summary>The column, or null for an invalid info.</summary>
	public DataGridColumn? Column => _column;

	/// <summary>Whether the info names a cell.</summary>
	public bool IsValid => _column != null;

	/// <inheritdoc/>
	public bool Equals(DataGridCellInfo other) =>
		ReferenceEquals(_column, other._column) && Equals(_item, other._item);

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is DataGridCellInfo other && Equals(other);

	/// <inheritdoc/>
	public override int GetHashCode() => (_column?.GetHashCode() ?? 0) ^ (_item?.GetHashCode() ?? 0);

	/// <summary>Whether two infos name the same cell.</summary>
	public static bool operator ==(DataGridCellInfo cell1, DataGridCellInfo cell2) => cell1.Equals(cell2);

	/// <summary>Whether two infos name different cells.</summary>
	public static bool operator !=(DataGridCellInfo cell1, DataGridCellInfo cell2) => !cell1.Equals(cell2);
}
