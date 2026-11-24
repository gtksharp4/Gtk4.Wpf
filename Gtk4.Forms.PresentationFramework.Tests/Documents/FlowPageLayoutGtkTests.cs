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

/// <summary>
/// The block-aware page layout of FlowDocumentPageViewer. Reported defect: the viewer put one wrapping label in a
/// scrolled window, so long lines ran off the right edge and every empty Paragraph added two blank lines.
/// </summary>
public partial class FlowPageLayoutGtkTests : WpfGtkTestBase
{
	const string Long = "The quick brown fox jumps over the lazy dog and keeps running through the forest " +
		"until the sun goes down behind the hills, while the farmers walk home along the dusty road.";

	static FlowDocument Doc(params string[] paragraphs)
	{
		var doc = new FlowDocument { PagePadding = new Thickness(20), Background = Brushes.White };
		foreach (var text in paragraphs) doc.Blocks.Add(text.Length == 0 ? new Paragraph() : new Paragraph(new Run(text)));
		return doc;
	}

	static FlowDocumentPageMeter Meter(FlowDocument doc, double w = 600, double h = 400) =>
		FlowDocumentPageMeter.Measure(doc, new Size(w, h));

	static List<FlowSlice> Slices(FlowDocumentPageMeter m) =>
		m.Pages.SelectMany(p => p.Columns).SelectMany(c => c.Slices).ToList();

	static (Window Win, FlowDocumentPageViewer Viewer, WidgetPixels Px) Show(FlowDocument doc, int page = 1)
	{
		var viewer = new FlowDocumentPageViewer { Document = doc };
		var win = new Window { Content = viewer, Width = 600, Height = 400 };
		win.Show();
		GtkPump.Until(() => viewer.Widget.IsMapped && viewer.PageWidget!.Width > 0);
		if (page > 1) viewer.GoToPage(page);
		GtkPump.For(300);
		return (win, viewer, WidgetPixels.Capture(viewer.PageWidget!));
	}

	static (int Min, int Max) InkColumns(WidgetPixels px, int fromY, int toY)
	{
		int min = int.MaxValue, max = -1;
		for (int y = fromY; y < Math.Min(toY, px.Height); y++)
			for (int x = 0; x < px.Width; x++)
				if (px.At(x, y).R < 128) { min = Math.Min(min, x); max = Math.Max(max, x); }
		return (min, max);
	}

	[Fact]
	public void An_empty_paragraph_between_two_paragraphs_collapses_to_one_line_height_gap() => Run(() =>
	{
		var m = Meter(Doc("one", string.Empty, "two"));
		var s = Slices(m);
		Assert.Equal(2, s.Count);
		double gap = s[1].Y - (s[0].Y + s[0].Height);
		Assert.Equal(m.Geometry.LineHeight, gap, 3);
	});

	[Fact]
	public void Adjacent_paragraph_margins_collapse_to_the_larger_one() => Run(() =>
	{
		var m = Meter(Doc("one", "two"));
		var s = Slices(m);
		Assert.Equal(m.Geometry.LineHeight, s[1].Y - (s[0].Y + s[0].Height), 3);
	});

	[Fact]
	public void Explicit_margins_are_honoured() => Run(() =>
	{
		var doc = Doc("one", "two");
		((Paragraph)doc.Blocks[0]).Margin = new Thickness(0, 0, 0, 30);
		((Paragraph)doc.Blocks[1]).Margin = new Thickness(0, 10, 0, 0);
		var s = Slices(Meter(doc));
		Assert.Equal(30, s[1].Y - (s[0].Y + s[0].Height), 3);
	});

	[Fact]
	public void Every_line_fits_the_column_for_one_two_and_three_columns() => Run(() =>
	{
		foreach (double columnWidth in new[] { double.NaN, 260, 170 })
		{
			var doc = Doc(Long, Long, Long);
			doc.ColumnWidth = columnWidth;
			var m = Meter(doc, 700, 500);
			Assert.True(m.Geometry.Columns >= 1);
			foreach (var box in m.Boxes)
			{
				Assert.True(box.TextWidth <= m.Geometry.ColumnWidth + 0.01);
				Assert.True(box.Lines.Count > 1, "the paragraph wrapped");
				foreach (var line in box.Lines) Assert.True(Measured(line, doc) <= box.TextWidth + 2, line);
			}
		}
	});

