using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class BringIntoViewTests : WpfGtkTestBase
{
	static FlowDocument Doc(int paragraphs, out Run last)
	{
		var doc = new FlowDocument();
		last = null!;
		for (int i = 0; i < paragraphs; i++)
		{
			last = new Run("Lorem ipsum dolor sit amet, consectetur adipiscing elit " + i);
			doc.Blocks.Add(new Paragraph(last));
		}
		return doc;
	}

	[Fact]
	public void The_event_and_args_have_the_wpf_shape()
	{
		Assert.Equal(RoutingStrategy.Bubble, FrameworkElement.RequestBringIntoViewEvent.RoutingStrategy);
		Assert.Equal("RequestBringIntoView", FrameworkElement.RequestBringIntoViewEvent.Name);
		Assert.Equal(typeof(RequestBringIntoViewEventHandler), FrameworkElement.RequestBringIntoViewEvent.HandlerType);
		var run = new Run("x");
		var args = new RequestBringIntoViewEventArgs(run, new Rect(1, 2, 3, 4));
		Assert.Same(run, args.TargetObject);
		Assert.Equal(new Rect(1, 2, 3, 4), args.TargetRect);
		Assert.Same(FrameworkElement.RequestBringIntoViewEvent, args.RoutedEvent);
		Assert.True(typeof(RequestBringIntoViewEventArgs).IsSealed);
	}

	[Fact]
	public void Content_BringIntoView_bubbles_through_the_content_parents_with_an_empty_rect() => Run(() =>
	{
		var doc = Doc(2, out var run);
		var viewer = new FlowDocumentPageViewer { Document = doc };
		var order = new List<string>();
		RequestBringIntoViewEventArgs? seen = null;
		var para = (Paragraph)run.Parent!;
		run.AddHandler(FrameworkElement.RequestBringIntoViewEvent,
			new RequestBringIntoViewEventHandler((s, e) => { order.Add("run"); seen = e; }));
		para.AddHandler(FrameworkElement.RequestBringIntoViewEvent,
			new RequestBringIntoViewEventHandler((s, e) => order.Add("paragraph")));
		doc.AddHandler(FrameworkElement.RequestBringIntoViewEvent,
			new RequestBringIntoViewEventHandler((s, e) => order.Add("document")));
		run.BringIntoView();
		Assert.Equal(new[] { "run", "paragraph", "document" }, order);
		Assert.Same(run, seen!.TargetObject);
		Assert.Equal(Rect.Empty, seen.TargetRect);
		Assert.True(seen.Handled);
		Assert.NotNull(viewer);
	});

	[Fact]
	public void FlowDocumentPageViewer_goes_to_the_page_of_the_element_and_handles_the_request() => Run(() =>
	{
		var doc = Doc(400, out var last);
		var viewer = new FlowDocumentPageViewer { Document = doc };
		var window = new Window { Content = viewer };
		window.Show();
		GtkPump.For(100);
		Assert.True(viewer.PageCount > 1);
		Assert.Equal(1, viewer.MasterPageNumber);
		last.BringIntoView();
		GtkPump.For(50);
		Assert.Equal(viewer.PageCount, viewer.MasterPageNumber);
		((Run)((Paragraph)doc.Blocks[0]).Inlines[0]).BringIntoView();
		GtkPump.For(50);
		Assert.Equal(1, viewer.MasterPageNumber);
		window.Close();
	});

	[Fact]
	public void FlowDocumentReader_scrolls_to_the_requested_element_and_handles_the_request() => Run(() =>
	{
		var doc = Doc(300, out var last);
		var reader = new FlowDocumentReader { Document = doc };
		var adjustment = ((Gtk.ScrolledWindow)reader.Widget).Vadjustment;
		adjustment.Upper = 10000;
		adjustment.PageSize = 500;
		var args = new RequestBringIntoViewEventArgs(last, Rect.Empty);
		last.RaiseEvent(args);
		Assert.True(args.Handled);
		Assert.True(adjustment.Value > 0);
		doc.Blocks[0].BringIntoView();
		Assert.Equal(0, adjustment.Value, 0);
	});

	[Fact]
	public void FlowDocumentScrollViewer_scrolls_its_window_to_the_block() => Run(() =>
	{
		var doc = Doc(300, out var last);
		var viewer = new FlowDocumentScrollViewer { Document = doc };
		var window = (Gtk.ScrolledWindow)viewer.Widget;
		var adjustment = window.Vadjustment;
		adjustment.Upper = 10000;
		adjustment.PageSize = 500;
		last.BringIntoView();
		Assert.True(adjustment.Value > 0);
		doc.Blocks[0].BringIntoView();
		Assert.Equal(0, adjustment.Value, 0);
	});

	[Fact]
	public void A_request_for_a_foreign_element_is_not_handled_by_the_viewer() => Run(() =>
	{
		var doc = Doc(2, out _);
		var viewer = new FlowDocumentPageViewer { Document = doc };
		var other = new Run("elsewhere");
		var args = new RequestBringIntoViewEventArgs(other, Rect.Empty);
		viewer.RaiseEvent(args);
		Assert.False(args.Handled);
	});

	[Fact]
	public void An_unhandled_request_reaches_the_window_without_throwing() => Run(() =>
	{
		var button = new Button();
		var window = new Window { Content = button };
		RequestBringIntoViewEventArgs? seen = null;
		window.RequestBringIntoView += (s, e) => seen = e;
		button.BringIntoView();
		Assert.Same(button, seen!.TargetObject);
		Assert.Equal(Rect.Empty, seen.TargetRect);
		Assert.True(seen.TargetRect.IsEmpty);
		Assert.False(seen.Handled);
	});
}
