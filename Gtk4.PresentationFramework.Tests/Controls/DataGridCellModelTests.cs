using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class DataGridCellModelTests : WpfGtkTestBase
{
	sealed class Item { public string Type { get; set; } = "Taxi"; }

	static DataGrid Grid(Style? columnStyle = null, Style? gridStyle = null)
	{
		var grid = new DataGrid { AutoGenerateColumns = false, CellStyle = gridStyle };
		grid.Columns.Add(new DataGridTextColumn
		{
			Header = "Type", Binding = new Binding("Type"), CellStyle = columnStyle,
		});
		grid.ItemsSource = new[] { new Item() };
		return grid;
	}

	static DataGridCell CellOf(DataGrid grid) =>
		(DataGridCell)((DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(0)!)
			.Presenter!.ItemContainerGenerator.ContainerFromIndex(0)!;

	static Style Aligned(TextAlignment alignment)
	{
		var style = new Style(typeof(DataGridCell));
		style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, alignment));
		return style;
	}

	[Fact]
	public void Cell_defaults_match_WPF()
	{
		var cell = new DataGridCell();
		Assert.False(cell.IsSelected);
		Assert.False(cell.IsEditing);
		Assert.False(cell.IsReadOnly);
		Assert.Null(cell.Column);
		Assert.True(cell.Focusable);
		Assert.True(DataGridCell.ColumnProperty.ReadOnly);
		Assert.True(DataGridCell.IsReadOnlyProperty.ReadOnly);
		Assert.Throws<InvalidOperationException>(() => cell.SetValue(DataGridCell.ColumnProperty, null));
	}

	[Fact]
	public void The_column_cell_style_applies_and_TextAlignment_reaches_the_TextBlock()
	{
		var grid = Grid(columnStyle: Aligned(TextAlignment.Right));
		var cell = CellOf(grid);
		Assert.Equal(TextAlignment.Right, TextBlock.GetTextAlignment(cell));
		Assert.Equal(TextAlignment.Right, ((TextBlock)cell.Content!).TextAlignment);
	}

	[Fact]
	public void The_grid_cell_style_is_the_fallback_and_the_column_style_wins()
	{
		Assert.Equal(TextAlignment.Center,
			((TextBlock)CellOf(Grid(gridStyle: Aligned(TextAlignment.Center))).Content!).TextAlignment);
		Assert.Equal(TextAlignment.Right,
			((TextBlock)CellOf(Grid(Aligned(TextAlignment.Right), Aligned(TextAlignment.Center))).Content!)
				.TextAlignment);
	}

	[Fact]
	public void A_read_only_column_makes_the_cell_read_only()
	{
		var grid = Grid();
		grid.Columns[0].IsReadOnly = true;
		Assert.True(CellOf(grid).IsReadOnly);
	}

	[Fact]
	public void CellStyle_and_HeaderStyle_default_to_null()
	{
		var column = new DataGridTextColumn();
		Assert.Null(column.CellStyle);
		Assert.Null(column.HeaderStyle);
		Assert.Null(new DataGrid().CellStyle);
	}

	[Fact]
	public void The_header_style_of_the_column_wins_over_ColumnHeaderStyle()
	{
		var columnStyle = new Style(typeof(DataGridColumnHeader));
		columnStyle.Setters.Add(new Setter(FrameworkElement.TagProperty, "column"));
		var gridStyle = new Style(typeof(DataGridColumnHeader));
		gridStyle.Setters.Add(new Setter(FrameworkElement.TagProperty, "grid"));
		var grid = Grid();
		grid.ColumnHeaderStyle = gridStyle;
		Assert.Equal("grid", grid.HeaderContainers[0].Tag);
		grid.Columns[0].HeaderStyle = columnStyle;
		grid.Columns.Add(new DataGridTextColumn());
		Assert.Equal("column", grid.HeaderContainers[0].Tag);
		Assert.Equal("grid", grid.HeaderContainers[1].Tag);
		Assert.Same(grid.Columns[0], grid.HeaderContainers[0].Column);
	}

	[Fact]
	public void CanUserAddRows_defaults_true_and_a_read_only_grid_reports_false()
	{
		var grid = new DataGrid();
		Assert.True(grid.CanUserAddRows);
		grid.CanUserAddRows = false;
		Assert.False(grid.CanUserAddRows);
		grid.CanUserAddRows = true;
		grid.IsReadOnly = true;
		Assert.False(grid.CanUserAddRows);
		grid.IsReadOnly = false;
		Assert.True(grid.CanUserAddRows);
	}

	[Fact]
	public void A_null_item_cell_round_trips_and_raises_once()
	{
		var grid = Grid();
		int changed = 0;
		grid.CurrentCellChanged += (_, _) => changed++;
		var cell = new DataGridCellInfo(null!, grid.Columns[0]);
		grid.CurrentCell = cell;
		grid.CurrentCell = cell;
		Assert.True(grid.CurrentCell.IsValid);
		Assert.Equal(cell, grid.CurrentCell);
		Assert.Equal(1, changed);
	}

	[Fact]
	public void Clearing_Columns_keeps_the_auto_generated_columns_owned()
	{
		var grid = new DataGrid { AutoGenerateColumns = true, ItemsSource = new[] { new Item() } };
		var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
		var effective = (System.Collections.Generic.IReadOnlyList<DataGridColumn>)typeof(DataGrid)
			.GetMethod("GetEffectiveColumns", flags)!.Invoke(grid, null)!;
		var auto = effective[0];
		Assert.Same(grid, auto.DataGridOwner);
		grid.Columns.Clear();
		Assert.Same(grid, auto.DataGridOwner);
	}

	[Fact]
	public void CurrentCell_syncs_with_CurrentItem_and_CurrentColumn_and_raises_once()
	{
		var grid = Grid();
		var item = grid.Items[0]!;
		var column = grid.Columns[0];
		int changed = 0;
		grid.CurrentCellChanged += (_, _) => changed++;
		Assert.False(grid.CurrentCell.IsValid);

		grid.CurrentCell = new DataGridCellInfo(item, column);
		Assert.Equal(1, changed);
		Assert.Same(item, grid.CurrentItem);
		Assert.Same(column, grid.CurrentColumn);

		grid.CurrentCell = new DataGridCellInfo(item, column);
		Assert.Equal(1, changed);

		grid.CurrentColumn = null;
		Assert.Equal(2, changed);
		Assert.False(grid.CurrentCell.IsValid);

		grid.CurrentItem = item;
		grid.CurrentColumn = column;
		Assert.Equal(new DataGridCellInfo(item, column), grid.CurrentCell);
	}

	[Fact]
	public void Keyboard_Focus_on_a_cell_focuses_it_and_makes_it_the_current_cell() => Run(() =>
	{
		var grid = Grid();
		var cell = CellOf(grid);
		Keyboard.Focus(cell);
		Assert.Same(cell, Keyboard.FocusedElement);
		Assert.True(cell.IsKeyboardFocused);
		Assert.Equal(new DataGridCellInfo(cell), grid.CurrentCell);
		cell.SetFocusedInternal(false);
	});

	[Fact]
	public void Equal_but_distinct_boxed_items_raise_CurrentCellChanged_once() => Run(() =>
	{
		var grid = Grid();
		var column = grid.Columns[0];
		int changed = 0;
		grid.CurrentCellChanged += (_, _) => changed++;
		grid.CurrentCell = new DataGridCellInfo(5, column);
		grid.CurrentCell = new DataGridCellInfo(5, column);
		Assert.Equal(1, changed);
	});

	[Fact]
	public void The_presenter_generator_is_the_inherited_one_and_maps_columns_to_cells() => Run(() =>
	{
		var grid = new DataGrid { AutoGenerateColumns = false };
		grid.Columns.Add(new DataGridTextColumn { Header = "A", Binding = new Binding("Type") });
		grid.Columns.Add(new DataGridTextColumn { Header = "B", Binding = new Binding("Type") });
		grid.ItemsSource = new[] { new Item() };
		var row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(0)!;
		var presenter = row.Presenter!;
		var cell = Assert.IsType<DataGridCell>(((ItemsControl)presenter).ItemContainerGenerator.ContainerFromIndex(1));
		Assert.Same(grid.Columns[1], cell.Column);
		Assert.Null(typeof(DataGridCellsPresenter).GetProperty("ItemContainerGenerator",
			System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
			| System.Reflection.BindingFlags.DeclaredOnly));
	});

	static DataGridColumnHeader HeaderOf(DataGrid grid, int index) => grid.HeaderContainers[index];

	[Fact]
	public void Grid_and_column_cell_style_and_read_only_changes_reach_the_built_cell() => Run(() =>
	{
		var grid = Grid();
		var cell = CellOf(grid);
		var column = grid.Columns[0];
		var styleA = Aligned(TextAlignment.Right);
		var styleB = Aligned(TextAlignment.Center);
		grid.CellStyle = styleA;
		Assert.Same(styleA, cell.Style);
		column.CellStyle = styleB;
		Assert.Same(styleB, cell.Style);
		Assert.Same(cell, CellOf(grid));
		grid.IsReadOnly = true;
		Assert.True(cell.IsReadOnly);
		grid.IsReadOnly = false;
		Assert.False(cell.IsReadOnly);
		column.IsReadOnly = true;
		Assert.True(cell.IsReadOnly);
	});

	[Fact]
	public void Grid_and_column_header_style_changes_reach_the_built_header() => Run(() =>
	{
		var grid = Grid();
		var header = HeaderOf(grid, 0);
		var s = new Style(typeof(DataGridColumnHeader));
		var s2 = new Style(typeof(DataGridColumnHeader));
		grid.ColumnHeaderStyle = s;
		Assert.Same(s, header.Style);
		grid.Columns[0].HeaderStyle = s2;
		Assert.Same(s2, header.Style);
		Assert.Same(header, HeaderOf(grid, 0));
	});
}
