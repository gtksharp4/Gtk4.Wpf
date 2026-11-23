using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

public partial class DataGrid
{
	readonly List<DataGridRow> _rowContainers = new List<DataGridRow>();
	readonly List<DataGridColumnHeader> _headerContainers = new List<DataGridColumnHeader>();
	List<DataGridColumn>? _autoColumns;
	List<DataGridColumn> _builtColumns = new List<DataGridColumn>();
	bool _containersDirty = true;
	ItemContainerGenerator? _generator;

	/// <summary>Creates a grid.</summary>
	public DataGrid() => Columns.CollectionChanged += (s, e) => _containersDirty = true;

	/// <summary>Gets the generator that maps the rows to their <see cref="DataGridRow"/> containers.</summary>
	public ItemContainerGenerator ItemContainerGenerator => _generator ??= new ItemContainerGenerator(new RowHost(this));

	internal IReadOnlyList<DataGridRow> RowContainers
	{
		get
		{
			EnsureContainers();
			return _rowContainers;
		}
	}

	internal IReadOnlyList<DataGridColumnHeader> HeaderContainers
	{
		get
		{
			EnsureContainers();
			return _headerContainers;
		}
	}

	bool ContainersCurrent(IReadOnlyList<DataGridColumn> columns)
	{
		if (_containersDirty || _rowContainers.Count != _rows.Count) return false;
		if (!_builtColumns.SequenceEqual(columns)) return false;
		for (int i = 0; i < _rows.Count; i++)
			if (!ReferenceEquals(_rowContainers[i].Item, _rows[i])) return false;
		return true;
	}

	// Builds one DataGridRow (with its cells presenter) per item and one header per column, unless they already
	// match the items and columns. Needs no GTK widget.
	void EnsureContainers()
	{
		var columns = GetEffectiveColumns();
		if (ContainersCurrent(columns)) return;
		foreach (var old in _rowContainers) old.LogicalParent = null;
		foreach (var old in _headerContainers) old.LogicalParent = null;
		_rowContainers.Clear();
		_headerContainers.Clear();
		foreach (var item in _rows)
			_rowContainers.Add(CreateRow(item, columns));
		foreach (var column in columns)
			_headerContainers.Add(CreateHeader(column));
		_builtColumns = new List<DataGridColumn>(columns);
		_containersDirty = false;
		_generator?.RaiseStatusChanged();
	}

	DataGridRow CreateRow(object? item, IReadOnlyList<DataGridColumn> columns)
	{
		var row = new DataGridRow { Item = item, LogicalParent = this };
		row.SetValue(DataContextProperty, item);
		var presenter = new DataGridCellsPresenter { Item = item, LogicalParent = row };
		row.Presenter = presenter;
		presenter.Realize(this, columns);
		return row;
	}

	// The column's HeaderStyle wins over ColumnHeaderStyle. The GTK grid still draws a label per header.
	DataGridColumnHeader CreateHeader(DataGridColumn column)
	{
		var header = new DataGridColumnHeader { Column = column, Content = column.Header, LogicalParent = this };
		var style = column.HeaderStyle ?? ColumnHeaderStyle;
		if (style != null) header.Style = style;
		else header.ApplyImplicitStyle();
		return header;
	}

	sealed class RowHost : IContainerHost
	{
		readonly DataGrid _owner;

		internal RowHost(DataGrid owner) => _owner = owner;

		public GeneratorStatus Status => GeneratorStatus.ContainersGenerated;

		public int ItemCount => _owner._rows.Count;

		public object? ItemAt(int index) => _owner._rows[index];

		public int IndexOfItem(object item) => _owner._rows.IndexOf(item);

		public DependencyObject? ContainerAt(int index) => _owner.RowContainers[index];

		public int IndexOfContainer(DependencyObject container) =>
			container is DataGridRow row ? _owner.RowContainers.ToList().IndexOf(row) : -1;
	}
}
