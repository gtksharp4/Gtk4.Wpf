using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class PrintDialogTests : WpfGtkTestBase
{
	[Fact]
	public void Defaults_match_wpf() => Run(() =>
	{
		var d = new PrintDialog();
		Assert.Equal(PageRangeSelection.AllPages, d.PageRangeSelection);
		Assert.False(d.UserPageRangeEnabled);
		Assert.Equal(1u, d.MinPage);
		Assert.Equal(9999u, d.MaxPage);
		Assert.True(d.PrintableAreaWidth > 0);
		Assert.True(d.PrintableAreaHeight > 0);
	});

	[Fact]
	public void PageRange_stores_its_value() => Run(() =>
	{
		var d = new PrintDialog { PageRange = new PageRange(2, 4), PageRangeSelection = PageRangeSelection.UserPages };
		Assert.Equal(new PageRange(2, 4), d.PageRange);
	});

	[Fact]
	public void PageRange_equality_covers_both_ends()
	{
		Assert.Equal(new PageRange(1, 3), new PageRange(1, 3));
		Assert.True(new PageRange(1, 3) == new PageRange(1, 3));
		Assert.True(new PageRange(1, 3) != new PageRange(1, 4));
		Assert.False(new PageRange(1, 3).Equals((object)"x"));
		Assert.Equal(new PageRange(5, 5), new PageRange(5));
		Assert.Equal(new PageRange(1, 3).GetHashCode(), new PageRange(1, 3).GetHashCode());
	}

	[Fact]
	public void PageRangeSelection_has_the_four_values()
	{
		Assert.Equal(0, (int)PageRangeSelection.AllPages);
		Assert.Equal(1, (int)PageRangeSelection.UserPages);
		Assert.True(Enum.IsDefined(typeof(PageRangeSelection), PageRangeSelection.CurrentPage));
		Assert.True(Enum.IsDefined(typeof(PageRangeSelection), PageRangeSelection.SelectedPages));
	}

	[Fact]
	public void SelectPages_covers_all_pages_or_the_clamped_user_range() => Run(() =>
	{
		var d = new PrintDialog();
		Assert.Equal(new[] { 1, 2, 3 }, d.SelectPages(3));
		d.PageRangeSelection = PageRangeSelection.UserPages;
		d.PageRange = new PageRange(2, 9);
		Assert.Equal(new[] { 2, 3 }, d.SelectPages(3));
		d.PageRange = new PageRange(4, 2);
		Assert.Equal(new[] { 2, 3 }, d.SelectPages(3));
		d.PageRange = new PageRange(5, 6);
		Assert.Empty(d.SelectPages(3));
	});

	[Fact]
	public void Print_methods_reject_null_arguments() => Run(() =>
	{
		var d = new PrintDialog();
		Assert.Throws<ArgumentNullException>(() => d.PrintVisual(null!, "x"));
		Assert.Throws<ArgumentNullException>(() => d.PrintDocument(null!, "x"));
	});

	[Fact]
	public void ShowDialog_reports_the_choice_of_the_dialog() => Run(() =>
	{
		var old = PrintDialog.Presenter;
		try
		{
			PrintDialog.Presenter = _ => null;
			Assert.False(new PrintDialog().ShowDialog());
		}
		finally { PrintDialog.Presenter = old; }
	});

	[Fact]
	public void PrintVisual_exports_a_one_page_pdf() => Run(() =>
	{
		var file = Path.Combine(Path.GetTempPath(), "gf52b-" + Guid.NewGuid().ToString("N") + ".pdf");
		try
		{
			var visual = new DrawingVisual();
			using (var dc = visual.RenderOpen())
				dc.Context.DrawRectangle(Brushes.Black, null, new Rect(10, 10, 100, 100));
			new PrintDialog { ExportFilename = file }.PrintVisual(visual, "test");
			AssertPdf(file);
		}
		finally { if (File.Exists(file)) File.Delete(file); }
	});

	[Fact]
	public void PrintDocument_runs_the_paginator_page_loop_into_the_pdf() => Run(() =>
	{
		var file = Path.Combine(Path.GetTempPath(), "gf52b-" + Guid.NewGuid().ToString("N") + ".pdf");
		try
		{
			var doc = new FlowDocument { PageWidth = 300, PageHeight = 200 };
			for (int i = 0; i < 30; i++) doc.Blocks.Add(new Paragraph(new Run("paragraph " + i)));
			var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
			Assert.True(paginator.PageCount > 1);
			new PrintDialog { ExportFilename = file }.PrintDocument(paginator, "doc");
			AssertPdf(file);
			Assert.True(new FileInfo(file).Length > 500);
		}
		finally { if (File.Exists(file)) File.Delete(file); }
	});

	static void AssertPdf(string file)
	{
		Assert.True(File.Exists(file), "the export wrote no file");
		var head = new byte[4];
		using (var s = File.OpenRead(file)) s.ReadExactly(head, 0, 4);
		Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(head));
	}
}
