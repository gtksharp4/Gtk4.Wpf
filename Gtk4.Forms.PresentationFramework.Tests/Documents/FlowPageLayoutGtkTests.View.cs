using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Gtk4.Forms.PresentationFramework.Tests.Appearance;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Documents;

/// <summary>Pixel tests of the drawn page: see <see cref="FlowPageLayoutGtkTests"/>.</summary>
public partial class FlowPageLayoutGtkTests
{
	[Fact]
	public void Drawn_text_stays_inside_the_page_padding() => Run(() =>
	{
		var (win, viewer, px) = Show(Doc(Long, Long));
		var (_, right) = InkColumns(px, 0, px.Height);
		double pageRight = viewer.PageWidget!.Width - 20;
		win.Close();
		Assert.True(right > 0 && right <= pageRight + 1, $"ink right edge {right}, column edge {pageRight}");
	});


	[Fact]
	public void PagePadding_moves_the_first_line_origin() => Run(() =>
	{
		var doc = Doc("Hello world");
		doc.PagePadding = new Thickness(60, 50, 20, 20);
		var (win, _, px) = Show(doc);
		int firstInkRow = -1, firstInkCol = int.MaxValue;
		for (int y = 0; y < px.Height && firstInkRow < 0; y++)
			for (int x = 0; x < px.Width; x++)
				if (px.At(x, y).R < 128) { firstInkRow = y; firstInkCol = Math.Min(firstInkCol, x); }
		win.Close();
		Assert.True(firstInkRow >= 50, $"first ink row {firstInkRow}");
		Assert.True(firstInkCol >= 59 && firstInkCol <= 75, $"first ink column {firstInkCol}");
	});

	[Fact]
	public void TextAlignment_Right_puts_line_edges_at_the_column_edge() => Run(() =>
	{
		var doc = Doc("Right aligned");
		doc.TextAlignment = TextAlignment.Right;
		var (win, viewer, px) = Show(doc);
		var (_, right) = InkColumns(px, 0, px.Height);
		var geo = ((FlowDocumentPaginator)((IDocumentPaginatorSource)doc).DocumentPaginator).Meter.Geometry;
		double edge = geo.ColumnX(0) + geo.ColumnWidth;
		win.Close();
		Assert.True(right >= edge - 6 && right <= edge + 1, $"ink right {right}, edge {edge}");
	});

	[Fact]
	public void NextPage_draws_different_text_and_the_page_count_matches_the_paginator() => Run(() =>
	{
		var doc = Doc(Long, Long, Long, Long, Long, Long, Long, Long, Long, Long, Long, Long);
		var (win, viewer, first) = Show(doc);
		var paginator = (FlowDocumentPaginator)((IDocumentPaginatorSource)doc).DocumentPaginator;
		Assert.True(viewer.PageCount > 1);
		Assert.Equal(paginator.PageCount, viewer.PageCount);
		viewer.NextPage();
		GtkPump.For(300);
		var second = WidgetPixels.Capture(viewer.PageWidget!);
		bool differs = false;
		for (int y = 0; y < first.Height && !differs; y += 2)
			for (int x = 0; x < first.Width; x += 2)
				if (first.At(x, y) != second.At(x, y)) { differs = true; break; }
		Assert.Equal(2, viewer.MasterPageNumber);
		win.Close();
		Assert.True(differs);
		Assert.False(viewer.CanGoToPage(viewer.PageCount + 1));
	});

	[Fact]
	public void The_viewer_viewport_is_the_page_wherever_PageWidth_is_NaN() => Run(() =>
	{
		var doc = Doc(Enumerable.Repeat(Long, 30).ToArray());
		var paginator = (FlowDocumentPaginator)((IDocumentPaginatorSource)doc).DocumentPaginator;
		paginator.Viewport = new Size(800, 600);
		int wide = paginator.PageCount;
		Assert.Equal(new Size(800, 600), paginator.PageSize);
		paginator.Viewport = new Size(300, 600);
		Assert.True(paginator.PageCount > wide, $"{wide} -> {paginator.PageCount}");
		doc.PageWidth = 500;
		Assert.Equal(500, paginator.PageSize.Width);
	});

	[Fact]
	public void The_column_rule_is_drawn_centred_in_the_gap_and_the_background_fills_the_page() => Run(() =>
	{
		var doc = Doc(Long, Long, Long, Long);
		doc.PageWidth = 580;
		doc.PagePadding = new Thickness(0);
		doc.ColumnWidth = 200;
		doc.ColumnGap = 40;
		doc.IsColumnWidthFlexible = false;
		doc.ColumnRuleWidth = 6;
		doc.ColumnRuleBrush = Brushes.Red;
		doc.Background = Brushes.Lime;
		var (win, viewer, px) = Show(doc);
		var g = ((FlowDocumentPaginator)((IDocumentPaginatorSource)doc).DocumentPaginator).Meter.Geometry;
		int pageLeft = (viewer.PageWidget!.Width - 580) / 2;
		int ruleX = pageLeft + (int)(g.ColumnX(0) + g.ColumnWidth + g.ColumnGap / 2);
		var rule = px.At(ruleX, px.Height / 2);
		var corner = px.At(pageLeft + 2, 2);
		win.Close();
		Assert.True(rule.R > 200 && rule.G < 40, $"{rule} at {ruleX}, columns {g.Columns}, gap {g.ColumnGap}");
		Assert.True(corner.G > 200 && corner.R < 40, corner.ToString());
		Assert.True(viewer.PageCount >= 1);
	});

}
