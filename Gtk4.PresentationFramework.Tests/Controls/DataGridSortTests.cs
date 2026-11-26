using System.ComponentModel;
using Xunit;
using WpfDataGridSort = System.Windows.Controls.DataGridSort;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no-GTK) tests for the <c>DataGridSort.SortItems</c> helper.
/// </summary>
public class DataGridSortTests : DataGridSortTestBase
{
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
}
