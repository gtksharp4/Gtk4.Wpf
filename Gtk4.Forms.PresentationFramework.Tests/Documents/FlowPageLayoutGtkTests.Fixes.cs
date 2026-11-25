using System;
using System.Windows;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Documents;

public partial class FlowPageLayoutGtkTests
{
	[Fact]
	public void Non_flexible_columns_keep_the_gap_and_leave_the_unused_width_on_the_right() => Run(() =>
	{
		var doc = Doc(Long);
		doc.PagePadding = new Thickness(0);
		doc.ColumnWidth = 200;
		doc.ColumnGap = 10;
		doc.IsColumnWidthFlexible = false;
		var g = Meter(doc, 650, 400).Geometry;
		Assert.Equal(3, g.Columns);
		Assert.Equal(200, g.ColumnWidth, 3);
		Assert.Equal(10, g.ColumnGap, 3);
		Assert.Equal(0, g.ColumnX(0), 3);
		Assert.Equal(210, g.ColumnX(1), 3);
		Assert.Equal(420, g.ColumnX(2), 3);
		doc.IsColumnWidthFlexible = true;
		g = Meter(doc, 650, 400).Geometry;
		Assert.Equal((650 - 20) / 3.0, g.ColumnWidth, 3);
		Assert.Equal(10, g.ColumnGap, 3);
	});

	[Fact]
	public void An_empty_paragraph_between_two_paragraphs_is_one_blank_line_between_collapsed_margins() => Run(() =>
	{
		var m = Meter(Doc("a", string.Empty, "b"));
		var s = Slices(m);
		Assert.Equal(3, s.Count);
		double lineHeight = m.Geometry.LineHeight;
		double pitch = m.Boxes[0].Pitch, blank = m.Boxes[1].Pitch;
		// The blank line is one line of its own pitch (the font's natural pitch may differ slightly from lineHeight).
		Assert.Equal(pitch + 2 * lineHeight + blank, s[2].Y - s[0].Y, 0.5);
		Assert.Equal(new[] { string.Empty }, m.Boxes[1].Lines);
		Assert.Equal(0, m.PageOfChar(m.Boxes[1].CharStart));
	});

	[Fact]
	public void An_empty_paragraph_on_a_later_page_reports_that_page() => Run(() =>
	{
		var doc = Doc("a", "b", "c", "d", "e", "f", string.Empty);
		var m = Meter(doc, 300, 90);
		Assert.True(m.PageCount > 1);
		var box = m.Boxes[6];
		int expected = m.Pages.FindIndex(p => p.Columns.Exists(c => c.Slices.Exists(s => s.Box == box)));
		Assert.True(expected > 0);
		Assert.Equal(expected, m.PageOfChar(box.CharStart));
	});

	[Fact]
	public void Explicit_zero_margins_remove_the_paragraph_spacing() => Run(() =>
	{
		var doc = Doc("a", "b");
		foreach (Paragraph p in doc.Blocks) p.Margin = new Thickness(0);
		var s = Slices(Meter(doc));
		Assert.Equal(s[0].Y + s[0].Height, s[1].Y, 3);
	});

	[Fact]
	public void Default_paragraph_margins_are_auto() => Run(() =>
	{
		var p = new Paragraph();
		Assert.True(double.IsNaN(p.Margin.Top) && double.IsNaN(p.Margin.Left));
		var m = Meter(Doc("a", "b"));
		Assert.Equal(m.Geometry.LineHeight, m.Boxes[0].Margin.Top, 3);
		Assert.Equal(0, m.Boxes[0].Margin.Left, 3);
	});

	[Fact]
	public void List_and_Table_without_a_margin_get_a_line_height_above_and_below_and_other_blocks_none() => Run(() =>
	{
		var doc = Doc("a");
		doc.Blocks.Add(new List());
		doc.Blocks.Add(new Table());
		doc.Blocks.Add(new Section());
		var m = Meter(doc);
		double lineHeight = m.Geometry.LineHeight;
		foreach (int i in new[] { 1, 2 })
		{
			Assert.Equal(lineHeight, m.Boxes[i].Margin.Top, 3);
			Assert.Equal(lineHeight, m.Boxes[i].Margin.Bottom, 3);
		}
		Assert.Equal(0, m.Boxes[3].Margin.Top, 3);
		Assert.Equal(0, m.Boxes[3].Margin.Bottom, 3);
	});
}
