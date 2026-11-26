using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Round 52 paginated-viewers review fixes (F1 to F10).</summary>
public class PaginatorReviewFixTests : WpfGtkTestBase
{
	sealed class MinimalPaginator : DynamicDocumentPaginator
	{
		public override bool IsPageCountValid => true;
		public override int PageCount => 1;
		public override Size PageSize { get; set; }
		public override IDocumentPaginatorSource Source => null!;
		public override DocumentPage GetPage(int pageNumber) => DocumentPage.Missing;
		public override int GetPageNumber(ContentPosition contentPosition) => 0;
		public override ContentPosition GetPagePosition(DocumentPage page) => ContentPosition.Missing;
		public override ContentPosition GetObjectPosition(object element) => ContentPosition.Missing;
	}

	sealed class OverridingPage : DocumentPage
	{
		internal OverridingPage() : base(new DrawingVisual())
		{
		}

		internal int Disposed2 { get; set; }

		public override void Dispose()
		{
			Disposed2++;
			base.Dispose();
		}
	}

	static FlowDocument Doc(int paragraphs)
	{
		var doc = new FlowDocument();
		for (int i = 0; i < paragraphs; i++)
			doc.Blocks.Add(new Paragraph(new Run("Lorem ipsum dolor sit amet, consectetur adipiscing elit " + i)));
		return doc;
	}

	static DynamicDocumentPaginator Paginator(FlowDocument doc) =>
		(DynamicDocumentPaginator)((IDocumentPaginatorSource)doc).DocumentPaginator;

	[Fact]
	public void F1_IsBackgroundPaginationEnabled_is_virtual_and_defaults_to_false()
	{
		var p = new MinimalPaginator();
		Assert.False(p.IsBackgroundPaginationEnabled);
		p.IsBackgroundPaginationEnabled = true;
		Assert.False(p.IsBackgroundPaginationEnabled);
		Assert.False(typeof(DynamicDocumentPaginator).GetProperty(nameof(p.IsBackgroundPaginationEnabled))!
			.GetMethod!.IsAbstract);
	}

	[Fact]
	public void F2_MinPage_and_MaxPage_are_uint_and_zero_throws_ArgumentException() => Run(() =>
	{
		var d = new PrintDialog();
		uint min = d.MinPage;
		uint max = d.MaxPage;
		Assert.Equal(1u, min);
		Assert.Equal(9999u, max);
		Assert.Throws<ArgumentException>(() => d.MinPage = 0);
		Assert.Throws<ArgumentException>(() => d.MaxPage = 0);
		d.MaxPage = 20u;
		Assert.Equal(20u, d.MaxPage);
	});

	[Fact]
	public void F3_DocumentPage_Dispose_is_virtual_and_overridable()
	{
		DocumentPage page = new OverridingPage();
		int raised = 0;
		page.Disposed += (s, e) => raised++;
		page.Dispose();
		page.Dispose();
		Assert.Equal(2, ((OverridingPage)page).Disposed2);
		Assert.Equal(1, raised);
		Assert.Null(page.Visual);
	}

	[Fact]
	public void F4_two_overlapping_calls_with_the_same_state_both_complete_uncancelled() => Run(() =>
	{
		var paginator = Paginator(Doc(40));
		var results = new System.Collections.Generic.List<GetPageCompletedEventArgs>();
		paginator.GetPageCompleted += (s, e) => results.Add(e);
		using (new GtkDispatcherScope())
		{
			paginator.GetPageAsync(0);
			paginator.GetPageAsync(1);
			GtkPump.For(150);
		}
		Assert.Equal(2, results.Count);
		Assert.All(results, e => Assert.False(e.Cancelled));
		Assert.All(results, e => Assert.NotNull(e.DocumentPage));
	});

