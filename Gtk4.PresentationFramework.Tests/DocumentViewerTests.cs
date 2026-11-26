using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class DocumentViewerTests : WpfGtkTestBase
{
	[Fact]
	public void Defaults_match_wpf() => Run(() =>
	{
		var v = new DocumentViewer();
		Assert.Equal(100, v.Zoom);
		Assert.True(v.ShowPageBorders);
		Assert.Equal(10, v.VerticalPageSpacing);
		Assert.Equal(10, v.HorizontalPageSpacing);
		Assert.Equal(1, v.MaxPagesAcross);
		Assert.Null(v.Document);
	});

	[Fact]
	public void Invalid_values_are_rejected() => Run(() =>
	{
		var v = new DocumentViewer();
		Assert.Throws<ArgumentException>(() => v.MaxPagesAcross = 0);
		Assert.Throws<ArgumentException>(() => v.VerticalPageSpacing = -1);
		Assert.Throws<ArgumentException>(() => v.Zoom = 0);
	});

	[Fact]
	public void Zoom_is_kept_between_5_and_5000() => Run(() =>
	{
		var v = new DocumentViewer { Zoom = 100000 };
		Assert.Equal(5000, v.Zoom);
		v.Zoom = 1;
		Assert.Equal(5, v.Zoom);
	});

	[Fact]
	public void FitToMaxPagesAcross_sets_the_row_size_and_validates() => Run(() =>
	{
		var v = new DocumentViewer();
		v.FitToMaxPagesAcross(3);
		Assert.Equal(3, v.MaxPagesAcross);
		Assert.Throws<ArgumentOutOfRangeException>(() => v.FitToMaxPagesAcross(0));
		v.ViewThumbnails();
		Assert.Equal(4, v.MaxPagesAcross);
	});

	[Fact]
	public void Fit_commands_need_a_document() => Run(() =>
	{
		var v = new DocumentViewer();
		Assert.False(DocumentViewer.FitToWidthCommand.CanExecute(null, v));
		v.Document = new FlowDocument();
		Assert.True(DocumentViewer.FitToWidthCommand.CanExecute(null, v));
		Assert.True(DocumentViewer.ViewThumbnailsCommand.CanExecute(null, v));
	});

	[Fact]
	public void ViewThumbnails_command_executes() => Run(() =>
	{
		var v = new DocumentViewer { Document = new FlowDocument() };
		DocumentViewer.ViewThumbnailsCommand.Execute(null, v);
		Assert.Equal(4, v.MaxPagesAcross);
	});

	[Fact]
	public void A_fixed_style_paginator_renders_every_page_visual_into_the_box() => Run(() =>
	{
		var v = new DocumentViewer { Document = new FixedSource(3), MaxPagesAcross = 2 };
		Assert.Equal(3, v.PageCount);
		Assert.NotNull(v.Widget);
		v.ShowPageBorders = false;
		v.MaxPagesAcross = 1;
		v.NextPage();
		Assert.Equal(2, v.MasterPageNumber);
	});

	[Fact]
	public void A_flow_document_pages_into_the_viewer() => Run(() =>
	{
		var doc = new FlowDocument { PageWidth = 200, PageHeight = 100 };
		for (int i = 0; i < 30; i++) doc.Blocks.Add(new Paragraph(new Run("line " + i)));
		var v = new DocumentViewer { Document = doc };
		Assert.True(v.PageCount > 1);
		Assert.NotNull(v.Widget);
	});

	sealed class FixedSource : DocumentPaginator, IDocumentPaginatorSource
	{
		readonly int _pages;

		internal FixedSource(int pages) => _pages = pages;

		public override bool IsPageCountValid => true;

		public override int PageCount => _pages;

		public override Size PageSize { get; set; } = new Size(100, 100);

		public override IDocumentPaginatorSource Source => this;

		DocumentPaginator IDocumentPaginatorSource.DocumentPaginator => this;

		public override DocumentPage GetPage(int pageNumber)
		{
			var visual = new DrawingVisual();
			using (var dc = visual.RenderOpen())
				dc.Context.DrawRectangle(Brushes.Red, null, new Rect(0, 0, 50, 50));
			return new DocumentPage(visual, PageSize, new Rect(0, 0, 100, 100), new Rect(0, 0, 100, 100));
		}
	}
}
