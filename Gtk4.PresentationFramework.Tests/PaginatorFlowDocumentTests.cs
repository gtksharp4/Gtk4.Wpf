using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>The FlowDocument paginator: sizes, counts and pages.</summary>
public class PaginatorFlowDocumentTests
{
	static FlowDocument Doc(int paragraphs)
	{
		var doc = new FlowDocument();
		for (int i = 0; i < paragraphs; i++)
			doc.Blocks.Add(new Paragraph(new Run("Lorem ipsum dolor sit amet, consectetur adipiscing elit " + i)));
		return doc;
	}

	static DynamicDocumentPaginator Paginator(FlowDocument doc) =>
		Assert.IsAssignableFrom<DynamicDocumentPaginator>(((IDocumentPaginatorSource)doc).DocumentPaginator);

	[Fact]
	public void FlowDocument_exposes_a_dynamic_paginator_that_is_stable()
	{
		var doc = Doc(1);
		var paginator = Paginator(doc);
		Assert.Same(paginator, ((IDocumentPaginatorSource)doc).DocumentPaginator);
		Assert.Same(doc, paginator.Source);
	}

	[Fact]
	public void PageSize_defaults_to_the_document_page_size()
	{
		var doc = Doc(1);
		doc.PageWidth = 400;
		doc.PageHeight = 300;
		Assert.Equal(new Size(400, 300), Paginator(doc).PageSize);
	}

	[Fact]
	public void PageSize_falls_back_to_letter_when_the_document_has_none() =>
		Assert.Equal(new Size(816, 1056), Paginator(Doc(1)).PageSize);

	[Fact]
	public void IsBackgroundPaginationEnabled_defaults_to_true_and_is_stored()
	{
		var paginator = Paginator(Doc(1));
		Assert.True(paginator.IsBackgroundPaginationEnabled);
		paginator.IsBackgroundPaginationEnabled = false;
		Assert.False(paginator.IsBackgroundPaginationEnabled);
	}

	[Fact]
	public void A_smaller_page_needs_more_pages()
	{
		var paginator = Paginator(Doc(60));
		paginator.PageSize = new Size(800, 800);
		int large = paginator.PageCount;
		paginator.PageSize = new Size(200, 200);
		Assert.True(paginator.PageCount > large, $"{paginator.PageCount} should exceed {large}");
	}

	[Fact]
	public void An_empty_document_has_one_page() => Assert.Equal(1, Paginator(new FlowDocument()).PageCount);

	[Fact]
	public void Padding_and_columns_reduce_the_room_so_more_pages_are_needed()
	{
		var doc = Doc(40);
		var paginator = Paginator(doc);
		paginator.PageSize = new Size(600, 600);
		int plain = paginator.PageCount;
		doc.PagePadding = new Thickness(100);
		Assert.True(paginator.PageCount > plain);
	}

	[Fact]
	public void Changing_PageSize_invalidates_the_count_and_raises_PagesChanged()
	{
		var paginator = Paginator(Doc(5));
		paginator.IsBackgroundPaginationEnabled = false;
		_ = paginator.PageCount;
		Assert.True(paginator.IsPageCountValid);
		PagesChangedEventArgs? seen = null;
		paginator.PagesChanged += (s, e) => seen = e;
		paginator.PageSize = new Size(300, 300);
		Assert.False(paginator.IsPageCountValid);
		Assert.NotNull(seen);
		Assert.Equal(0, seen!.Start);
		_ = paginator.PageCount;
		Assert.True(paginator.IsPageCountValid);
	}

	[Fact]
	public void Setting_the_same_PageSize_changes_nothing()
	{
		var paginator = Paginator(Doc(5));
		paginator.PageSize = new Size(300, 300);
		_ = paginator.PageCount;
		paginator.PageSize = new Size(300, 300);
		Assert.True(paginator.IsPageCountValid);
	}

	[Fact]
	public void ComputePageCount_makes_the_count_valid()
	{
		var paginator = Paginator(Doc(5));
		paginator.IsBackgroundPaginationEnabled = false;
		paginator.PageSize = new Size(300, 300);
		paginator.ComputePageCount();
		Assert.True(paginator.IsPageCountValid);
	}

	[Fact]
	public void GetPage_returns_a_page_of_the_paginator_size_and_Missing_past_the_end()
	{
		var paginator = Paginator(Doc(3));
		paginator.PageSize = new Size(500, 700);
		var page = paginator.GetPage(0);
		Assert.Equal(new Size(500, 700), page.Size);
		Assert.NotNull(page.Visual);
		Assert.Same(DocumentPage.Missing, paginator.GetPage(paginator.PageCount));
		Assert.Throws<ArgumentOutOfRangeException>(() => paginator.GetPage(-1));
	}

	[Fact]
	public void Async_calls_reject_a_null_user_state()
	{
		var paginator = Paginator(Doc(1));
		Assert.Throws<ArgumentNullException>(() => paginator.GetPageAsync(0, null!));
		Assert.Throws<ArgumentNullException>(() => paginator.ComputePageCountAsync(null!));
		Assert.Throws<ArgumentNullException>(() => paginator.CancelAsync(null!));
		Assert.Throws<ArgumentNullException>(() => paginator.GetPageNumberAsync(ContentPosition.Missing, null!));
	}

	[Fact]
	public void GetPageNumber_of_Missing_is_the_first_page_and_null_throws()
	{
		var paginator = Paginator(Doc(1));
		Assert.Equal(0, paginator.GetPageNumber(ContentPosition.Missing));
		Assert.Throws<ArgumentNullException>(() => paginator.GetPageNumber(null!));
	}
}
