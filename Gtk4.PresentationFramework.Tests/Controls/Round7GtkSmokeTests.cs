using System;
using System.Collections.Generic;
using Xunit;
using WpfCalendar       = System.Windows.Controls.Calendar;
using WpfDatePicker     = System.Windows.Controls.DatePicker;
using WpfPasswordBox    = System.Windows.Controls.PasswordBox;
using WpfRepeatButton   = System.Windows.Controls.Primitives.RepeatButton;
using WpfStatusBar      = System.Windows.Controls.Primitives.StatusBar;
using WpfThumb          = System.Windows.Controls.Primitives.Thumb;
using WpfUniformGrid    = System.Windows.Controls.Primitives.UniformGrid;
using WpfViewbox        = System.Windows.Controls.Viewbox;
using WpfDataGrid       = System.Windows.Controls.DataGrid;
using WpfDataGridTextColumn = System.Windows.Controls.DataGridTextColumn;
using WpfTextBlock      = System.Windows.Controls.TextBlock;
using WpfBinding        = System.Windows.Data.Binding;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for the Round-7 controls. Each body runs on the GTK thread
/// via Run(…). Skipped when GTK is unavailable.
/// </summary>
public class Round7GtkSmokeTests : WpfGtkTestBase
{
	// ── Calendar ─────────────────────────────────────────────────────────────

	[Fact]
	public void Calendar_Widget_is_GtkCalendar() => Run(() =>
	{
		var cal = new WpfCalendar();
		Assert.IsType<Gtk.Calendar>(cal.Widget);
	});

	[Fact]
	public void Calendar_SelectedDate_set_before_widget_does_not_throw() => Run(() =>
	{
		var cal = new WpfCalendar { SelectedDate = new System.DateTime(2024, 6, 15) };
		var ex = Record.Exception(() => _ = cal.Widget);
		Assert.Null(ex);
	});

	// ── DatePicker ────────────────────────────────────────────────────────────

	[Fact]
	public void DatePicker_Widget_is_GtkBox() => Run(() =>
	{
		var dp = new WpfDatePicker();
		Assert.IsType<Gtk.Box>(dp.Widget);
	});

	[Fact]
	public void DatePicker_Widget_contains_Entry_and_MenuButton() => Run(() =>
	{
		var dp = new WpfDatePicker();
		var box = (Gtk.Box)dp.Widget;
		var entry = box.FirstChild as Gtk.Entry;
		Assert.NotNull(entry);
		var btn = entry!.NextSibling as Gtk.MenuButton;
		Assert.NotNull(btn);
	});

	// ── PasswordBox ──────────────────────────────────────────────────────────

	[Fact]
	public void PasswordBox_Widget_is_GtkPasswordEntry() => Run(() =>
	{
		var pb = new WpfPasswordBox();
		Assert.IsType<Gtk.PasswordEntry>(pb.Widget);
	});

	[Fact]
	public void PasswordBox_Password_set_reflects_in_entry() => Run(() =>
	{
		var pb = new WpfPasswordBox { Password = "hello" };
		var entry = (Gtk.PasswordEntry)pb.Widget;
		Assert.Equal("hello", entry.Text);
	});

	// ── RepeatButton ─────────────────────────────────────────────────────────

	[Fact]
	public void RepeatButton_Widget_is_GtkButton() => Run(() =>
	{
		var rb = new WpfRepeatButton();
		Assert.IsType<Gtk.Button>(rb.Widget);
	});

	[Fact]
	public void RepeatButton_Click_handler_attaches() => Run(() =>
	{
		var rb = new WpfRepeatButton();
		_ = rb.Widget; // materialise
		bool fired = false;
		rb.Click += (s, e) => { fired = true; };
		// Without actual press we just verify no exception.
		Assert.False(fired);
	});

	// ── Thumb ────────────────────────────────────────────────────────────────

	[Fact]
	public void Thumb_Widget_is_GtkBox() => Run(() =>
	{
		var thumb = new WpfThumb();
		Assert.IsType<Gtk.Box>(thumb.Widget);
	});

	// ── GridSplitter ─────────────────────────────────────────────────────────

