using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>TextBox/RichTextBox scroll-bar visibility, scrolling members on real GTK widgets.</summary>
public class TextScrollingTests : WpfGtkTestBase
{
	// ── Scroll-bar visibility ────────────────────────────────────────────────

	[Fact]
	public void TextBox_scrollbar_visibility_defaults_to_Hidden()
	{
		var tb = new TextBox();
		Assert.Equal(ScrollBarVisibility.Hidden, tb.HorizontalScrollBarVisibility);
		Assert.Equal(ScrollBarVisibility.Hidden, tb.VerticalScrollBarVisibility);
		Assert.Equal(ScrollBarVisibility.Hidden, new RichTextBox().HorizontalScrollBarVisibility);
		Assert.Equal(ScrollBarVisibility.Hidden, new RichTextBox().VerticalScrollBarVisibility);
	}

	[Fact]
	public void TextBox_scrollbar_DPs_are_the_ScrollViewer_properties()
	{
		Assert.Same(ScrollViewer.HorizontalScrollBarVisibilityProperty, TextBox.HorizontalScrollBarVisibilityProperty);
		Assert.Same(ScrollViewer.VerticalScrollBarVisibilityProperty, TextBox.VerticalScrollBarVisibilityProperty);
		Assert.Same(ScrollViewer.HorizontalScrollBarVisibilityProperty,
			RichTextBox.HorizontalScrollBarVisibilityProperty);
	}

	[Fact]
	public void TextBox_scrollbar_visibility_maps_to_policies() => Run(() =>
	{
		var tb = new TextBox
		{
			AcceptsReturn = true,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
		};
		var sw = (Gtk.ScrolledWindow)tb.Widget;
		Assert.Equal(Gtk.PolicyType.Always, sw.HscrollbarPolicy);
		Assert.Equal(Gtk.PolicyType.Automatic, sw.VscrollbarPolicy);
		tb.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
		Assert.Equal(Gtk.PolicyType.Never, sw.VscrollbarPolicy);
	});

	[Fact]
	public void TextBox_default_Hidden_maps_to_External_policy() => Run(() =>
	{
		var sw = (Gtk.ScrolledWindow)new TextBox { AcceptsReturn = true }.Widget;
		Assert.Equal(Gtk.PolicyType.External, sw.HscrollbarPolicy);
		Assert.Equal(Gtk.PolicyType.External, sw.VscrollbarPolicy);
	});

	[Fact]
	public void TextBox_scrollbar_visibility_on_single_line_is_store_only() => Run(() =>
	{
		var tb = new TextBox { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
		Assert.IsType<Gtk.Entry>(tb.Widget);
		Assert.Equal(ScrollBarVisibility.Auto, tb.VerticalScrollBarVisibility);
	});

	[Fact]
	public void RichTextBox_scrollbar_visibility_maps_to_policies() => Run(() =>
	{
		var rtb = new RichTextBox { VerticalScrollBarVisibility = ScrollBarVisibility.Visible };
		var sw = (Gtk.ScrolledWindow)rtb.Widget;
		Assert.Equal(Gtk.PolicyType.Always, sw.VscrollbarPolicy);
		Assert.Equal(Gtk.PolicyType.External, sw.HscrollbarPolicy);
		rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
		Assert.Equal(Gtk.PolicyType.Automatic, sw.HscrollbarPolicy);
	});

	// ── ScrollToEnd / ScrollToHome ───────────────────────────────────────────

	static Gtk.ScrolledWindow Scrolled(TextBox tb) => (Gtk.ScrolledWindow)tb.Widget;

	[Fact]
	public void ScrollToEnd_and_ScrollToHome_do_not_throw_with_or_without_widget() => Run(() =>
	{
		var single = new TextBox { Text = "abc" };
		_ = single.Widget;
		single.ScrollToEnd();
		single.ScrollToHome();
		new TextBox().ScrollToEnd();
	});

	[Fact]
	public void ScrollToEnd_right_after_AppendText_scrolls_to_the_bottom_and_ScrollToHome_back() => Run(() =>
	{
		var log = new TextBox
		{
			AcceptsReturn = true,
			Height = 100,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
		};
		var win = new Window { Content = log };
		win.Show();
		GtkPump.For(100);
		for (int i = 0; i < 200; i++) log.AppendText("line " + i + "\n");
		log.ScrollToEnd();
		var adj = Scrolled(log).Vadjustment;
		GtkPump.Until(() => adj.Upper > adj.PageSize && adj.Value + adj.PageSize >= adj.Upper - 1, 3000);
		var atEnd = (adj.Value, adj.PageSize, adj.Upper);
		log.ScrollToHome();
		GtkPump.Until(() => adj.Value == 0, 3000);
		var atHome = adj.Value;
		win.Close();
		Assert.True(atEnd.Upper > atEnd.PageSize, $"not scrollable: {atEnd}");
		Assert.True(atEnd.Value + atEnd.PageSize >= atEnd.Upper - 1, $"not at end: {atEnd}");
		Assert.Equal(0, atHome);
	});

	[Fact]
	public void AppendText_past_MaxLength_is_clamped_and_raises_TextChanged() => Run(() =>
	{
		var tb = new TextBox { AcceptsReturn = true, MaxLength = 8, Text = "abcde" };
		var win = new Window { Content = tb };
		win.Show();
		GtkPump.For(100);
		int raised = 0;
		tb.TextChanged += (_, _) => raised++;
		tb.AppendText("fghij");
		var text = tb.Text;
		var count = raised;
		tb.AppendText("k");
		var after = tb.Text;
		win.Close();
		Assert.Equal("abcdefgh", text);
		Assert.True(count > 0);
		Assert.Equal("abcdefgh", after);
	});
}
