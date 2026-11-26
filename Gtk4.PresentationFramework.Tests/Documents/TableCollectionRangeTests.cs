using System;
using System.Collections;
using System.Linq;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class TableCollectionRangeTests
{
	[Fact]
	public void AddRange_RemoveRange_and_capacity_work_and_track_parents()
	{
		var row = new TableRow();
		var cells = new[] { new TableCell(), new TableCell(), new TableCell() };
		row.Cells.AddRange(cells);
		Assert.Equal(3, row.Cells.Count);
		Assert.All(cells, c => Assert.Same(row, c.Parent));

		row.Cells.RemoveRange(1, 2);
		Assert.Single(row.Cells);
		Assert.Null(cells[1].Parent);
		Assert.Null(cells[2].Parent);

		row.Cells.Capacity = 8;
		Assert.Equal(8, row.Cells.Capacity);
		row.Cells.TrimToSize();
		Assert.Equal(1, row.Cells.Capacity);
		Assert.Throws<ArgumentNullException>(() => row.Cells.AddRange(null!));
		Assert.Throws<ArgumentException>(() => row.Cells.RemoveRange(0, 5));
	}

	[Fact]
	public void Synchronisation_members_and_CopyTo_are_public_on_every_collection()
	{
		var group = new TableRowGroup();
		group.Rows.AddRange(new[] { new TableRow(), new TableRow() });
		Assert.False(group.Rows.IsSynchronized);
		Assert.NotNull(group.Rows.SyncRoot);
		var copy = new object[2];
		group.Rows.CopyTo(copy, 0);
		Assert.Equal(group.Rows.Cast<object>(), copy);

		var table = new Table();
		table.RowGroups.AddRange(new[] { new TableRowGroup() });
		table.Columns.AddRange(new[] { new TableColumn(), new TableColumn() });
		table.Columns.RemoveRange(0, 1);
		Assert.Single(table.Columns);
		Assert.Single(table.RowGroups);
	}

	[Fact]
	public void TrimToSize_shrinks_even_when_nearly_full()
	{
		var row = new TableRow();
		row.Cells.Capacity = 10;
		for (int i = 0; i < 9; i++) row.Cells.Add(new TableCell());
		row.Cells.TrimToSize();
		Assert.Equal(9, row.Cells.Capacity);
	}
}
