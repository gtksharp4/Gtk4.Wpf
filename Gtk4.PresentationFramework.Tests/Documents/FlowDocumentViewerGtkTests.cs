using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Documents;

public class FlowDocumentViewerGtkTests : WpfGtkTestBase
{
	static FlowDocument Doc(string text)
	{
		var d = new FlowDocument();
		d.Blocks.Add(new Paragraph(new Run(text)));
		return d;
	}

	static Gtk.Label LabelOf(Gtk.ScrolledWindow sw)
	{
		var child = sw.Child;
		return (Gtk.Label)(child is Gtk.Viewport vp ? vp.Child! : child!);
	}

	[Fact]
	public void Viewer_scroll_policies_default_vertical_Always_and_follow_the_properties() => Run(() =>
	{
		var viewer = new FlowDocumentScrollViewer();
		var sw = (Gtk.ScrolledWindow)viewer.Widget;
		Assert.Equal(Gtk.PolicyType.Automatic, sw.HscrollbarPolicy);
		Assert.Equal(Gtk.PolicyType.Always, sw.VscrollbarPolicy);
		viewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
		viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
		Assert.Equal(Gtk.PolicyType.Automatic, sw.VscrollbarPolicy);
		Assert.Equal(Gtk.PolicyType.Never, sw.HscrollbarPolicy);
	});

