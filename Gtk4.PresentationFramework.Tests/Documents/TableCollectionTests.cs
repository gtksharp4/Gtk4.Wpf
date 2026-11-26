using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class TableCollectionTests
{
	[Theory]
	[InlineData(typeof(TableRowGroupCollection))]
	[InlineData(typeof(TableRowCollection))]
	[InlineData(typeof(TableCellCollection))]
	[InlineData(typeof(TableColumnCollection))]
	public void Table_collections_are_sealed_lists_over_object(Type type)
	{
		Assert.Equal(typeof(object), type.BaseType);
		Assert.True(typeof(IList).IsAssignableFrom(type));
		Assert.True(type.IsSealed);
	}

	[Fact]
	public void Rows_work_through_IList_and_IList_of_T_and_track_parents()
	{
		var group = new TableRowGroup();
		var generic = (System.Collections.Generic.IList<TableRow>)group.Rows;
		var plain = (IList)group.Rows;
		var a = new TableRow();
		var b = new TableRow();
		var c = new TableRow();

		generic.Add(a);
		plain.Add(b);
		generic.Insert(0, c);

		Assert.Equal(new[] { c, a, b }, group.Rows);
		Assert.Same(group, a.Parent);
		Assert.Equal(1, plain.IndexOf(a));
		Assert.True(plain.Contains(b));
		Assert.Throws<InvalidOperationException>(() => new TableRowGroup().Rows.Add(a));
		Assert.Throws<ArgumentException>(() => plain.Add("x"));

		generic[0] = new TableRow();
		Assert.Null(c.Parent);
		group.Rows.Remove(a);
		Assert.Null(a.Parent);
		plain.Remove(b);
		group.Rows.Clear();
		Assert.Empty(group.Rows);
		Assert.Null(b.Parent);
	}

	[Fact]
	public void Columns_and_cells_track_parents()
	{
		var table = new Table();
		var col = new TableColumn();
		table.Columns.Add(col);
		Assert.Same(table, col.Parent);
		table.Columns.RemoveAt(0);
		Assert.Null(col.Parent);

		var row = new TableRow();
		var cell = new TableCell();
		row.Cells.Add(cell);
		Assert.Same(row, cell.Parent);
	}

	[Fact]
	public void Name_is_backed_by_NameProperty_on_content_elements()
	{
		var run = new Run();
		Assert.Equal(string.Empty, run.GetValue(FrameworkContentElement.NameProperty));
		run.Name = "x";
		Assert.Equal("x", run.GetValue(FrameworkContentElement.NameProperty));
		run.SetValue(FrameworkContentElement.NameProperty, "y");
		Assert.Equal("y", run.Name);
	}

	[Fact]
	public void TextAlignment_is_inherited_by_a_nested_cell()
	{
		var outer = new TableCell { TextAlignment = TextAlignment.Right };
		var table = new Table();
		var group = new TableRowGroup();
		var row = new TableRow();
		var inner = new TableCell();
		row.Cells.Add(inner);
		group.Rows.Add(row);
		table.RowGroups.Add(group);
		outer.Blocks.Add(table);

		Assert.Equal(TextAlignment.Right, inner.TextAlignment);
	}

	[Fact]
	public void GridLength_itself_rejects_negative_widths()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new TableColumn { Width = new GridLength(-1) });
	}

	[Fact]
	public void FlowDocumentScrollViewer_shares_the_ScrollViewer_visibility_properties()
	{
		Assert.Same(ScrollViewer.VerticalScrollBarVisibilityProperty,
			FlowDocumentScrollViewer.VerticalScrollBarVisibilityProperty);
		Assert.Same(ScrollViewer.HorizontalScrollBarVisibilityProperty,
			FlowDocumentScrollViewer.HorizontalScrollBarVisibilityProperty);
		Assert.Equal(ScrollBarVisibility.Visible,
			FlowDocumentScrollViewer.VerticalScrollBarVisibilityProperty.GetMetadata(typeof(FlowDocumentScrollViewer))
				.DefaultValue);
	}
}
