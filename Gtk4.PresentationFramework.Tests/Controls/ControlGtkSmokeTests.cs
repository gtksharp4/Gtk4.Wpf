using System;
using System.Windows.Media;
using Xunit;
using WpfBorder = System.Windows.Controls.Border;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfImage = System.Windows.Controls.Image;
using WpfLabel = System.Windows.Controls.Label;
using WpfRadioButton = System.Windows.Controls.RadioButton;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for the Phase-1 controls: verify backing widget type and
/// primary property reflection. All bodies run on the GTK thread via Run(…).
/// Skipped when native GTK is unavailable.
/// </summary>
public class ControlGtkSmokeTests : WpfGtkTestBase
{
	// ── TextBox ──────────────────────────────────────────────────────────────

	[Fact]
	public void TextBox_Widget_is_GtkEntry() => Run(() =>
	{
		var tb = new WpfTextBox { Text = "hi" };
		Assert.IsType<Gtk.Entry>(tb.Widget);
	});

	[Fact]
	public void TextBox_Entry_text_matches_Text() => Run(() =>
	{
		var tb = new WpfTextBox { Text = "hello" };
		var entry = (Gtk.Entry)tb.Widget;
		Assert.Equal("hello", entry.Text);
	});

	[Fact]
	public void TextBox_Text_change_updates_GtkEntry() => Run(() =>
	{
		var tb = new WpfTextBox { Text = "before" };
		var entry = (Gtk.Entry)tb.Widget;
		tb.Text = "after";
		Assert.Equal("after", entry.Text);
	});

	[Fact]
	public void TextBox_GtkEntry_change_updates_Text() => Run(() =>
	{
		var tb = new WpfTextBox();
		var entry = (Gtk.Entry)tb.Widget;
		entry.Text = "typed";
		Assert.Equal("typed", tb.Text);
	});

	[Fact]
	public void TextBox_TextChanged_fires_on_GtkEntry_change() => Run(() =>
	{
		var tb = new WpfTextBox();
		var entry = (Gtk.Entry)tb.Widget;
		bool fired = false;
		tb.TextChanged += (s, e) => { fired = true; };
		entry.Text = "x";
		Assert.True(fired);
	});

	// ── CheckBox ─────────────────────────────────────────────────────────────

	[Fact]
	public void CheckBox_Widget_is_GtkCheckButton() => Run(() =>
	{
		var cb = new WpfCheckBox();
		Assert.IsType<Gtk.CheckButton>(cb.Widget);
	});

	[Fact]
	public void CheckBox_IsChecked_true_activates_GtkCheckButton() => Run(() =>
	{
		var cb = new WpfCheckBox { IsChecked = true };
		var btn = (Gtk.CheckButton)cb.Widget;
		Assert.True(btn.Active);
	});

	[Fact]
	public void CheckBox_GtkToggle_updates_IsChecked() => Run(() =>
	{
		var cb = new WpfCheckBox();
		var btn = (Gtk.CheckButton)cb.Widget;
		btn.Active = true;
		Assert.True(cb.IsChecked == true);
	});

	[Fact]
	public void CheckBox_Checked_event_fires_on_GtkToggle() => Run(() =>
	{
		var cb = new WpfCheckBox();
		_ = cb.Widget; // materialise
		bool fired = false;
		cb.Checked += (s, e) => { fired = true; };
		var btn = (Gtk.CheckButton)cb.Widget;
		btn.Active = true;
		Assert.True(fired);
	});

	// ── RadioButton ──────────────────────────────────────────────────────────

	[Fact]
	public void RadioButton_Widget_is_GtkCheckButton() => Run(() =>
	{
		var rb = new WpfRadioButton();
		Assert.IsType<Gtk.CheckButton>(rb.Widget);
	});