	[Fact]
	public void Viewer_policy_set_before_the_widget_exists_is_applied_on_creation() => Run(() =>
	{
		var viewer = new FlowDocumentScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Hidden };
		Assert.Equal(Gtk.PolicyType.External, ((Gtk.ScrolledWindow)viewer.Widget).VscrollbarPolicy);
	});

	[Fact]
	public void Viewer_shows_the_document_markup_and_follows_edits() => Run(() =>
	{
		var doc = Doc("hello");
		var viewer = new FlowDocumentScrollViewer { Document = doc };
		var label = LabelOf((Gtk.ScrolledWindow)viewer.Widget);
		Assert.True(label.UseMarkup);
		Assert.Equal("hello", label.Text);
		doc.Blocks.Add(new Paragraph(new Run("more")));
		Assert.Equal("hello\n\nmore", label.Text);
		viewer.Document = null;
		Assert.Equal(string.Empty, label.Text);
		Assert.Null(doc.Parent);
	});

	[Fact]
	public void Viewer_selection_flag_maps_to_label_selectable() => Run(() =>
	{
		var viewer = new FlowDocumentScrollViewer();
		var label = LabelOf((Gtk.ScrolledWindow)viewer.Widget);
		Assert.True(viewer.IsSelectionEnabled);
		Assert.True(label.Selectable);
		viewer.IsSelectionEnabled = false;
		Assert.False(label.Selectable);
	});

	[Fact]
	public void Viewer_document_font_weight_and_background_reach_the_label() => Run(() =>
	{
		var doc = Doc("x");
		doc.FontWeight = FontWeights.Bold;
		doc.Background = Brushes.Red;
		var viewer = new FlowDocumentScrollViewer { Document = doc };
		var label = LabelOf((Gtk.ScrolledWindow)viewer.Widget);
		Assert.Equal("x", label.Text);
		Assert.Contains("weight=\"700\"", doc.BuildViewMarkup());
		doc.FontWeight = FontWeights.Normal;
		Assert.Contains("weight=\"400\"", doc.BuildViewMarkup());
	});

	[Fact]
	public void Viewer_stores_toolbar_and_zoom_with_coercion() => Run(() =>
	{
		var viewer = new FlowDocumentScrollViewer();
		Assert.False(viewer.IsToolBarVisible);
		Assert.Equal(100.0, viewer.Zoom);
		Assert.Equal(80.0, viewer.MinZoom);
		Assert.Equal(200.0, viewer.MaxZoom);
		viewer.Zoom = 500;
		Assert.Equal(200.0, viewer.Zoom);
		viewer.Zoom = 10;
		Assert.Equal(80.0, viewer.Zoom);
		Assert.Throws<ArgumentException>(() => viewer.Zoom = 0);
		Assert.Throws<ArgumentException>(() => viewer.VerticalScrollBarVisibility = (ScrollBarVisibility)9);
	});

	[Fact]
	public void Document_cannot_have_two_viewers() => Run(() =>
	{
		var doc = Doc("x");
		var first = new FlowDocumentScrollViewer { Document = doc };
		Assert.Same(first, doc.Parent);
		Assert.Throws<InvalidOperationException>(() => new FlowDocumentScrollViewer { Document = doc });
	});

	[Fact]
	public void Reader_defaults_match_WPF() => Run(() =>
	{
		var reader = new FlowDocumentReader();
		Assert.Equal(FlowDocumentReaderViewingMode.Page, reader.ViewingMode);
		Assert.True(reader.IsPageViewEnabled);
		Assert.True(reader.IsTwoPageViewEnabled);
		Assert.True(reader.IsScrollViewEnabled);
		Assert.True(reader.IsFindEnabled);
		Assert.True(reader.IsPrintEnabled);
		Assert.Equal(100.0, reader.Zoom);
		Assert.Equal(80.0, reader.MinZoom);
		Assert.Equal(200.0, reader.MaxZoom);
		Assert.Equal(10.0, reader.ZoomIncrement);
		Assert.Equal(0, reader.PageCount);
		Assert.Equal(0, reader.PageNumber);
		Assert.False(reader.CanGoToPage(1));
		Assert.Throws<ArgumentException>(() => reader.ViewingMode = (FlowDocumentReaderViewingMode)7);
	});

	[Fact]
	public void Reader_renders_the_document_in_every_viewing_mode_and_counts_one_page() => Run(() =>
	{
		var reader = new FlowDocumentReader { Document = Doc("page") };
		var label = LabelOf((Gtk.ScrolledWindow)reader.Widget);
		Assert.Equal("page", label.Text);
		foreach (var mode in new[]
		{
			FlowDocumentReaderViewingMode.TwoPage, FlowDocumentReaderViewingMode.Scroll
		})
		{
			reader.ViewingMode = mode;
			Assert.Equal("page", label.Text);
		}
		Assert.Equal(1, reader.PageCount);
		Assert.Equal(1, reader.PageNumber);
		Assert.True(reader.CanGoToPage(1));
		Assert.False(reader.CanGoToPage(2));
		Assert.Throws<InvalidOperationException>(() => reader.SetValue(FlowDocumentReader.PageCountProperty, 3));
	});

	[Fact]
	public void Reader_takes_border_through_Control_and_the_document_as_xaml_content() => Run(() =>
	{
		var navy = Brushes.Navy;
		var reader = new FlowDocumentReader { BorderBrush = navy, BorderThickness = new Thickness(2) };
		Assert.Same(navy, reader.BorderBrush);
		Assert.Equal(new Thickness(2), reader.BorderThickness);
		var attr = (System.Windows.Markup.ContentPropertyAttribute?)Attribute.GetCustomAttribute(
			typeof(FlowDocumentReader), typeof(System.Windows.Markup.ContentPropertyAttribute));
		Assert.Equal("Document", attr?.Name);
	});

	[Fact]
	public void Table_columns_become_label_tab_stops() => Run(() =>
	{
		var doc = new FlowDocument();
		var table = new Table();
		table.RowGroups.Add(new TableRowGroup());
		var row = new TableRow();
		table.RowGroups[0].Rows.Add(row);
		row.Cells.Add(new TableCell(new Paragraph(new Run("a"))));
		row.Cells.Add(new TableCell(new Paragraph(new Run("a much longer cell"))));
		row.Cells.Add(new TableCell(new Paragraph(new Run("c"))));
		doc.Blocks.Add(table);
		var viewer = new FlowDocumentScrollViewer { Document = doc };
		var label = LabelOf((Gtk.ScrolledWindow)viewer.Widget);
		Assert.NotNull(label.Tabs);
		Assert.Equal(2, label.Tabs!.Size);
		Assert.True(label.Tabs.PositionsInPixels);
		label.Tabs.GetTab(0, out _, out int first);
		label.Tabs.GetTab(1, out _, out int second);
		Assert.Equal(doc.BuildTabStops()![0], first);
		Assert.True(second - first > first);
		viewer.Document = Doc("plain");
		Assert.Null(label.Tabs);
	});
}
