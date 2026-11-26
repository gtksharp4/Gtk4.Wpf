using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for <see cref="TextBox"/> selection, caret and <see cref="TextBox.AcceptsTab"/>.
/// Each body runs on the STA GTK thread via <c>Run(…)</c>; skips when GTK is unavailable.
/// </summary>
/// <remarks>
/// GTK limitation: <see cref="Gtk.Entry"/> (single-line) has no <c>AcceptsTab</c> property.
/// <c>AcceptsTab</c> is stored in the DP and applied only when the backing widget is
/// <see cref="Gtk.TextView"/> (multiline/<see cref="TextBox.AcceptsReturn"/>=<c>true</c>).
/// </remarks>
public class TextBoxGtkSmokeTests : WpfGtkTestBase
{
	// ── Widget shape ─────────────────────────────────────────────────────────

	[Fact]
	public void SingleLine_Widget_is_GtkEntry() => Run(() =>
	{
		var tb = new TextBox { Text = "hi" };
		Assert.IsType<Gtk.Entry>(tb.Widget);
	});

	[Fact]
	public void Multiline_Widget_is_GtkScrolledWindow() => Run(() =>
	{
		var tb = new TextBox { AcceptsReturn = true, Text = "hi" };
		Assert.IsType<Gtk.ScrolledWindow>(tb.Widget);
	});

	// ── Single-line selection → Entry.SelectRegion ───────────────────────────

	[Fact]
	public void Select_on_realized_Entry_sets_selection_bounds() => Run(() =>
	{
		var tb = new TextBox { Text = "hello world" };
		_ = tb.Widget; // realize
		tb.Select(6, 5);

		Assert.Equal(6, tb.SelectionStart);
		Assert.Equal(5, tb.SelectionLength);
	});

	[Fact]
	public void SelectAll_on_realized_Entry_selects_whole_text() => Run(() =>
	{
		var tb = new TextBox { Text = "abc" };
		_ = tb.Widget;
		tb.SelectAll();

		Assert.Equal(0, tb.SelectionStart);
		Assert.Equal(3, tb.SelectionLength);
	});

	[Fact]
	public void Select_before_realize_does_not_throw_and_persists() => Run(() =>
	{
		var tb = new TextBox { Text = "hello" };
		tb.Select(1, 3); // before Widget is accessed
		_ = tb.Widget;   // realize — should apply without throwing

		Assert.Equal(1, tb.SelectionStart);
		Assert.Equal(3, tb.SelectionLength);
	});

	// ── Multiline selection → TextBuffer.SelectRange ─────────────────────────

	[Fact]
	public void Select_on_realized_TextView_reflects_into_buffer() => Run(() =>
	{
		var tb = new TextBox { AcceptsReturn = true, Text = "hello world" };
		var sw       = (Gtk.ScrolledWindow)tb.Widget;
		var viewport = sw.Child as Gtk.Viewport;
		var textView = (Gtk.TextView)(viewport != null ? viewport.Child! : sw.Child!);

		tb.Select(6, 5);

		Assert.True(textView.Buffer.HasSelection);
	});

	[Fact]
	public void SelectAll_on_realized_TextView_selects_all() => Run(() =>
	{
		var tb = new TextBox { AcceptsReturn = true, Text = "abc" };
		var sw       = (Gtk.ScrolledWindow)tb.Widget;
		var viewport = sw.Child as Gtk.Viewport;
		var textView = (Gtk.TextView)(viewport != null ? viewport.Child! : sw.Child!);

		tb.SelectAll();

		Assert.True(textView.Buffer.HasSelection);
	});

	// ── AcceptsTab ────────────────────────────────────────────────────────────

	[Fact]
	public void AcceptsTab_true_maps_to_TextView_in_multiline() => Run(() =>
	{
		var tb = new TextBox { AcceptsReturn = true, AcceptsTab = true };
		var sw       = (Gtk.ScrolledWindow)tb.Widget;
		var viewport = sw.Child as Gtk.Viewport;
		var textView = (Gtk.TextView)(viewport != null ? viewport.Child! : sw.Child!);

		Assert.True(textView.AcceptsTab);
	});

	[Fact]
	public void AcceptsTab_false_default_maps_to_TextView() => Run(() =>
	{
		var tb = new TextBox { AcceptsReturn = true };
		var sw       = (Gtk.ScrolledWindow)tb.Widget;
		var viewport = sw.Child as Gtk.Viewport;
		var textView = (Gtk.TextView)(viewport != null ? viewport.Child! : sw.Child!);

		Assert.False(textView.AcceptsTab);
	});

	[Fact]
	public void AcceptsTab_on_singleline_stored_but_no_throw() => Run(() =>
	{
		var tb = new TextBox { AcceptsTab = true };
		var ex = Record.Exception(() => _ = tb.Widget);
		Assert.Null(ex);
		Assert.True(tb.AcceptsTab); // DP value preserved
	});
}