	[Fact]
	public void RadioButton_IsChecked_true_activates_GtkCheckButton() => Run(() =>
	{
		var rb = new WpfRadioButton { IsChecked = true };
		var btn = (Gtk.CheckButton)rb.Widget;
		Assert.True(btn.Active);
	});

	[Fact]
	public void RadioButton_GtkToggle_updates_IsChecked() => Run(() =>
	{
		var rb = new WpfRadioButton();
		var btn = (Gtk.CheckButton)rb.Widget;
		btn.Active = true;
		Assert.True(rb.IsChecked);
	});

	// ── ComboBox ─────────────────────────────────────────────────────────────

	[Fact]
	public void ComboBox_Widget_is_GtkDropDown() => Run(() =>
	{
		var cb = new WpfComboBox();
		Assert.IsType<Gtk.DropDown>(cb.Widget);
	});

	[Fact]
	public void ComboBox_ItemsSource_populates_model() => Run(() =>
	{
		var cb = new WpfComboBox();
		cb.ItemsSource = new[] { "One", "Two", "Three" };
		_ = cb.Widget;
		var dd = (Gtk.DropDown)cb.Widget;
		Assert.NotNull(dd.Model);
	});

	[Fact]
	public void ComboBox_SelectedIndex_set_before_widget_applies_on_creation() => Run(() =>
	{
		var cb = new WpfComboBox();
		cb.ItemsSource = new[] { "A", "B", "C" };
		cb.SelectedIndex = 1;
		var dd = (Gtk.DropDown)cb.Widget;
		Assert.Equal(1u, dd.Selected);
	});

	// ── Image ─────────────────────────────────────────────────────────────────

	[Fact]
	public void Image_Widget_is_GtkPicture() => Run(() =>
	{
		var img = new WpfImage();
		Assert.IsAssignableFrom<Gtk.Picture>(img.Widget);
	});

	[Fact]
	public void Image_Source_null_does_not_throw() => Run(() =>
	{
		var img = new WpfImage { Source = null };
		var ex = Record.Exception(() => _ = img.Widget);
		Assert.Null(ex);
	});

	[Fact]
	public void Image_Source_set_does_not_throw() => Run(() =>
	{
		var img = new WpfImage();
		_ = img.Widget;
		// A non-existent path: GTK sets an error icon — no exception.
		var ex = Record.Exception(() =>
			img.Source = ImageSource.FromFile("nonexistent.png"));
		Assert.Null(ex);
	});

	// ── Border ───────────────────────────────────────────────────────────────

	[Fact]
	public void Border_Widget_is_GtkFrame() => Run(() =>
	{
		var b = new WpfBorder();
		Assert.IsAssignableFrom<Gtk.Frame>(b.Widget);
	});

	[Fact]
	public void Border_Child_set_before_widget_parents_on_creation() => Run(() =>
	{
		var b = new WpfBorder();
		var tb = new WpfTextBlock { Text = "inside" };
		b.Child = tb;
		var frame = (Gtk.Frame)b.Widget;
		Assert.Same(tb.Widget, frame.Child);
	});

	[Fact]
	public void Border_Child_set_after_widget_parents_immediately() => Run(() =>
	{
		var b = new WpfBorder();
		_ = b.Widget; // materialise first
		var tb = new WpfTextBlock { Text = "added later" };
		b.Child = tb;
		var frame = (Gtk.Frame)b.Widget;
		Assert.Same(tb.Widget, frame.Child);
	});

	// ── Label ────────────────────────────────────────────────────────────────

	[Fact]
	public void Label_Widget_is_GtkBox() => Run(() =>
	{
		var lbl = new WpfLabel();
		Assert.IsType<Gtk.Box>(lbl.Widget);
	});

	[Fact]
	public void Label_string_Content_does_not_throw() => Run(() =>
	{
		var lbl = new WpfLabel();
		_ = lbl.Widget; // materialise
		var ex = Record.Exception(() => lbl.Content = "World");
		Assert.Null(ex);
	});
}