using System;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

public class DataGridCellInfoTests
{
	sealed class Row { public string Name { get; set; } = "a"; }

	static DataGridTextColumn Column() => new DataGridTextColumn { Header = "Name" };

	[Fact]
	public void The_default_info_is_invalid()
	{
		var info = default(DataGridCellInfo);
		Assert.False(info.IsValid);
		Assert.Null(info.Column);
		Assert.Same(DependencyProperty.UnsetValue, info.Item);
	}

	[Fact]
	public void The_item_and_column_ctor_stores_both()
	{
		var item = new Row();
		var column = Column();
		var info = new DataGridCellInfo(item, column);
		Assert.True(info.IsValid);
		Assert.Same(item, info.Item);
		Assert.Same(column, info.Column);
	}

	[Fact]
	public void Null_arguments_throw_ArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new DataGridCellInfo((DataGridCell)null!));
		Assert.Throws<ArgumentNullException>(() => new DataGridCellInfo(null!, Column()));
		Assert.Throws<ArgumentNullException>(() => new DataGridCellInfo(new Row(), null!));
	}

	[Fact]
	public void The_cell_ctor_takes_the_cells_row_item_and_column()
	{
		var item = new Row();
		var column = Column();
		var cell = new DataGridCell { Column = column, RowItem = item };
		var info = new DataGridCellInfo(cell);
		Assert.Same(item, info.Item);
		Assert.Same(column, info.Column);
	}

	[Fact]
	public void Equality_compares_item_and_column_by_reference()
	{
		var item = new Row();
		var column = Column();
		var a = new DataGridCellInfo(item, column);
		var b = new DataGridCellInfo(item, column);
		Assert.True(a == b);
		Assert.False(a != b);
		Assert.True(a.Equals(b));
		Assert.True(a.Equals((object)b));
		Assert.Equal(a.GetHashCode(), b.GetHashCode());
		Assert.True(a != new DataGridCellInfo(new Row(), column));
		Assert.True(a != new DataGridCellInfo(item, Column()));
		Assert.False(a.Equals("x"));
		Assert.True(default(DataGridCellInfo) == default(DataGridCellInfo));
	}
}
