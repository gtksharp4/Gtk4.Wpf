using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

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
}
