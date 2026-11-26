using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Navigation, page state and commands of the paginated viewers, on real controls.</summary>
public class DocumentViewerBaseTests : WpfGtkTestBase
{
	static FlowDocument Long()
	{
		var doc = new FlowDocument { PageWidth = 200, PageHeight = 120 };
		for (int i = 0; i < 40; i++)
			doc.Blocks.Add(new Paragraph(new Run("Lorem ipsum dolor sit amet consectetur " + i)));
		return doc;
	}

	static FlowDocumentPageViewer Viewer() => new FlowDocumentPageViewer { Document = Long() };

	static bool CanExecute(UIElement target, RoutedCommand command, object? parameter = null) =>
		command.CanExecute(parameter, target);

	[Fact]
	public void A_viewer_without_a_document_has_no_pages() => Run(() =>
	{
		var viewer = new FlowDocumentPageViewer();
		Assert.Equal(0, viewer.PageCount);
		Assert.Equal(0, viewer.MasterPageNumber);
		Assert.False(viewer.CanGoToNextPage);
		Assert.False(viewer.CanGoToPreviousPage);
		Assert.False(viewer.CanGoToPage(1));
	});

	[Fact]
	public void Setting_a_document_shows_page_one_of_its_paginator_count() => Run(() =>
	{
		var viewer = Viewer();
		var paginator = viewer.Document!.DocumentPaginator;
		Assert.True(viewer.PageCount > 1);
		Assert.Equal(paginator.PageCount, viewer.PageCount);
		Assert.Equal(1, viewer.MasterPageNumber);
		Assert.False(viewer.CanGoToPreviousPage);
		Assert.True(viewer.CanGoToNextPage);
	});

	[Fact]
	public void Navigation_methods_move_the_master_page() => Run(() =>
	{
		var viewer = Viewer();
		viewer.NextPage();
		Assert.Equal(2, viewer.MasterPageNumber);
		viewer.LastPage();
		Assert.Equal(viewer.PageCount, viewer.MasterPageNumber);
		Assert.False(viewer.CanGoToNextPage);
		Assert.True(viewer.CanGoToPreviousPage);
		viewer.PreviousPage();
		Assert.Equal(viewer.PageCount - 1, viewer.MasterPageNumber);
		viewer.FirstPage();
		Assert.Equal(1, viewer.MasterPageNumber);
		viewer.GoToPage(3);
		Assert.Equal(3, viewer.MasterPageNumber);
	});

	[Fact]
	public void Navigating_past_either_end_or_to_a_missing_page_is_ignored() => Run(() =>
	{
		var viewer = Viewer();
		viewer.PreviousPage();
		Assert.Equal(1, viewer.MasterPageNumber);
		viewer.GoToPage(0);
		viewer.GoToPage(viewer.PageCount + 1);
		Assert.Equal(1, viewer.MasterPageNumber);
		viewer.LastPage();
		viewer.NextPage();
		Assert.Equal(viewer.PageCount, viewer.MasterPageNumber);
	});

	[Fact]
	public void CanGoToPage_covers_one_to_PageCount() => Run(() =>
	{
		var viewer = Viewer();
		Assert.False(viewer.CanGoToPage(0));
		Assert.True(viewer.CanGoToPage(1));
		Assert.True(viewer.CanGoToPage(viewer.PageCount));
		Assert.False(viewer.CanGoToPage(viewer.PageCount + 1));
	});

	[Fact]
	public void Commands_can_execute_by_position() => Run(() =>
	{
		var viewer = Viewer();
		Assert.False(CanExecute(viewer, NavigationCommands.PreviousPage));
		Assert.False(CanExecute(viewer, NavigationCommands.FirstPage));
		Assert.True(CanExecute(viewer, NavigationCommands.NextPage));
		Assert.True(CanExecute(viewer, NavigationCommands.LastPage));
		viewer.LastPage();
		Assert.True(CanExecute(viewer, NavigationCommands.PreviousPage));
		Assert.True(CanExecute(viewer, NavigationCommands.FirstPage));
		Assert.False(CanExecute(viewer, NavigationCommands.NextPage));
		Assert.False(CanExecute(viewer, NavigationCommands.LastPage));
	});

	[Fact]
	public void Commands_execute_against_the_viewer() => Run(() =>
	{
		var viewer = Viewer();
		NavigationCommands.NextPage.Execute(null, viewer);
		Assert.Equal(2, viewer.MasterPageNumber);
		NavigationCommands.LastPage.Execute(null, viewer);
		Assert.Equal(viewer.PageCount, viewer.MasterPageNumber);
		NavigationCommands.FirstPage.Execute(null, viewer);
		Assert.Equal(1, viewer.MasterPageNumber);
	});

	[Fact]
	public void GoToPage_command_needs_an_existing_page_parameter() => Run(() =>
	{
		var viewer = Viewer();
		Assert.True(CanExecute(viewer, NavigationCommands.GoToPage, 2));
		Assert.False(CanExecute(viewer, NavigationCommands.GoToPage, 0));
		Assert.False(CanExecute(viewer, NavigationCommands.GoToPage, null));
		NavigationCommands.GoToPage.Execute(2, viewer);
		Assert.Equal(2, viewer.MasterPageNumber);
	});

	[Fact]
	public void PageViews_follow_the_master_page() => Run(() =>
	{
		var viewer = Viewer();
		var views = viewer.PageViews;
		Assert.Single(views);
		Assert.Equal(0, views[0].PageNumber);
		viewer.GoToPage(3);
		Assert.Equal(2, views[0].PageNumber);
		Assert.Same(viewer.Document!.DocumentPaginator, views[0].DocumentPaginator);
		Assert.NotSame(DocumentPage.Missing, views[0].DocumentPage);
	});

	[Fact]
	public void Replacing_the_document_resets_to_the_first_page() => Run(() =>
	{
		var viewer = Viewer();
		viewer.LastPage();
		viewer.Document = Long();
		Assert.Equal(1, viewer.MasterPageNumber);
		viewer.Document = null;
		Assert.Equal(0, viewer.PageCount);
		Assert.Equal(0, viewer.MasterPageNumber);
	});

	[Fact]
	public void A_page_size_change_refreshes_the_page_count() => Run(() =>
	{
		var viewer = Viewer();
		int before = viewer.PageCount;
		viewer.Document!.DocumentPaginator.PageSize = new Size(100, 60);
		Assert.True(viewer.PageCount > before);
	});

	[Fact]
	public void The_master_page_is_clamped_when_the_count_shrinks() => Run(() =>
	{
		var viewer = Viewer();
		viewer.LastPage();
		viewer.Document!.DocumentPaginator.PageSize = new Size(2000, 3000);
		Assert.Equal(viewer.PageCount, viewer.MasterPageNumber);
	});

	[Fact]
	public void A_flow_document_cannot_belong_to_two_viewers() => Run(() =>
	{
		var doc = Long();
		var first = new FlowDocumentPageViewer { Document = doc };
		Assert.Throws<InvalidOperationException>(() => new FlowDocumentPageViewer { Document = doc });
		Assert.Same(first, doc.Parent);
	});

	[Fact]
	public void MasterPageNumberChanged_hook_runs_only_on_a_change() => Run(() =>
	{
		var viewer = new Probe { Document = Long() };
		viewer.Changes = 0;
		viewer.GoToPage(1);
		Assert.Equal(0, viewer.Changes);
		viewer.GoToPage(2);
		Assert.Equal(1, viewer.Changes);
	});

	sealed class Probe : FlowDocumentPageViewer
	{
		internal int Changes;

		protected override void OnMasterPageNumberChanged() => Changes++;
	}
}
