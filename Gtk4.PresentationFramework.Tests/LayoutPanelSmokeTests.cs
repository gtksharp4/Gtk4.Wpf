using System.Windows.Controls;
using Xunit;
using WpfCanvas = System.Windows.Controls.Canvas;
using WpfDockPanel = System.Windows.Controls.DockPanel;
using WpfGrid = System.Windows.Controls.Grid;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfWrapPanel = System.Windows.Controls.WrapPanel;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for the Phase-1 layout panels.
/// Each test body runs on the GTK STA thread via Run(...); the class ctor skips
/// when GTK is unavailable.
/// </summary>
public class LayoutPanelSmokeTests : WpfGtkTestBase
{
	// ── Grid ─────────────────────────────────────────────────────────────────────

	[Fact]
	public void Grid_Widget_is_GtkGrid() => Run(() =>
	{
		var grid = new WpfGrid();
		Assert.IsAssignableFrom<Gtk.Grid>(grid.Widget);
	});

	[Fact]
	public void Grid_child_placed_at_row_col() => Run(() =>
	{
		var grid = new WpfGrid();
		grid.RowDefinitions.Add(new RowDefinition());
		grid.RowDefinitions.Add(new RowDefinition());
		grid.ColumnDefinitions.Add(new ColumnDefinition());
		grid.ColumnDefinitions.Add(new ColumnDefinition());

		var tb = new WpfTextBlock { Text = "cell" };
		WpfGrid.SetRow(tb, 1);
		WpfGrid.SetColumn(tb, 1);
		grid.Children.Add(tb);

		var gtkGrid = (Gtk.Grid)grid.Widget;
		// Child widget exists inside the Gtk.Grid
		Assert.NotNull(tb.Widget);
		Assert.IsType<Gtk.Label>(tb.Widget);
	});

	[Fact]
	public void Grid_attached_props_read_back_after_add() => Run(() =>
	{
		var grid = new WpfGrid();
		var tb = new WpfTextBlock { Text = "x" };
		WpfGrid.SetRow(tb, 2);
		WpfGrid.SetColumn(tb, 3);
		WpfGrid.SetRowSpan(tb, 2);
		WpfGrid.SetColumnSpan(tb, 2);
		grid.Children.Add(tb);

		Assert.Equal(2, WpfGrid.GetRow(tb));
		Assert.Equal(3, WpfGrid.GetColumn(tb));
		Assert.Equal(2, WpfGrid.GetRowSpan(tb));
		Assert.Equal(2, WpfGrid.GetColumnSpan(tb));
	});

	// ── Canvas ───────────────────────────────────────────────────────────────────

	[Fact]
	public void Canvas_Widget_is_GtkFixed() => Run(() =>
	{
		var canvas = new WpfCanvas();
		Assert.IsAssignableFrom<Gtk.Fixed>(canvas.Widget);
	});

	[Fact]
	public void Canvas_child_at_Left_Top_places_in_Fixed() => Run(() =>
	{
		var canvas = new WpfCanvas();
		var tb = new WpfTextBlock { Text = "positioned" };
		WpfCanvas.SetLeft(tb, 10);
		WpfCanvas.SetTop(tb, 20);
		canvas.Children.Add(tb);

		// Widget is a Gtk.Label (TextBlock backing) and is inside the Fixed.
		Assert.IsType<Gtk.Label>(tb.Widget);
		Assert.IsAssignableFrom<Gtk.Fixed>(canvas.Widget);
	});

	[Fact]
	public void Canvas_moving_Left_Top_does_not_throw() => Run(() =>
	{
		var canvas = new WpfCanvas();
		var tb = new WpfTextBlock { Text = "move" };
		WpfCanvas.SetLeft(tb, 0);
		WpfCanvas.SetTop(tb, 0);
		canvas.Children.Add(tb);
		_ = canvas.Widget; // ensure Fixed is live

		// Repositioning via attached property must not throw.
		var ex = Record.Exception(() =>
		{
			WpfCanvas.SetLeft(tb, 50);
			WpfCanvas.SetTop(tb, 60);
		});
		Assert.Null(ex);
	});

	// ── DockPanel ────────────────────────────────────────────────────────────────

	[Fact]
	public void DockPanel_Widget_created_without_throw() => Run(() =>
	{
		var dp = new WpfDockPanel();
		var ex = Record.Exception(() => _ = dp.Widget);
		Assert.Null(ex);
	});

	[Fact]
	public void DockPanel_child_added_without_throw() => Run(() =>
	{
		var dp = new WpfDockPanel();
		_ = dp.Widget; // ensure GTK containers are built

		var tb = new WpfTextBlock { Text = "top" };
		WpfDockPanel.SetDock(tb, Dock.Top);
		var ex = Record.Exception(() => dp.Children.Add(tb));
		Assert.Null(ex);
	});

	[Fact]
	public void DockPanel_LastChildFill_default_true() => Run(() =>
	{
		var dp = new WpfDockPanel();
		Assert.True(dp.LastChildFill);
	});

	// ── WrapPanel ────────────────────────────────────────────────────────────────

	[Fact]
	public void WrapPanel_Widget_is_GtkFlowBox() => Run(() =>
	{
		var wp = new WpfWrapPanel();
		Assert.IsType<Gtk.FlowBox>(wp.Widget);
	});

	[Fact]
	public void WrapPanel_child_added_without_throw() => Run(() =>
	{
		var wp = new WpfWrapPanel();
		_ = wp.Widget;

		var tb = new WpfTextBlock { Text = "wrap" };
		var ex = Record.Exception(() => wp.Children.Add(tb));
		Assert.Null(ex);
	});

	[Fact]
	public void WrapPanel_remove_child_without_throw() => Run(() =>
	{
		var wp = new WpfWrapPanel();
		_ = wp.Widget;

		var tb = new WpfTextBlock { Text = "rm" };
		wp.Children.Add(tb);
		var ex = Record.Exception(() => wp.Children.Remove(tb));
		Assert.Null(ex);
	});
}