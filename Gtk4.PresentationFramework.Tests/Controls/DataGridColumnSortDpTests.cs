using System.ComponentModel;
using Xunit;
using WpfDataGrid           = System.Windows.Controls.DataGrid;
using WpfDataGridTextColumn = System.Windows.Controls.DataGridTextColumn;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// DP round-trip tests for the DataGrid/DataGridColumn sort properties.
/// </summary>
public class DataGridColumnSortDpTests
{
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
}
