using Xunit;
using WpfButton = System.Windows.Controls.Button;
using WpfGrid = System.Windows.Controls.Grid;
using WpfStackPanel = System.Windows.Controls.StackPanel;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests: construct real WPF-on-GTK objects and verify that the backing
/// GTK widget types are correct. Guard: each test body runs via <c>Run(...)</c>
/// on the STA GTK thread; the class ctor skips when GTK is unavailable.
/// </summary>
public class GtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void TextBlock_Widget_is_GtkLabel() => Run(() =>
	{
		var tb = new WpfTextBlock { Text = "hi" };
		Assert.IsAssignableFrom<Gtk.Label>(tb.Widget);
	});

	[Fact]
	public void TextBlock_Label_text_matches_Text() => Run(() =>
	{
		var tb = new WpfTextBlock { Text = "hello" };
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal("hello", label.Text);
	});

	[Fact]
	public void Grid_Widget_is_GtkGrid() => Run(() =>
	{
		var grid = new WpfGrid();
		Assert.IsAssignableFrom<Gtk.Grid>(grid.Widget);
	});

	[Fact]
	public void StackPanel_Widget_is_GtkBox() => Run(() =>
	{
		var sp = new WpfStackPanel();
		Assert.IsAssignableFrom<Gtk.Box>(sp.Widget);
	});

	[Fact]
	public void Button_Widget_is_GtkButton() => Run(() =>
	{
		var btn = new WpfButton();
		btn.Content = "Click me";
		Assert.IsType<Gtk.Button>(btn.Widget);
	});

	[Fact]
	public void Window_Widget_is_GtkWindow() => Run(() =>
	{
		var win = new WpfWindow { Title = "Test" };
		Assert.IsType<Gtk.Window>(win.Widget);
	});

	[Fact]
	public void Window_with_Grid_and_TextBlock_contains_GtkLabel() => Run(() =>
	{
		var win = new WpfWindow { Title = "Smoke" };
		var grid = new WpfGrid();
		var tb = new WpfTextBlock { Text = "hi" };
		grid.Children.Add(tb);
		win.Content = grid;

		var gtkWin = (Gtk.Window)win.Widget;
		var gtkGrid = (Gtk.Grid)grid.Widget;
		var label = (Gtk.Label)tb.Widget;

		Assert.Equal("hi", label.Text);
		Assert.IsType<Gtk.Window>(gtkWin);
		Assert.IsAssignableFrom<Gtk.Grid>(gtkGrid);
		Assert.IsAssignableFrom<Gtk.Label>(label);
	});

	[Fact]
	public void Button_Click_raises_event() => Run(() =>
	{
		var btn = new WpfButton();
		btn.Content = "Go";
		// Materialise the widget so the GTK Clicked signal is wired.
		_ = btn.Widget;

		bool clicked = false;
		btn.Click += (s, e) => { clicked = true; };

		// Emit the native GTK "clicked" signal — the same path a real user click takes.
		GLib.Signal.Emit((Gtk.Button)btn.Widget, "clicked");

		Assert.True(clicked);
	});

	[Fact]
	public void TextBlock_Text_change_updates_GtkLabel() => Run(() =>
	{
		var tb = new WpfTextBlock { Text = "before" };
		var label = (Gtk.Label)tb.Widget;

		tb.Text = "after";

		Assert.Equal("after", label.Text);
	});

	/// <summary>#5: Content set before Show() must not throw a double-add.</summary>
	[Fact]
	public void Window_Show_with_Content_set_first_does_not_throw() => Run(() =>
	{
		var win = new WpfWindow { Title = "DoubleParentTest" };
		var grid = new WpfGrid();
		win.Content = grid; // Content set BEFORE Show() — parenting via OnPropertyChanged

		// Show() must not add the widget a second time (GTK throws on double-add).
		var ex = Record.Exception(() => win.Show());

		Assert.Null(ex);
	});

	/// <summary>#5: Content set after Show() must not throw a double-add.</summary>
	[Fact]
	public void Window_Show_then_set_Content_does_not_throw() => Run(() =>
	{
		var win = new WpfWindow { Title = "DoubleParentTest2" };
		win.Show(); // widget exists, no content yet

		var grid = new WpfGrid();
		var ex = Record.Exception(() => win.Content = grid);

		Assert.Null(ex);
	});
}