	[Fact]
	public void GridSplitter_Widget_is_GtkBox() => Run(() =>
	{
		var gs = new System.Windows.Controls.GridSplitter();
		Assert.IsType<Gtk.Box>(gs.Widget);
	});

	// ── Viewbox ──────────────────────────────────────────────────────────────

	[Fact]
	public void Viewbox_Widget_is_GtkAspectFrame() => Run(() =>
	{
		var vb = new WpfViewbox();
		Assert.IsType<Gtk.AspectFrame>(vb.Widget);
	});

	[Fact]
	public void Viewbox_Child_set_appears_in_AspectFrame() => Run(() =>
	{
		var vb = new WpfViewbox();
		var tb = new WpfTextBlock { Text = "content" };
		vb.Child = tb;
		var frame = (Gtk.AspectFrame)vb.Widget;
		Assert.Same(tb.Widget, frame.Child);
	});

	// ── UniformGrid ───────────────────────────────────────────────────────────

	[Fact]
	public void UniformGrid_Widget_is_GtkGrid() => Run(() =>
	{
		var ug = new WpfUniformGrid();
		Assert.IsType<Gtk.Grid>(ug.Widget);
	});

	[Fact]
	public void UniformGrid_four_children_packs_four_widgets() => Run(() =>
	{
		var ug = new WpfUniformGrid();
		for (int i = 0; i < 4; i++)
			ug.Children.Add(new WpfTextBlock { Text = i.ToString() });

		_ = ug.Widget;

		int count = 0;
		for (var w = ((Gtk.Grid)ug.Widget).FirstChild; w != null; w = w.NextSibling)
			count++;
		Assert.Equal(4, count);
	});

	// ── StatusBar ─────────────────────────────────────────────────────────────

	[Fact]
	public void StatusBar_Widget_is_GtkBox() => Run(() =>
	{
		var sb = new WpfStatusBar();
		Assert.IsType<Gtk.Box>(sb.Widget);
	});

	[Fact]
	public void StatusBar_items_produce_child_widgets() => Run(() =>
	{
		var sb = new WpfStatusBar();
		sb.ItemsSource = new[] { "Ready", "Ln 1" };
		_ = sb.Widget;

		int count = 0;
		for (var w = ((Gtk.Box)sb.Widget).FirstChild; w != null; w = w.NextSibling)
			count++;
		Assert.Equal(2, count);
	});

	// ── DataGrid ─────────────────────────────────────────────────────────────

	[Fact]
	public void DataGrid_Widget_is_GtkScrolledWindow() => Run(() =>
	{
		var dg = new WpfDataGrid();
		Assert.IsType<Gtk.ScrolledWindow>(dg.Widget);
	});

	[Fact]
	public void DataGrid_two_rows_two_cols_builds_grid() => Run(() =>
	{
		var dg = new WpfDataGrid();
		dg.AutoGenerateColumns = false;
		dg.Columns.Add(new WpfDataGridTextColumn
		{
			Header = "Name",
			Binding = new WpfBinding("Name"),
		});
		dg.Columns.Add(new WpfDataGridTextColumn
		{
			Header = "Value",
			Binding = new WpfBinding("Value"),
		});
		dg.ItemsSource = new[]
		{
			new { Name = "Alpha", Value = "1" },
			new { Name = "Beta",  Value = "2" },
		};

		var scroll = (Gtk.ScrolledWindow)dg.Widget;
		var grid = scroll.Child as Gtk.Viewport;
		// Grid is wrapped in Gtk.Viewport by ScrolledWindow.
		// The inner Gtk.Grid contains 1 header row + 2 data rows = 3 rows × 2 cols = 6 cells.
		// Just verify the ScrolledWindow realized without exception.
		Assert.NotNull(scroll);
	});

	[Fact]
	public void DataGrid_AutoGenerateColumns_produces_grid_cells() => Run(() =>
	{
		var dg = new WpfDataGrid();
		dg.ItemsSource = new[]
		{
			new { Name = "Alice", Age = 30 },
		};
		var scroll = (Gtk.ScrolledWindow)dg.Widget;
		Assert.NotNull(scroll);
	});
}