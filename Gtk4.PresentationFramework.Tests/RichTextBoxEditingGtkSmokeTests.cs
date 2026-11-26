using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for the <see cref="RichTextBox"/> editing operations that drive the backing
/// <see cref="Gtk.TextView"/>/<see cref="Gtk.TextBuffer"/> (undo/redo, select-all, clipboard,
/// scrolling). Each body runs on the STA GTK thread; skips when GTK is unavailable.
/// </summary>
public class RichTextBoxEditingGtkSmokeTests : WpfGtkTestBase
{
	static Gtk.TextView TextViewOf(RichTextBox rtb)
	{
		var sw = (Gtk.ScrolledWindow)rtb.Widget;
		var viewport = sw.Child as Gtk.Viewport;
		return (Gtk.TextView)(viewport != null ? viewport.Child! : sw.Child!);
	}

	[Fact]
	public void Undo_then_Redo_round_trips_an_edit() => Run(() =>
	{
		var rtb = new RichTextBox();
		var buf = TextViewOf(rtb).Buffer;

		var end = buf.EndIter;
		buf.Insert(ref end, "hello");
		Assert.Equal("hello", buf.Text);
		Assert.True(rtb.CanUndo);

		rtb.Undo();
		Assert.Equal(string.Empty, buf.Text);
		Assert.True(rtb.CanRedo);

		rtb.Redo();
		Assert.Equal("hello", buf.Text);
	});

	[Fact]
	public void SelectAll_selects_all_and_raises_SelectionChanged() => Run(() =>
	{
		var rtb = new RichTextBox();
		var buf = TextViewOf(rtb).Buffer;
		var end = buf.EndIter;
		buf.Insert(ref end, "abc");

		bool selectionChanged = false;
		rtb.SelectionChanged += (_, _) => selectionChanged = true;

		rtb.SelectAll();

		Assert.True(buf.HasSelection);
		Assert.True(selectionChanged);
	});

	[Fact]
	public void AcceptsTab_maps_to_TextView() => Run(() =>
	{
		var rtb = new RichTextBox { AcceptsTab = true };
		Assert.True(TextViewOf(rtb).AcceptsTab);
	});

	[Fact]
	public void Clipboard_and_scroll_commands_do_not_throw() => Run(() =>
	{
		var rtb = new RichTextBox();
		var buf = TextViewOf(rtb).Buffer;
		var end = buf.EndIter;
		buf.Insert(ref end, "content");
		rtb.SelectAll();

		var ex = Record.Exception(() =>
		{
			rtb.Copy();
			rtb.Cut();
			rtb.Paste();
			rtb.ScrollToHome();
			rtb.ScrollToEnd();
		});

		Assert.Null(ex);
	});
}
