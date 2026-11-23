using System;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

public class DataGridColumnWidthTests : WpfGtkTestBase
{
	static DataGridLength Star(double v) => new DataGridLength(v, DataGridLengthUnitType.Star);

	[Fact]
	public void Dependency_properties_have_WPF_names_types_and_defaults() => Run(() =>
	{
		Assert.Equal("ColumnWidth", DataGrid.ColumnWidthProperty.Name);
		Assert.Equal(typeof(DataGridLength), DataGrid.ColumnWidthProperty.PropertyType);
		Assert.Equal(typeof(DataGrid), DataGrid.ColumnWidthProperty.OwnerType);
		Assert.Equal(typeof(DataGridLength), DataGridColumn.WidthProperty.PropertyType);
		var grid = new DataGrid();
		Assert.Equal(DataGridLength.SizeToHeader, grid.ColumnWidth);
		Assert.Equal(20.0, grid.MinColumnWidth);
		Assert.Equal(double.PositiveInfinity, grid.MaxColumnWidth);
		Assert.Equal(DataGridLength.Auto, new DataGridTextColumn().Width);
	});

	[Fact]
	public void Min_and_Max_validate() => Run(() =>
	{
		var grid = new DataGrid();
		Assert.Throws<ArgumentException>(() => grid.MinColumnWidth = -1);
		Assert.Throws<ArgumentException>(() => grid.MinColumnWidth = double.PositiveInfinity);
		Assert.Throws<ArgumentException>(() => grid.MinColumnWidth = double.NaN);
		Assert.Throws<ArgumentException>(() => grid.MaxColumnWidth = -1);
		grid.MaxColumnWidth = double.PositiveInfinity;
		grid.MinColumnWidth = 0;
		grid.MaxColumnWidth = 100;
		Assert.Equal(100, grid.MaxColumnWidth);
	});

	[Fact]
	public void Column_width_follows_grid_ColumnWidth_until_set_locally() => Run(() =>
	{
		var grid = new DataGrid();
		var col = new DataGridTextColumn();
		grid.Columns.Add(col);
		Assert.Equal(DataGridLength.SizeToHeader, col.Width);
		grid.ColumnWidth = Star(1);
		Assert.True(col.Width.IsStar);
		col.Width = new DataGridLength(50);
		Assert.Equal(new DataGridLength(50), col.Width);
		grid.ColumnWidth = DataGridLength.SizeToCells;
		Assert.Equal(new DataGridLength(50), col.Width);
		col.ClearValue(DataGridColumn.WidthProperty);
		Assert.Equal(DataGridLength.SizeToCells, col.Width);
	});

	[Fact]
	public void Joining_and_leaving_a_grid_recoerces_width() => Run(() =>
	{
		var grid = new DataGrid { ColumnWidth = Star(2) };
		var col = new DataGridTextColumn();
		Assert.Equal(DataGridLength.Auto, col.Width);
		grid.Columns.Add(col);
		Assert.Equal(Star(2), col.Width);
		grid.Columns.Remove(col);
		Assert.Equal(DataGridLength.Auto, col.Width);
	});

	[Fact]
	public void Star_column_header_expands_and_pixel_column_requests_its_width() => Run(() =>
	{
		var grid = new DataGrid { AutoGenerateColumns = false };
		grid.Columns.Add(new DataGridTextColumn { Header = "A", Width = Star(1) });
		var pixel = new DataGridTextColumn { Header = "B", Width = new DataGridLength(120) };
		grid.Columns.Add(pixel);
		grid.Columns.Add(new DataGridTextColumn { Header = "C", Width = DataGridLength.Auto });
		grid.ItemsSource = new[] { "x" };
		_ = grid.Widget;
		var native = grid._grid!;
		Assert.True(native.GetChildAt(0, 0)!.Hexpand);
		Assert.False(native.GetChildAt(1, 0)!.Hexpand);
		Assert.False(native.GetChildAt(2, 0)!.Hexpand);
		Assert.Equal(120, WidthRequest(native.GetChildAt(1, 0)!));
		pixel.Width = DataGridLength.Auto;
		Assert.Equal(-1, WidthRequest(native.GetChildAt(1, 0)!));
		grid.Columns[2].ClearValue(DataGridColumn.WidthProperty);
		grid.ColumnWidth = Star(1);
		Assert.True(native.GetChildAt(2, 0)!.Hexpand);
	});

	static int WidthRequest(Gtk.Widget widget)
	{
		widget.GetSizeRequest(out var w, out _);
		return w;
	}
}