	static double Measured(string line, FlowDocument doc)
	{
		var layout = new Pango.Layout(GtkFontCatalog.FontMap!.CreateContext());
		using (var desc = new Pango.FontDescription { Family = doc.FontFamily.Source, AbsoluteSize = doc.FontSize * 1024.0 })
			layout.FontDescription = desc;
		layout.SetText(line);
		layout.GetPixelSize(out int w, out _);
		return w;
	}

	[Fact]
	public void ColumnWidth_200_with_gap_10_on_650_gives_three_flexible_columns_of_210() => Run(() =>
	{
		var doc = Doc(Long) ;
		doc.PagePadding = new Thickness(0);
		doc.ColumnWidth = 200;
		doc.ColumnGap = 10;
		var g = Meter(doc, 650, 400).Geometry;
		Assert.Equal(3, g.Columns);
		Assert.Equal(210, g.ColumnWidth, 3);
		doc.IsColumnWidthFlexible = false;
		g = Meter(doc, 650, 400).Geometry;
		Assert.Equal(3, g.Columns);
		Assert.Equal(200, g.ColumnWidth, 3);
	});

	[Fact]
	public void Default_column_gap_is_one_line_height_and_default_column_width_is_20_font_sizes() => Run(() =>
	{
		var doc = Doc(Long);
		doc.PagePadding = new Thickness(0);
		var g = Meter(doc, 1000, 400).Geometry;
		Assert.Equal(g.LineHeight, g.ColumnGap, 3);
		Assert.Equal((int)((1000 + g.LineHeight) / (20 * doc.FontSize + g.LineHeight)), g.Columns);
		Assert.True(g.Columns > 1, $"{g.Columns} columns, size {doc.FontSize}");
	});

	[Fact]
	public void Auto_page_padding_is_one_line_height_per_side() => Run(() =>
	{
		var doc = new FlowDocument();
		var g = Meter(doc).Geometry;
		Assert.Equal(g.LineHeight, g.Padding.Left, 3);
		Assert.Equal(g.LineHeight, g.Padding.Bottom, 3);
	});

	[Fact]
	public void PageCount_grows_with_font_size_and_shrinks_with_page_height() => Run(() =>
	{
		var doc = Doc(Long, Long, Long, Long);
		int normal = Meter(doc, 400, 200).PageCount;
		doc.FontSize *= 2;
		int doubled = Meter(doc, 400, 200).PageCount;
		doc.FontSize /= 2;
		int taller = Meter(doc, 400, 800).PageCount;
		Assert.True(doubled > normal, $"{normal} -> {doubled}");
		Assert.True(taller < normal, $"{normal} -> {taller}");
	});

	[Fact]
	public void Min_and_Max_page_size_clamp_and_Min_wins() => Run(() =>
	{
		var doc = Doc(Long);
		doc.PageWidth = 1000;
		doc.MaxPageWidth = 500;
		Assert.Equal(500, FlowPageGeometry.PageSize(doc, new Size(0, 0)).Width);
		doc.MinPageWidth = 700;
		Assert.Equal(700, FlowPageGeometry.PageSize(doc, new Size(0, 0)).Width);
		doc.PageWidth = double.NaN;
		doc.MinPageWidth = 0;
		doc.MaxPageWidth = double.PositiveInfinity;
		Assert.Equal(333, FlowPageGeometry.PageSize(doc, new Size(333, 10)).Width);
	});

	[Fact]
	public void Align_and_indent_apply_per_paragraph() => Run(() =>
	{
		var doc = Doc("short", "short");
		((Paragraph)doc.Blocks[0]).TextAlignment = TextAlignment.Center;
		var boxes = Meter(doc).Boxes;
		Assert.Equal(TextAlignment.Center, boxes[0].Alignment);
		Assert.Equal(TextAlignment.Left, boxes[1].Alignment);
	});
}
