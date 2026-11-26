using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>The paginator object model: pages and event args.</summary>
public class PaginatorObjectModelTests
{
	[Fact]
	public void DocumentPage_Missing_has_a_zero_size_and_no_visual()
	{
		Assert.Equal(new Size(0, 0), DocumentPage.Missing.Size);
		Assert.Null(DocumentPage.Missing.Visual);
	}

	[Fact]
	public void DocumentPage_with_only_a_visual_has_empty_size_and_boxes()
	{
		var page = new DocumentPage(new DrawingVisual());
		Assert.Equal(Size.Empty, page.Size);
		Assert.True(page.BleedBox.IsEmpty);
		Assert.True(page.ContentBox.IsEmpty);
	}

	[Fact]
	public void DocumentPage_keeps_its_size_and_boxes()
	{
		var bleed = new Rect(0, 0, 100, 200);
		var content = new Rect(10, 10, 80, 180);
		var visual = new DrawingVisual();
		var page = new DocumentPage(visual, new Size(100, 200), bleed, content);
		Assert.Same(visual, page.Visual);
		Assert.Equal(bleed, page.BleedBox);
		Assert.Equal(content, page.ContentBox);
	}

	[Fact]
	public void DocumentPage_Dispose_raises_Disposed_once_and_releases_the_visual()
	{
		var page = new DocumentPage(new DrawingVisual());
		int raised = 0;
		page.Disposed += (s, e) => raised++;
		page.Dispose();
		page.Dispose();
		Assert.Equal(1, raised);
		Assert.Null(page.Visual);
	}

	[Fact]
	public void ContentPosition_Missing_is_a_shared_instance() =>
		Assert.Same(ContentPosition.Missing, ContentPosition.Missing);

	[Fact]
	public void PagesChangedEventArgs_carries_start_and_count()
	{
		var e = new PagesChangedEventArgs(2, 5);
		Assert.Equal(2, e.Start);
		Assert.Equal(5, e.Count);
	}

	[Fact]
	public void PagesChangedEventArgs_rejects_negative_values()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new PagesChangedEventArgs(-1, 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => new PagesChangedEventArgs(0, -1));
	}

	[Fact]
	public void PaginationProgressEventArgs_carries_start_and_count_and_rejects_negatives()
	{
		var e = new PaginationProgressEventArgs(1, 3);
		Assert.Equal(1, e.Start);
		Assert.Equal(3, e.Count);
		Assert.Throws<ArgumentOutOfRangeException>(() => new PaginationProgressEventArgs(-1, 0));
	}

	[Fact]
	public void GetPageCompletedEventArgs_throws_the_error_when_the_page_is_read()
	{
		var e = new GetPageCompletedEventArgs(null, 3, new InvalidOperationException("boom"), false, "s");
		Assert.Equal(3, e.PageNumber);
		Assert.Equal("s", e.UserState);
		Assert.Throws<System.Reflection.TargetInvocationException>(() => e.DocumentPage);
	}

	[Fact]
	public void GetPageNumberCompletedEventArgs_returns_values_when_it_succeeded()
	{
		var e = new GetPageNumberCompletedEventArgs(ContentPosition.Missing, 4, null, false, null);
		Assert.Same(ContentPosition.Missing, e.ContentPosition);
		Assert.Equal(4, e.PageNumber);
	}

}
