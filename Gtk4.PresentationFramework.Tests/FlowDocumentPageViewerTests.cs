using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class FlowDocumentPageViewerTests : WpfGtkTestBase
{
	static void AssertDefaultPaginatorSize(FlowDocument doc, double width, double height)
	{
		var size = ((IDocumentPaginatorSource)doc).DocumentPaginator.PageSize;
		Assert.Equal(width, size.Width);
		Assert.Equal(height, size.Height);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Replaced_or_cleared_document_paginates_at_the_default_page_size_again(bool toNull) => Run(() =>
	{
		var docA = new FlowDocument(new Paragraph(new Run("a")));
		var viewer = new FlowDocumentPageViewer { Document = docA };
		var win = new Window { Content = viewer, Width = 600, Height = 400 };
		win.Show();
		GtkPump.Until(() => viewer.Widget.IsMapped && viewer.PageWidget!.Width > 0);
		GtkPump.Until(() => ((IDocumentPaginatorSource)docA).DocumentPaginator.PageSize.Width != 816);
		Assert.NotEqual(816, ((IDocumentPaginatorSource)docA).DocumentPaginator.PageSize.Width);
		viewer.Document = toNull ? null : new FlowDocument(new Paragraph(new Run("b")));
		GtkPump.For(100);
		win.Close();
		AssertDefaultPaginatorSize(docA, 816, 1056);
	});

	[Fact]
	public void Defaults_match_wpf() => Run(() =>
	{
		var v = new FlowDocumentPageViewer();
		Assert.Equal(100, v.Zoom);
		Assert.Equal(200, v.MaxZoom);
		Assert.Equal(80, v.MinZoom);
		Assert.Equal(10, v.ZoomIncrement);
		Assert.True(v.CanIncreaseZoom);
		Assert.True(v.CanDecreaseZoom);
		Assert.Equal(0.4, v.SelectionOpacity);
		Assert.NotNull(v.SelectionBrush);
		Assert.False(v.IsSelectionActive);
		Assert.False(v.IsInactiveSelectionHighlightEnabled);
	});

	[Fact]
	public void Zoom_is_clamped_to_the_limits() => Run(() =>
	{
		var v = new FlowDocumentPageViewer { Zoom = 500 };
		Assert.Equal(200, v.Zoom);
		v.Zoom = 10;
		Assert.Equal(80, v.Zoom);
	});

	[Fact]
	public void IncreaseZoom_and_DecreaseZoom_step_and_stop_at_the_limits() => Run(() =>
	{
		var v = new FlowDocumentPageViewer();
		v.IncreaseZoom();
		Assert.Equal(110, v.Zoom);
		for (int i = 0; i < 20; i++) v.IncreaseZoom();
		Assert.Equal(200, v.Zoom);
		Assert.False(v.CanIncreaseZoom);
		Assert.True(v.CanDecreaseZoom);
		for (int i = 0; i < 20; i++) v.DecreaseZoom();
		Assert.Equal(80, v.Zoom);
		Assert.False(v.CanDecreaseZoom);
	});

	[Fact]
	public void Lowering_MaxZoom_pulls_Zoom_down() => Run(() =>
	{
		var v = new FlowDocumentPageViewer { MaxZoom = 120 };
		v.Zoom = 150;
		Assert.Equal(120, v.Zoom);
		v.MaxZoom = 90;
		Assert.Equal(90, v.Zoom);
		Assert.Equal(90, v.MaxZoom);
	});

	[Fact]
	public void MaxZoom_never_goes_below_MinZoom() => Run(() =>
	{
		var v = new FlowDocumentPageViewer { MaxZoom = 10 };
		Assert.Equal(80, v.MaxZoom);
	});

	[Fact]
	public void Non_positive_zoom_values_are_rejected() => Run(() =>
	{
		var v = new FlowDocumentPageViewer();
		Assert.Throws<ArgumentException>(() => v.Zoom = 0);
		Assert.Throws<ArgumentException>(() => v.ZoomIncrement = -1);
	});

	[Fact]
	public void The_content_property_is_Document() => Run(() =>
	{
		var attr = (System.Windows.Markup.ContentPropertyAttribute)Attribute.GetCustomAttribute(
			typeof(DocumentViewerBase), typeof(System.Windows.Markup.ContentPropertyAttribute))!;
		Assert.Equal("Document", attr.Name);
	});

	[Fact]
	public void The_viewer_shows_the_document_text_in_the_shared_presenter() => Run(() =>
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(new Paragraph(new Run("viewer text")));
		var v = new FlowDocumentPageViewer { Document = doc };
		Assert.NotNull(v.Widget);
		Assert.Equal(1, v.PageCount);
	});
}
