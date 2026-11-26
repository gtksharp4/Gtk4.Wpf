using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>TextWrapping and AppendText on real GTK widgets.</summary>
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
	public void TextBlock_TextWrapping_maps_to_label_wrap(
		TextWrapping value, bool wrap, Pango.WrapMode mode) => Run(() =>
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
	public void TextBox_with_wrapping_only_keeps_the_single_line_entry() => Run(() =>
	{
		var tb = new TextBox { TextWrapping = TextWrapping.Wrap };
		Assert.IsType<Gtk.Entry>(tb.Widget);
		Assert.Equal(TextWrapping.Wrap, tb.TextWrapping);
	});

	[Fact]
	public void TextBox_wrapping_without_AcceptsReturn_is_content_sized_in_a_StackPanel() => Run(() =>
	{
		var tb = new TextBox { TextWrapping = TextWrapping.Wrap, Text = "some text" };
		var win = new Window { Content = new StackPanel { Children = { tb } } };
		win.Show();
		try
		{
			// The size arrives with GTK's first layout pass; slow runners (macOS) need more than 2 s.
			GtkState.Require(() => tb.ActualHeight > 0, "TextBox ActualHeight > 0", win, tb);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void TextBox_wrapping_without_AcceptsReturn_enforces_MaxLength() => Run(() =>
	{
		var tb = new TextBox { TextWrapping = TextWrapping.Wrap, MaxLength = 3 };
		var entry = (Gtk.Entry)tb.Widget;
		entry.Text = "abcdef";
		Assert.Equal("abc", tb.Text);
	});

	[Fact]
	public void TextBox_TextWrappingProperty_is_owned_from_TextBlock_with_AffectsMeasure()
	{
		Assert.Same(TextBlock.TextWrappingProperty, TextBox.TextWrappingProperty);
		var md = (FrameworkPropertyMetadata)TextBox.TextWrappingProperty.GetMetadata(typeof(TextBox));
		Assert.True(md.AffectsMeasure);
		Assert.Equal(TextWrapping.NoWrap, md.DefaultValue);
	}

	[Fact]
	public void TextWrapping_lives_in_System_Windows() =>
		Assert.Equal("System.Windows", typeof(TextWrapping).Namespace);

	[Fact]
	public void ScrollViewer_scrollbar_visibility_accessors_reject_null()
	{
		Assert.Throws<System.ArgumentNullException>(() => ScrollViewer.GetHorizontalScrollBarVisibility(null!));
		Assert.Throws<System.ArgumentNullException>(() => ScrollViewer.GetVerticalScrollBarVisibility(null!));
		Assert.Throws<System.ArgumentNullException>(
			() => ScrollViewer.SetHorizontalScrollBarVisibility(null!, ScrollBarVisibility.Auto));
		Assert.Throws<System.ArgumentNullException>(
			() => ScrollViewer.SetVerticalScrollBarVisibility(null!, ScrollBarVisibility.Auto));
	}

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
	public void AppendText_on_multiline_keeps_the_caret() => Run(() =>
	{
		var tb = new TextBox { AcceptsReturn = true, Text = "abc" };
		var buf = View(tb).Buffer;
		buf.PlaceCursor(buf.GetIterAtOffset(1));
		var count = 0;
		tb.TextChanged += (_, _) => count++;
		tb.AppendText("def");
		Assert.Equal("abcdef", tb.Text);
		Assert.Equal("abcdef", buf.Text);
		Assert.Equal(1, buf.CursorPosition);
		Assert.Equal(1, count);
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
}
