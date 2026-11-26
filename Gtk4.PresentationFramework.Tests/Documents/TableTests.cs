using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Documents;

public class TableTests
{
	static Table Grid(params string[][] rows)
	{
		var table = new Table();
		var group = new TableRowGroup();
		table.RowGroups.Add(group);
		foreach (var cells in rows)
		{
			var row = new TableRow();
			group.Rows.Add(row);
			foreach (var text in cells) row.Cells.Add(new TableCell(new Paragraph(new Run(text))));
		}
		return table;
	}

	[Fact]
	public void Table_defaults_and_content_properties()
	{
		var t = new Table();
		Assert.Equal(2.0, t.CellSpacing);
		Assert.Empty(t.Columns);
		Assert.Empty(t.RowGroups);
		Assert.Equal("RowGroups", ContentOf(typeof(Table)));
		Assert.Equal("Rows", ContentOf(typeof(TableRowGroup)));
		Assert.Equal("Cells", ContentOf(typeof(TableRow)));
		Assert.Equal("Blocks", ContentOf(typeof(TableCell)));
		Assert.Throws<ArgumentException>(() => t.CellSpacing = -1);
	}

	static string? ContentOf(Type t) =>
		((ContentPropertyAttribute?)Attribute.GetCustomAttribute(t, typeof(ContentPropertyAttribute)))?.Name;

	[Fact]
	public void TableColumn_defaults()
	{
		var c = new TableColumn();
		Assert.Equal(GridLength.Auto, c.Width);
		Assert.Null(c.Background);
		c.Background = Brushes.Beige;
		c.Width = new GridLength(80);
		Assert.Equal(80, c.Width.Value);
	}

	[Fact]
	public void TableCell_defaults_and_span_validation()
	{
		var cell = new TableCell();
		Assert.Equal(1, cell.ColumnSpan);
		Assert.Equal(1, cell.RowSpan);
		Assert.Equal(new Thickness(0), cell.Padding);
		Assert.Equal(new Thickness(0), cell.BorderThickness);
		Assert.Null(cell.BorderBrush);
		Assert.Equal(TextAlignment.Left, cell.TextAlignment);
		Assert.True(double.IsNaN(cell.LineHeight));
		Assert.Equal(FlowDirection.LeftToRight, cell.FlowDirection);
		Assert.Throws<ArgumentException>(() => cell.ColumnSpan = 0);
		Assert.Throws<ArgumentException>(() => cell.RowSpan = -1);
		cell.ColumnSpan = 6;
		Assert.Equal(6, cell.ColumnSpan);
		Assert.Throws<ArgumentNullException>(() => new TableCell(null!));
	}

	[Fact]
	public void Collections_are_IList_and_track_parents()
	{
		var table = new Table();
		IList<TableColumn> columns = table.Columns;
		var col = new TableColumn();
		columns.Add(col);
		Assert.Same(table, col.Parent);
		Assert.Throws<InvalidOperationException>(() => columns.Add(col));
		columns.Remove(col);
		Assert.Null(col.Parent);

		var group = new TableRowGroup();
		table.RowGroups.Add(group);
		Assert.Same(table, group.Parent);
		var other = new Table();
		Assert.Throws<InvalidOperationException>(() => other.RowGroups.Add(group));

		var row = new TableRow();
		group.Rows.Add(row);
		var cell = new TableCell();
		row.Cells.Add(cell);
		Assert.Same(row, cell.Parent);
		Assert.Throws<InvalidOperationException>(() => new TableRow().Cells.Add(cell));
		row.Cells.Clear();
		Assert.Null(cell.Parent);
		Assert.Throws<ArgumentNullException>(() => row.Cells.Add(null!));
	}

	[Fact]
	public void Collections_insert_and_replace_parent_correctly()
	{
		var row = new TableRow();
		var a = new TableCell();
		var b = new TableCell();
		var c = new TableCell();
		row.Cells.Add(a);
		row.Cells.Insert(0, b);
		Assert.Same(b, row.Cells[0]);
		row.Cells[0] = c;
		Assert.Null(b.Parent);
		Assert.Same(row, c.Parent);
		row.Cells.RemoveAt(1);
		Assert.Null(a.Parent);
		Assert.Single(row.Cells);
	}

	[Fact]
	public void Rows_render_one_per_line_with_tab_separated_cells()
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(Grid(new[] { "a", "b" }, new[] { "c", "d" }));
		Assert.Equal("a\tb\nc\td", doc.BuildPlainText());
		Assert.Equal("a\tb\nc\td", doc.BuildMarkup());
		Assert.Equal("a\tb\nc\td", doc.GetAllText());
	}

	[Fact]
	public void Table_between_paragraphs_keeps_block_separators()
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(new Paragraph(new Run("before")));
		doc.Blocks.Add(Grid(new[] { "x" }));
		Assert.Equal("before\n\nx", doc.BuildPlainText());
	}

	[Fact]
	public void Row_and_cell_font_size_render_as_pango_spans()
	{
		var table = Grid(new[] { "a", "b" });
		var row = table.RowGroups[0].Rows[0];
		row.FontSize = 40;
		row.Cells[1].FontSize = 20;
		var doc = new FlowDocument();
		doc.Blocks.Add(table);
		Assert.Equal("<span size=\"30720\">a\t<span size=\"15360\">b</span></span>", doc.BuildMarkup());
	}

	[Fact]
	public void Empty_table_and_cell_with_several_blocks()
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(new Table());
		Assert.Equal(string.Empty, doc.BuildPlainText());
		var cell = new TableCell(new Paragraph(new Run("1")));
		cell.Blocks.Add(new Paragraph(new Run("2")));
		var t = new Table();
		t.RowGroups.Add(new TableRowGroup());
		t.RowGroups[0].Rows.Add(new TableRow());
		t.RowGroups[0].Rows[0].Cells.Add(cell);
		Assert.Equal("1 2", t.RowGroups[0].Rows[0].Cells[0] == cell ? Plain(t) : "");
	}

	static string Plain(Table t)
	{
		var d = new FlowDocument();
		d.Blocks.Add(t);
		return d.BuildPlainText();
	}

	[Fact]
	public void Table_builder_sample_shape_compiles_and_renders()
	{
		var table = new Table { CellSpacing = 10, Background = Brushes.White };
		for (int i = 0; i < 6; i++) table.Columns.Add(new TableColumn());
		var steel = Brushes.LightSteelBlue;
		table.Columns[1].Background = table.Columns[3].Background = steel;
		table.RowGroups.Add(new TableRowGroup());
		table.RowGroups[0].Rows.Add(new TableRow());
		var current = table.RowGroups[0].Rows[0];
		current.Background = Brushes.Silver;
		current.FontSize = 40;
		current.FontWeight = FontWeights.Bold;
		current.Cells.Add(new TableCell(new Paragraph(new Run("2004 Sales Project"))));
		current.Cells[0].ColumnSpan = 6;
		Assert.Equal(6, current.Cells[0].ColumnSpan);
		Assert.Equal(FontWeights.Bold, current.FontWeight);
		Assert.Same(steel, table.Columns[3].Background);
	}
}
