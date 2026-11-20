using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>TextWrapping, AppendText, scroll members, scroll-bar visibility and ClipToBounds on real GTK widgets.</summary>
public class TextGapsTests : WpfGtkTestBase
{
	static Gtk.TextView View(TextBox tb) => (Gtk.TextView)((Gtk.ScrolledWindow)tb.Widget).Child!;

	// ── TextWrapping enum / TextBlock ────────────────────────────────────────

	[Fact]
	public void TextWrapping_enum_values_match_WPF()
	{
		Assert.Equal(0, (int)TextWrapping.WrapWithOverflow);
		Assert.Equal(1, (int)TextWrapping.NoWrap);
		Assert.Equal(2, (int)TextWrapping.Wrap);
	}

	[Fact]
	public void TextBlock_TextWrapping_defaults_to_NoWrap() =>
		Assert.Equal(TextWrapping.NoWrap, new TextBlock().TextWrapping);

	[Theory]
	[InlineData(TextWrapping.NoWrap, false, Pango.WrapMode.Word)]
	[InlineData(TextWrapping.Wrap, true, Pango.WrapMode.WordChar)]
	[InlineData(TextWrapping.WrapWithOverflow, true, Pango.WrapMode.Word)]
	public void TextBlock_TextWrapping_maps_to_label_wrap(TextWrapping value, bool wrap, Pango.WrapMode mode) => Run(() =>
	{
		var tb = new TextBlock { Text = "a b c", TextWrapping = value };
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal(wrap, label.Wrap);
		if (wrap) Assert.Equal(mode, label.WrapMode);
	});

	[Fact]
	public void TextBlock_TextWrapping_change_after_realize_updates_label() => Run(() =>
	{
		var tb = new TextBlock { Text = "a b c" };
		var label = (Gtk.Label)tb.Widget;
		Assert.False(label.Wrap);
		tb.TextWrapping = TextWrapping.Wrap;
		Assert.True(label.Wrap);
		tb.TextWrapping = TextWrapping.NoWrap;
		Assert.False(label.Wrap);
	});

	// ── TextBox wrapping ─────────────────────────────────────────────────────

	[Theory]
	[InlineData(TextWrapping.NoWrap, Gtk.WrapMode.None)]
	[InlineData(TextWrapping.Wrap, Gtk.WrapMode.WordChar)]
	[InlineData(TextWrapping.WrapWithOverflow, Gtk.WrapMode.Word)]
	public void TextBox_TextWrapping_maps_to_TextView(TextWrapping value, Gtk.WrapMode mode) => Run(() =>
	{
		var tb = new TextBox { AcceptsReturn = true, TextWrapping = value };
		Assert.Equal(mode, View(tb).WrapMode);
	});

	[Fact]
	public void TextBox_TextWrapping_change_after_realize_updates_TextView() => Run(() =>
	{
		var tb = new TextBox { AcceptsReturn = true };
		var view = View(tb);
		tb.TextWrapping = TextWrapping.Wrap;
		Assert.Equal(Gtk.WrapMode.WordChar, view.WrapMode);
	});

	[Fact]
	public void TextBox_with_wrapping_uses_multiline_widget() => Run(() =>
	{
		var tb = new TextBox { TextWrapping = TextWrapping.Wrap };
		Assert.IsType<Gtk.ScrolledWindow>(tb.Widget);
	});

	// ── AppendText / ScrollToEnd / ScrollToHome ──────────────────────────────

	[Fact]
	public void AppendText_appends_to_Text_without_widget()
	{
		var tb = new TextBox { Text = "ab" };
		tb.AppendText("cd");
		Assert.Equal("abcd", tb.Text);
	}

	[Fact]
	public void AppendText_updates_both_widget_kinds() => Run(() =>
	{
		var single = new TextBox { Text = "ab" };
		var entry = (Gtk.Entry)single.Widget;
		single.AppendText("cd");
		Assert.Equal("abcd", entry.Text);

		var multi = new TextBox { AcceptsReturn = true, Text = "ab" };
		var view = View(multi);
		multi.AppendText("\ncd");
		Assert.Equal("ab\ncd", view.Buffer.Text);
		Assert.Equal("ab\ncd", multi.Text);
	});

	[Fact]
	public void AppendText_raises_TextChanged_once()
	{
		var tb = new TextBox();
		var count = 0;
		tb.TextChanged += (_, _) => count++;
		tb.AppendText("x");
		Assert.Equal(1, count);
	}

	[Fact]
	public void AppendText_null_throws_ArgumentNullException() =>
		Assert.Throws<System.ArgumentNullException>(() => new TextBox().AppendText(null!));

	[Fact]
	public void ScrollToEnd_and_ScrollToHome_do_not_throw_with_or_without_widget() => Run(() =>
	{
		var tb = new TextBox { AcceptsReturn = true, Text = "a\nb\nc" };
		tb.ScrollToEnd();
		tb.ScrollToHome();
		var single = new TextBox { Text = "abc" };
		_ = single.Widget;
		single.ScrollToEnd();
		single.ScrollToHome();
		new TextBox().ScrollToEnd();
	});

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
		Assert.Same(ScrollViewer.HorizontalScrollBarVisibilityProperty, RichTextBox.HorizontalScrollBarVisibilityProperty);
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

	// ── ClipToBounds ─────────────────────────────────────────────────────────

	[Fact]
	public void ClipToBounds_defaults_to_false() =>
		Assert.False(new Button().ClipToBounds);

	[Fact]
	public void ClipToBounds_maps_to_widget_overflow() => Run(() =>
	{
		var b = new StackPanel();
		Assert.Equal(Gtk.Overflow.Visible, b.Widget.Overflow);
		b.ClipToBounds = true;
		Assert.Equal(Gtk.Overflow.Hidden, b.Widget.Overflow);
		b.ClipToBounds = false;
		Assert.Equal(Gtk.Overflow.Visible, b.Widget.Overflow);
	});

	[Fact]
	public void ClipToBounds_set_before_realize_applies_on_creation() => Run(() =>
	{
		var b = new StackPanel { ClipToBounds = true };
		Assert.Equal(Gtk.Overflow.Hidden, b.Widget.Overflow);
	});
}
