using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

public class DataGridContainerTests : WpfGtkTestBase
{
	sealed class Item
	{
		public string Type { get; set; } = "";
		public string Cost { get; set; } = "";
	}

	static DataGrid Grid(out Item[] items)
	{
		items = new[] { new Item { Type = "Taxi", Cost = "10" }, new Item { Type = "Meal", Cost = "20" } };
		var grid = new DataGrid { AutoGenerateColumns = false };
		grid.Columns.Add(new DataGridTextColumn { Header = "Type", Binding = new Binding("Type") });
		grid.Columns.Add(new DataGridTextColumn { Header = "Cost", Binding = new Binding("Cost") });
		grid.ItemsSource = items;
		return grid;
	}

	static DataGridCellsPresenter PresenterOf(DataGrid grid, int row) =>
		((DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(row)!).Presenter!;

	[Fact]
	public void ContainerFromIndex_returns_a_DataGridRow_for_the_item_without_a_widget()
	{
		var grid = Grid(out var items);
		var row = Assert.IsType<DataGridRow>(grid.ItemContainerGenerator.ContainerFromIndex(1));
		Assert.Same(items[1], row.Item);
		Assert.Same(items[1], row.DataContext);
		Assert.Same(row, grid.ItemContainerGenerator.ContainerFromIndex(1));
		Assert.Same(row, grid.ItemContainerGenerator.ContainerFromItem(items[1]));
		Assert.Equal(1, grid.ItemContainerGenerator.IndexFromContainer(row));
		Assert.Same(items[1], grid.ItemContainerGenerator.ItemFromContainer(row));
		Assert.Null(grid.ItemContainerGenerator.ContainerFromIndex(2));
		Assert.Null(grid.ItemContainerGenerator.ContainerFromIndex(-1));
	}

	[Fact]
	public void The_generator_has_the_same_type_as_the_ItemsControl_one()
	{
		var grid = Grid(out _);
		Assert.IsType<ItemContainerGenerator>(grid.ItemContainerGenerator);
		Assert.IsType<ItemContainerGenerator>(new ItemsControl().ItemContainerGenerator);
	}

	[Fact]
	public void First_child_descent_from_a_row_reaches_the_cells_presenter()
	{
		var grid = Grid(out var items);
		DependencyObject node = grid.ItemContainerGenerator.ContainerFromIndex(0)!;
		DataGridCellsPresenter? found = null;
		while (VisualTreeHelper.GetChildrenCount(node) > 0)
		{
			node = VisualTreeHelper.GetChild(node, 0);
			if (node is DataGridCellsPresenter p) { found = p; break; }
		}
		Assert.NotNull(found);
		Assert.Same(items[0], found!.Item);
	}

	[Fact]
	public void The_presenter_generator_returns_one_cell_per_column()
	{
		var grid = Grid(out var items);
		var presenter = PresenterOf(grid, 0);
		var first = Assert.IsType<DataGridCell>(presenter.ItemContainerGenerator.ContainerFromIndex(0));
		var second = Assert.IsType<DataGridCell>(presenter.ItemContainerGenerator.ContainerFromIndex(1));
		Assert.Same(grid.Columns[0], first.Column);
		Assert.Same(grid.Columns[1], second.Column);
		Assert.Null(presenter.ItemContainerGenerator.ContainerFromIndex(2));
		Assert.Equal(1, presenter.ItemContainerGenerator.IndexFromContainer(second));
		Assert.Same(grid.Columns[1], presenter.ItemContainerGenerator.ItemFromContainer(second));
		Assert.Same(items[0], first.RowItem);
	}

	[Fact]
	public void Cells_host_a_TextBlock_with_the_column_text()
	{
		var grid = Grid(out _);
		var cell = (DataGridCell)PresenterOf(grid, 1).ItemContainerGenerator.ContainerFromIndex(0)!;
		Assert.Equal("Meal", Assert.IsType<TextBlock>(cell.Content).Text);
	}

	[Fact]
	public void Auto_generated_columns_keep_their_identity_between_calls()
	{
		var grid = new DataGrid { ItemsSource = new[] { new Item { Type = "a" } } };
		var cell1 = PresenterOf(grid, 0).ItemContainerGenerator.ContainerFromIndex(0);
		var cell2 = PresenterOf(grid, 0).ItemContainerGenerator.ContainerFromIndex(0);
		Assert.NotNull(cell1);
		Assert.Same(cell1, cell2);
	}

	[Fact]
	public void Adding_a_column_or_changing_the_items_rebuilds_the_containers()
	{
		var grid = Grid(out var items);
		var row = grid.ItemContainerGenerator.ContainerFromIndex(0);
		grid.Columns.Add(new DataGridTextColumn { Header = "X", Binding = new Binding("Type") });
		var rebuilt = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(0)!;
		Assert.NotSame(row, rebuilt);
		Assert.NotNull(rebuilt.Presenter!.ItemContainerGenerator.ContainerFromIndex(2));
		grid.ItemsSource = new[] { items[1] };
		Assert.Same(items[1], ((DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(0)!).Item);
		Assert.Null(grid.ItemContainerGenerator.ContainerFromIndex(1));
	}

	[Fact]
	public void A_sort_reorders_the_row_containers() => Run(() =>
	{
		var grid = Grid(out var items);
		_ = grid.Widget;
		grid.ToggleSort(grid.Columns[0]);
		Assert.Same(items[1], ((DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(0)!).Item);
	});

	[Fact]
	public void The_ItemsControl_generator_still_maps_its_own_containers()
	{
		var list = new ItemsControl();
		var button = new Button();
		list.Items.Add(button);
		Assert.Same(button, list.ItemContainerGenerator.ContainerFromIndex(0));
		Assert.Equal(0, list.ItemContainerGenerator.IndexFromContainer(button));
		Assert.Same(button, list.ItemContainerGenerator.ItemFromContainer(button));
	}

	[Fact]
	public void The_realised_grid_hosts_the_cell_widgets_with_their_text() => Run(() =>
	{
		var grid = Grid(out _);
		Assert.NotNull(grid.Widget);
		var cell = (DataGridCell)PresenterOf(grid, 0).ItemContainerGenerator.ContainerFromIndex(1)!;
		Assert.Same(cell.Widget, grid.AutomationCellWidget(0, 1));
		Assert.Equal("10", ((Gtk.Label)((TextBlock)cell.Content!).Widget).Text);
	});
}
