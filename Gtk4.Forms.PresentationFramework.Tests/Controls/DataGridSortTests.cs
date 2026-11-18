using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;
using WpfDataGrid           = System.Windows.Controls.DataGrid;
using WpfDataGridTextColumn = System.Windows.Controls.DataGridTextColumn;
using WpfBinding            = System.Windows.Data.Binding;
using WpfDataGridSort       = System.Windows.Controls.DataGridSort;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no-GTK) tests for DataGrid column-sort feature.
/// </summary>
public class DataGridSortTests
{
	// ── POCO used by several tests ────────────────────────────────────────────

	sealed class Person
	{
		public string Name { get; }
		public int    Age  { get; }
		public Person(string name, int age) { Name = name; Age = age; }
	}

	// ── DataGridSort.SortItems ────────────────────────────────────────────────

	[Fact]
	public void SortItems_ascending_by_int_property()
	{
		var items = new object?[] { new Person("Bob", 30), new Person("Alice", 20), new Person("Charlie", 25) };
		var result = WpfDataGridSort.SortItems(items, "Age", ListSortDirection.Ascending);
		Assert.Equal(20, ((Person)result[0]!).Age);
		Assert.Equal(25, ((Person)result[1]!).Age);
		Assert.Equal(30, ((Person)result[2]!).Age);
	}

	[Fact]
	public void SortItems_descending_by_int_property()
	{
		var items = new object?[] { new Person("Bob", 30), new Person("Alice", 20), new Person("Charlie", 25) };
		var result = WpfDataGridSort.SortItems(items, "Age", ListSortDirection.Descending);
		Assert.Equal(30, ((Person)result[0]!).Age);
		Assert.Equal(25, ((Person)result[1]!).Age);
		Assert.Equal(20, ((Person)result[2]!).Age);
	}

	[Fact]
	public void SortItems_ascending_by_string_property()
	{
		var items = new object?[] { new Person("Charlie", 1), new Person("Alice", 2), new Person("Bob", 3) };
		var result = WpfDataGridSort.SortItems(items, "Name", ListSortDirection.Ascending);
		Assert.Equal("Alice",   ((Person)result[0]!).Name);
		Assert.Equal("Bob",     ((Person)result[1]!).Name);
		Assert.Equal("Charlie", ((Person)result[2]!).Name);
	}

	[Fact]
	public void SortItems_stable_preserves_equal_key_order()
	{
		var a = new Person("Alice", 25);
		var b = new Person("Bob",   25);
		var items = new object?[] { a, b };
		var result = WpfDataGridSort.SortItems(items, "Age", ListSortDirection.Ascending);
		Assert.Same(a, result[0]);
		Assert.Same(b, result[1]);
	}

	[Fact]
	public void SortItems_null_items_sort_first()
	{
		var items = new object?[] { new Person("Alice", 1), null, new Person("Bob", 2) };
		var result = WpfDataGridSort.SortItems(items, "Age", ListSortDirection.Ascending);
		Assert.Null(result[0]);
	}

	[Fact]
	public void SortItems_empty_path_returns_original_order()
	{
		var items = new object?[] { new Person("Bob", 2), new Person("Alice", 1) };
		var result = WpfDataGridSort.SortItems(items, string.Empty, ListSortDirection.Ascending);
		// No sort applied; order unchanged.
		Assert.Equal("Bob",   ((Person)result[0]!).Name);
		Assert.Equal("Alice", ((Person)result[1]!).Name);
	}

	// ── DataGridColumn DPs ────────────────────────────────────────────────────

	[Fact]
	public void DataGridColumn_CanUserSort_default_is_true()
	{
		var col = new WpfDataGridTextColumn();
		Assert.True(col.CanUserSort);
	}

	[Fact]
	public void DataGridColumn_CanUserSort_roundtrip()
	{
		var col = new WpfDataGridTextColumn { CanUserSort = false };
		Assert.False(col.CanUserSort);
	}

	[Fact]
	public void DataGridColumn_SortMemberPath_default_is_null()
	{
		var col = new WpfDataGridTextColumn();
		Assert.Null(col.SortMemberPath);
	}

	[Fact]
	public void DataGridColumn_SortMemberPath_roundtrip()
	{
		var col = new WpfDataGridTextColumn { SortMemberPath = "LastName" };
		Assert.Equal("LastName", col.SortMemberPath);
	}

	[Fact]
	public void DataGridColumn_SortDirection_default_is_null()
	{
		var col = new WpfDataGridTextColumn();
		Assert.Null(col.SortDirection);
	}

	[Fact]
	public void DataGridColumn_SortDirection_roundtrip()
	{
		var col = new WpfDataGridTextColumn { SortDirection = ListSortDirection.Ascending };
		Assert.Equal(ListSortDirection.Ascending, col.SortDirection);
	}

	// ── DataGrid.CanUserSortColumns DP ────────────────────────────────────────

	[Fact]
	public void DataGrid_CanUserSortColumns_default_is_true()
	{
		var dg = new WpfDataGrid();
		Assert.True(dg.CanUserSortColumns);
	}

	[Fact]
	public void DataGrid_CanUserSortColumns_roundtrip()
	{
		var dg = new WpfDataGrid { CanUserSortColumns = false };
		Assert.False(dg.CanUserSortColumns);
	}

	// ── ToggleSort seam ───────────────────────────────────────────────────────

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

	// ── ToggleSort reorders Items ─────────────────────────────────────────────

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
