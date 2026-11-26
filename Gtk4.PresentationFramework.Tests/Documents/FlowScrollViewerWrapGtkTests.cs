using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Documents;

/// <summary>The scrolling viewers wrap the document to the viewport: no horizontal overflow.</summary>
public class FlowScrollViewerWrapGtkTests : WpfGtkTestBase
{
	const string Long = "The quick brown fox jumps over the lazy dog and keeps running through the forest " +
		"until the sun goes down behind the hills, while the farmers walk home along the dusty road.";

	static FlowDocument Doc()
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(new Paragraph(new Run(Long + Long + Long)));
		return doc;
	}

	static string Measure(FrameworkElement viewer)
	{
		var win = new Window { Content = viewer, Width = 400, Height = 300 };
		win.Show();
		GtkPump.Until(() => viewer.Widget.IsMapped && viewer.Widget.Width > 0);
		GtkPump.For(300);
		var sw = (Gtk.ScrolledWindow)viewer.Widget;
		var child = sw.Child!;
		var label = child is Gtk.Viewport vp ? vp.Child! : child;
		string result = $"{label.Width}/{sw.Width}/{sw.Hadjustment.Upper}/{sw.Hadjustment.PageSize}";
		win.Close();
		return result;
	}

	[Fact]
	public void ScrollViewer_label_is_no_wider_than_the_viewport() => Run(() =>
	{
		var parts = Measure(new FlowDocumentScrollViewer { Document = Doc() }).Split('/');
		Assert.True(int.Parse(parts[0]) <= int.Parse(parts[1]), string.Join("/", parts));
		Assert.True(double.Parse(parts[2]) <= double.Parse(parts[3]) + 1, string.Join("/", parts));
	});

	[Fact]
	public void Reader_label_is_no_wider_than_the_viewport() => Run(() =>
	{
		var parts = Measure(new FlowDocumentReader { Document = Doc() }).Split('/');
		Assert.True(int.Parse(parts[0]) <= int.Parse(parts[1]), string.Join("/", parts));
	});
}
