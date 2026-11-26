using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>The async paginator calls complete through the dispatcher.</summary>
public class PaginatorAsyncGtkTests : WpfGtkTestBase
{
	static DynamicDocumentPaginator Paginator()
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(new Paragraph(new Run("one two three")));
		return (DynamicDocumentPaginator)((IDocumentPaginatorSource)doc).DocumentPaginator;
	}

	// Posted work runs from the real GLib loop, as under Application.Run.
	void Hosted(Action body) => Run(() =>
	{
		using (new GtkDispatcherScope()) body();
	});

	// Waits for the posted completion rather than a fixed time, which a slow runner can outlast.
	static void Drain(Func<bool> done) => GtkPump.Until(done);

	[Fact]
	public void GetPageAsync_completes_later_with_the_page_and_state() => Hosted(() =>
	{
		var paginator = Paginator();
		GetPageCompletedEventArgs? done = null;
		paginator.GetPageCompleted += (s, e) => done = e;
		paginator.GetPageAsync(0, "state");
		Assert.Null(done);
		Drain(() => done != null);
		Assert.NotNull(done);
		Assert.Equal("state", done!.UserState);
		Assert.NotNull(done.DocumentPage);
	});

	[Fact]
	public void GetPageAsync_without_state_uses_the_paginator() => Hosted(() =>
	{
		var paginator = Paginator();
		object? state = null;
		paginator.GetPageCompleted += (s, e) => state = e.UserState;
		paginator.GetPageAsync(0);
		Drain(() => state != null);
		Assert.Same(paginator, state);
	});

	[Fact]
	public void CancelAsync_completes_the_call_as_cancelled() => Hosted(() =>
	{
		var paginator = Paginator();
		GetPageCompletedEventArgs? done = null;
		paginator.GetPageCompleted += (s, e) => done = e;
		paginator.GetPageAsync(0, "x");
		paginator.CancelAsync("x");
		Drain(() => done != null);
		Assert.True(done!.Cancelled);
	});

	[Fact]
	public void ComputePageCountAsync_completes_and_validates_the_count() => Hosted(() =>
	{
		var paginator = Paginator();
		paginator.IsBackgroundPaginationEnabled = false;
		paginator.PageSize = new Size(300, 300);
		AsyncCompletedEventArgs? done = null;
		paginator.ComputePageCountCompleted += (s, e) => done = e;
		paginator.ComputePageCountAsync();
		Drain(() => done != null);
		Assert.NotNull(done);
		Assert.True(paginator.IsPageCountValid);
	});

	[Fact]
	public void Background_pagination_validates_the_count_after_a_resize() => Hosted(() =>
	{
		var paginator = Paginator();
		bool completed = false;
		paginator.PaginationCompleted += (s, e) => completed = true;
		paginator.PageSize = new Size(300, 300);
		Assert.False(paginator.IsPageCountValid);
		Drain(() => completed);
		Assert.True(completed);
		Assert.True(paginator.IsPageCountValid);
	});

	[Fact]
	public void GetPageNumberAsync_completes_with_the_page_number() => Hosted(() =>
	{
		var paginator = Paginator();
		GetPageNumberCompletedEventArgs? done = null;
		paginator.GetPageNumberCompleted += (s, e) => done = e;
		paginator.GetPageNumberAsync(ContentPosition.Missing);
		Drain(() => done != null);
		Assert.Equal(0, done!.PageNumber);
	});
}
