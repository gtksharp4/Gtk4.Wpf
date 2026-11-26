using System.ComponentModel;
using Xunit;
using WpfDataGrid           = System.Windows.Controls.DataGrid;
using WpfDataGridTextColumn = System.Windows.Controls.DataGridTextColumn;
using WpfBinding            = System.Windows.Data.Binding;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Tests for the <c>DataGrid.ToggleSort</c> seam: direction cycle, sibling clearing,
/// the sort gates, and item reordering. Pure (no realized GTK widget needed).
/// </summary>
public class DataGridToggleSortTests : DataGridSortTestBase
{
	[Fact]
	public void ToggleSort_first_toggle_sets_Ascending()
	{
		var dg  = new WpfDataGrid();
		var col = new WpfDataGridTextColumn { Binding = new WpfBinding("Age") };
		dg.Columns.Add(col);
		dg.ItemsSource = new[] { new Person("A", 1) };

		dg.ToggleSort(col);

		Assert.Equal(ListSortDirection.Ascending, col.SortDirection);
	}

	[Fact]
	public void ToggleSort_second_toggle_sets_Descending()
	{
		var dg  = new WpfDataGrid();
		var col = new WpfDataGridTextColumn { Binding = new WpfBinding("Age") };
		dg.Columns.Add(col);
		dg.ItemsSource = new[] { new Person("A", 1) };

		dg.ToggleSort(col);
		dg.ToggleSort(col);

		Assert.Equal(ListSortDirection.Descending, col.SortDirection);
	}

	[Fact]
	public void ToggleSort_third_toggle_cycles_back_to_Ascending()
	{
		var dg  = new WpfDataGrid();
		var col = new WpfDataGridTextColumn { Binding = new WpfBinding("Age") };
		dg.Columns.Add(col);
		dg.ItemsSource = new[] { new Person("A", 1) };

		dg.ToggleSort(col);
		dg.ToggleSort(col);
		dg.ToggleSort(col);

		Assert.Equal(ListSortDirection.Ascending, col.SortDirection);
	}

	[Fact]
	public void ToggleSort_clears_sibling_column_direction()
	{
		var dg   = new WpfDataGrid();
		var colA = new WpfDataGridTextColumn { Binding = new WpfBinding("Name") };
		var colB = new WpfDataGridTextColumn { Binding = new WpfBinding("Age")  };
		dg.Columns.Add(colA);
		dg.Columns.Add(colB);
		dg.ItemsSource = new[] { new Person("A", 1) };

		dg.ToggleSort(colA);  // A = Ascending
		dg.ToggleSort(colB);  // B = Ascending; A should be cleared

		Assert.Null(colA.SortDirection);
		Assert.Equal(ListSortDirection.Ascending, colB.SortDirection);
	}

	[Fact]
	public void ToggleSort_with_CanUserSortColumns_false_does_nothing()
	{
		var dg  = new WpfDataGrid { CanUserSortColumns = false };
		var col = new WpfDataGridTextColumn { Binding = new WpfBinding("Age") };
		dg.Columns.Add(col);
		dg.ItemsSource = new[] { new Person("A", 1) };

		dg.ToggleSort(col);

		Assert.Null(col.SortDirection);
	}

	[Fact]
	public void ToggleSort_with_CanUserSort_false_does_nothing()
	{
		var dg  = new WpfDataGrid();
		var col = new WpfDataGridTextColumn { Binding = new WpfBinding("Age"), CanUserSort = false };
		dg.Columns.Add(col);
		dg.ItemsSource = new[] { new Person("A", 1) };

		dg.ToggleSort(col);

		Assert.Null(col.SortDirection);
	}

	[Fact]
	public void ToggleSort_ascending_reorders_Items()
	{
		var dg  = new WpfDataGrid();
		var col = new WpfDataGridTextColumn { Binding = new WpfBinding("Age") };
		dg.Columns.Add(col);
		var items = new[] { new Person("Bob", 30), new Person("Alice", 20), new Person("Charlie", 25) };
		dg.ItemsSource = items;

		dg.ToggleSort(col); // Ascending by Age

		Assert.Equal(20, ((Person)dg.Items[0]!).Age);
		Assert.Equal(25, ((Person)dg.Items[1]!).Age);
		Assert.Equal(30, ((Person)dg.Items[2]!).Age);
	}

	[Fact]
	public void ToggleSort_descending_reorders_Items()
	{
		var dg  = new WpfDataGrid();
		var col = new WpfDataGridTextColumn { Binding = new WpfBinding("Age") };
		dg.Columns.Add(col);
		var items = new[] { new Person("Bob", 30), new Person("Alice", 20), new Person("Charlie", 25) };
		dg.ItemsSource = items;

		dg.ToggleSort(col); // Ascending
		dg.ToggleSort(col); // Descending by Age

		Assert.Equal(30, ((Person)dg.Items[0]!).Age);
		Assert.Equal(25, ((Person)dg.Items[1]!).Age);
		Assert.Equal(20, ((Person)dg.Items[2]!).Age);
	}

	[Fact]
	public void ToggleSort_uses_SortMemberPath_over_binding_path()
	{
		var dg  = new WpfDataGrid();
		var col = new WpfDataGridTextColumn
		{
			Binding        = new WpfBinding("Name"),  // binding path
			SortMemberPath = "Age",                   // sort by Age instead
		};
		dg.Columns.Add(col);
		var items = new[] { new Person("Bob", 30), new Person("Alice", 20), new Person("Charlie", 25) };
		dg.ItemsSource = items;

		dg.ToggleSort(col); // Ascending by Age (not Name)

		Assert.Equal(20, ((Person)dg.Items[0]!).Age);
	}
}
