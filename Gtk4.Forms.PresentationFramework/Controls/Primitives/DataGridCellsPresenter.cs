using System.Collections.Generic;

namespace System.Windows.Controls.Primitives;

/// <summary>
/// Holds the <see cref="DataGridCell"/>s of one <see cref="DataGridRow"/>, one per effective column. The cells are
/// generated eagerly (no GTK widget needed) and are placed into the grid's GTK layout by the
/// <see cref="DataGrid"/> itself, so this presenter has no widget of its own.
/// </summary>
public class DataGridCellsPresenter : ItemsControl
{
	readonly List<DataGridCell> _cells = new List<DataGridCell>();
	List<DataGridColumn> _columns = new List<DataGridColumn>();
	ItemContainerGenerator? _generator;

	/// <summary>Gets the row item the presenter shows.</summary>
	public object? Item { get; internal set; }

	/// <summary>Gets the generator that maps the columns to their cells.</summary>
	public new ItemContainerGenerator ItemContainerGenerator => _generator ??= new ItemContainerGenerator(new CellHost(this));

	internal IReadOnlyList<DataGridCell> Cells => _cells;

	/// <inheritdoc/>
	internal override IEnumerable<UIElement> VisualChildren => _cells;

	/// <summary>Replaces the cells with one per column of <paramref name="columns"/>.</summary>
	internal void Realize(DataGrid grid, IReadOnlyList<DataGridColumn> columns)
	{
		foreach (var old in _cells) old.LogicalParent = null;
		_cells.Clear();
		_columns = new List<DataGridColumn>(columns);
		foreach (var column in columns)
		{
			var cell = new DataGridCell { Column = column, RowItem = Item, LogicalParent = this };
			cell.IsReadOnly = column.IsReadOnly || grid.IsReadOnly;
			var style = column.CellStyle ?? grid.CellStyle;
			if (style != null) cell.Style = style;
			else cell.ApplyImplicitStyle();
			cell.Content = new TextBlock { Text = column.GetCellText(Item) };
			cell.GotKeyboardFocus += (s, e) => grid.CurrentCell = new DataGridCellInfo(cell);
			_cells.Add(cell);
		}
	}


	sealed class CellHost : IContainerHost
	{
		readonly DataGridCellsPresenter _owner;

		internal CellHost(DataGridCellsPresenter owner) => _owner = owner;

		public GeneratorStatus Status => GeneratorStatus.ContainersGenerated;

		public int ItemCount => _owner._cells.Count;

		public object? ItemAt(int index) => _owner._columns[index];

		public int IndexOfItem(object item) => item is DataGridColumn c ? _owner._columns.IndexOf(c) : -1;

		public DependencyObject? ContainerAt(int index) => _owner._cells[index];

		public int IndexOfContainer(DependencyObject container) =>
			container is DataGridCell cell ? _owner._cells.IndexOf(cell) : -1;
	}
}
