using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for <see cref="RichTextBox"/> and <see cref="FlowDocumentScrollViewer"/>.
/// Each body runs on the STA GTK thread via <c>Run(…)</c>; skips when GTK is unavailable.
/// </summary>
public class RichTextBoxGtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void RichTextBox_CreateWidget_yields_ScrolledWindow() => Run(() =>
	{
		var rtb = new RichTextBox();
		Assert.IsType<Gtk.ScrolledWindow>(rtb.Widget);
	});

	[Fact]
	public void RichTextBox_ScrolledWindow_contains_TextView() => Run(() =>
	{
		var rtb = new RichTextBox();
		var sw = (Gtk.ScrolledWindow)rtb.Widget;
		// GtkSharp4: Child wraps in a Viewport; unwrap one level.
		var viewport = sw.Child as Gtk.Viewport;
		var textView = viewport != null ? viewport.Child as Gtk.TextView : sw.Child as Gtk.TextView;
		Assert.NotNull(textView);
	});

	[Fact]
	public void RichTextBox_IsReadOnly_true_sets_Editable_false() => Run(() =>
	{
		var rtb = new RichTextBox { IsReadOnly = true };
		var sw = (Gtk.ScrolledWindow)rtb.Widget;
		var viewport = sw.Child as Gtk.Viewport;
		var textView = viewport != null ? viewport.Child as Gtk.TextView : sw.Child as Gtk.TextView;
		Assert.NotNull(textView);
		Assert.False(textView!.Editable);
	});

	[Fact]
	public void RichTextBox_Document_syncs_buffer_text() => Run(() =>
	{
		var doc = new FlowDocument();
		var p = new Paragraph(); p.Inlines.Add(new Run { Text = "hello world" });
		doc.Blocks.Add(p);

		var rtb = new RichTextBox { Document = doc };
		var sw = (Gtk.ScrolledWindow)rtb.Widget;
		var viewport = sw.Child as Gtk.Viewport;
		var textView = viewport != null ? viewport.Child as Gtk.TextView : sw.Child as Gtk.TextView;
		Assert.NotNull(textView);
		Assert.Contains("hello world", textView!.Buffer.Text);
	});

	[Fact]
	public void FlowDocumentScrollViewer_CreateWidget_yields_ScrolledWindow() => Run(() =>
	{
		var viewer = new FlowDocumentScrollViewer();
		Assert.IsType<Gtk.ScrolledWindow>(viewer.Widget);
	});

	[Fact]
	public void FlowDocumentScrollViewer_realizes_with_document() => Run(() =>
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(new Paragraph { Inlines = { new Run { Text = "Rich content" } } });
		var viewer = new FlowDocumentScrollViewer { Document = doc };
		// Widget creation should not throw.
		var sw = viewer.Widget;
		Assert.NotNull(sw);
	});
}