	[Fact]
	public void F4_CancelAsync_cancels_every_outstanding_call_with_that_state() => Run(() =>
	{
		var paginator = Paginator(Doc(5));
		var results = new System.Collections.Generic.List<GetPageCompletedEventArgs>();
		paginator.GetPageCompleted += (s, e) => results.Add(e);
		using (new GtkDispatcherScope())
		{
			paginator.GetPageAsync(0, "a");
			paginator.GetPageAsync(0, "a");
			paginator.GetPageAsync(0, "b");
			paginator.CancelAsync("a");
			GtkPump.For(150);
		}
		Assert.Equal(3, results.Count);
		Assert.Equal(2, results.FindAll(e => e.Cancelled).Count);
		Assert.False(results.Find(e => (string)e.UserState! == "b")!.Cancelled);
	});

	[Fact]
	public void F5_parameterless_FitToMaxPagesAcross_keeps_MaxPagesAcross_and_the_range_is_1_to_32() => Run(() =>
	{
		var v = new DocumentViewer { MaxPagesAcross = 3 };
		v.FitToMaxPagesAcross();
		Assert.Equal(3, v.MaxPagesAcross);
		v.FitToMaxPagesAcross(32);
		Assert.Equal(32, v.MaxPagesAcross);
		Assert.Throws<ArgumentOutOfRangeException>(() => v.FitToMaxPagesAcross(33));
		Assert.Throws<ArgumentOutOfRangeException>(() => v.FitToMaxPagesAcross(0));
		Assert.Throws<ArgumentException>(() => v.MaxPagesAcross = 33);
	});

	[Fact]
	public void F6_content_and_page_property_changes_invalidate_the_count_and_raise_PagesChanged()
	{
		var doc = Doc(40);
		var paginator = Paginator(doc);
		paginator.IsBackgroundPaginationEnabled = false;
		paginator.PageSize = new Size(600, 600);
		int plain = paginator.PageCount;
		int raised = 0;
		paginator.PagesChanged += (s, e) => raised++;
		doc.PagePadding = new Thickness(100);
		Assert.True(raised > 0);
		Assert.True(paginator.PageCount > plain);
		int padded = paginator.PageCount;
		for (int i = 0; i < 40; i++) doc.Blocks.Add(new Paragraph(new Run("more text " + i)));
		Assert.False(paginator.IsPageCountValid);
		Assert.True(paginator.PageCount > padded);
	}

	[Fact]
	public void F7_the_status_label_follows_a_page_count_change_with_the_master_page_unchanged() => Run(() =>
	{
		var doc = Doc(2);
		doc.PageWidth = 300;
		doc.PageHeight = 300;
		var v = new FlowDocumentPageViewer { Document = doc };
		Assert.NotNull(v.Widget);
		var status = (Gtk.Label)typeof(FlowDocumentPageViewer)
			.GetField("_status", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(v)!;
		Assert.Equal($"1 / {v.PageCount}", status.Text);
		int before = v.PageCount;
		for (int i = 0; i < 60; i++) doc.Blocks.Add(new Paragraph(new Run("more text " + i)));
		Assert.True(v.PageCount > before);
		Assert.Equal(1, v.MasterPageNumber);
		Assert.Equal($"1 / {v.PageCount}", status.Text);
	});

	[Fact]
	public void F10_commands_are_static_get_only_properties_and_TextEffectTarget_has_no_public_constructor()
	{
		foreach (var name in new[] { "ViewThumbnailsCommand", "FitToWidthCommand", "FitToHeightCommand",
			"FitToMaxPagesAcrossCommand" })
		{
			var p = typeof(DocumentViewer).GetProperty(name, BindingFlags.Public | BindingFlags.Static);
			Assert.NotNull(p);
			Assert.Null(p!.SetMethod);
			Assert.Equal(typeof(RoutedUICommand), p.PropertyType);
			Assert.Null(typeof(DocumentViewer).GetField(name, BindingFlags.Public | BindingFlags.Static));
		}
		Assert.Empty(typeof(TextEffectTarget).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
	}
}
