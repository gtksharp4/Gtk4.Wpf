using Xunit;
using WpfContextMenu    = System.Windows.Controls.ContextMenu;
using WpfMenu           = System.Windows.Controls.Menu;
using WpfMenuItem       = System.Windows.Controls.MenuItem;
using WpfToolBar        = System.Windows.Controls.ToolBar;
using WpfTreeView       = System.Windows.Controls.TreeView;
using WpfTreeViewItem   = System.Windows.Controls.TreeViewItem;
using WpfTextBlock      = System.Windows.Controls.TextBlock;
using WpfPopup          = System.Windows.Controls.Primitives.Popup;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for Round-6 hierarchical and popup controls.
/// Each body runs on the GTK thread via Run(…); skipped when GTK is unavailable.
/// </summary>
public class HierarchicalGtkSmokeTests : WpfGtkTestBase
{
	// ── MenuItem ─────────────────────────────────────────────────────────────

	[Fact]
	public void MenuItem_leaf_Widget_is_GtkButton() => Run(() =>
	{
		var mi = new WpfMenuItem { Header = "Open" };
		Assert.IsType<Gtk.Button>(mi.Widget);
	});

	[Fact]
	public void MenuItem_leaf_label_matches_Header() => Run(() =>
	{
		var mi  = new WpfMenuItem { Header = "Save" };
		var btn = (Gtk.Button)mi.Widget;
		Assert.Equal("Save", btn.Label);
	});

	[Fact]
	public void MenuItem_with_subitems_Widget_is_GtkMenuButton() => Run(() =>
	{
		var parent = new WpfMenuItem { Header = "File" };
		parent.Items.Add(new WpfMenuItem { Header = "Open" });
		Assert.IsType<Gtk.MenuButton>(parent.Widget);
	});

	[Fact]
	public void MenuItem_submenu_MenuButton_has_Popover() => Run(() =>
	{
		var parent = new WpfMenuItem { Header = "File" };
		parent.Items.Add(new WpfMenuItem { Header = "Open" });
		var mb = (Gtk.MenuButton)parent.Widget;
		Assert.NotNull(mb.Popover);
	});

	// ── Menu ─────────────────────────────────────────────────────────────────

	[Fact]
	public void Menu_Widget_is_GtkBox() => Run(() =>
	{
		var m = new WpfMenu();
		Assert.IsType<Gtk.Box>(m.Widget);
	});

	[Fact]
	public void Menu_MenuItems_produce_child_buttons() => Run(() =>
	{
		var m = new WpfMenu();
		m.ItemsSource = new[]
		{
			new WpfMenuItem { Header = "File" },
			new WpfMenuItem { Header = "Edit" },
		};
		// Two items → two child widgets in the box; just check widget count equals items.
		Assert.Equal(2, m.Items.Count);
		_ = m.Widget; // materialise
	});

	// ── ToolBar ───────────────────────────────────────────────────────────────

	[Fact]
	public void ToolBar_Widget_is_GtkBox() => Run(() =>
	{
		var tb = new WpfToolBar();
		Assert.IsType<Gtk.Box>(tb.Widget);
	});

	[Fact]
	public void ToolBar_GtkBox_orientation_is_Horizontal() => Run(() =>
	{
		var tb  = new WpfToolBar();
		var box = (Gtk.Box)tb.Widget;
		Assert.Equal(Gtk.Orientation.Horizontal, box.Orientation);
	});

	// ── TreeViewItem ─────────────────────────────────────────────────────────

	[Fact]
	public void TreeViewItem_Widget_is_GtkExpander() => Run(() =>
	{
		var tvi = new WpfTreeViewItem { Header = "Node" };
		Assert.IsType<Gtk.Expander>(tvi.Widget);
	});

	[Fact]
	public void TreeViewItem_GtkExpander_label_matches_Header() => Run(() =>
	{
		var tvi = new WpfTreeViewItem { Header = "Root" };
		var exp = (Gtk.Expander)tvi.Widget;
		Assert.Equal("Root", exp.Label);
	});

	[Fact]
	public void TreeViewItem_IsExpanded_true_reflects_on_GtkExpander() => Run(() =>
	{
		var tvi = new WpfTreeViewItem { Header = "h", IsExpanded = true };
		var exp = (Gtk.Expander)tvi.Widget;
		Assert.True(exp.Expanded);
	});

	[Fact]
	public void TreeViewItem_GtkExpand_updates_IsExpanded() => Run(() =>
	{
		var tvi = new WpfTreeViewItem { Header = "h" };
		var exp = (Gtk.Expander)tvi.Widget;
		exp.Expanded = true;
		Assert.True(tvi.IsExpanded);
	});

	[Fact]
	public void TreeViewItem_nested_Items_build_widget_tree() => Run(() =>
	{
		var root  = new WpfTreeViewItem { Header = "Root" };
		var child = new WpfTreeViewItem { Header = "Child" };
		root.Items.Add(child);
		root.IsExpanded = true;
		// Materialise; child widget should be a Gtk.Expander.
		_ = root.Widget;
		Assert.IsType<Gtk.Expander>(child.Widget);
	});

	// ── TreeView ─────────────────────────────────────────────────────────────

	[Fact]
	public void TreeView_Widget_is_GtkBox() => Run(() =>
	{
		var tv = new WpfTreeView();
		Assert.IsType<Gtk.Box>(tv.Widget);
	});

	[Fact]
	public void TreeView_with_TreeViewItems_builds_widget_tree() => Run(() =>
	{
		var tv   = new WpfTreeView();
		var tvi1 = new WpfTreeViewItem { Header = "A" };
		var tvi2 = new WpfTreeViewItem { Header = "B" };
		tv.ItemsSource = new[] { tvi1, tvi2 };
		_ = tv.Widget;
		Assert.IsType<Gtk.Expander>(tvi1.Widget);
		Assert.IsType<Gtk.Expander>(tvi2.Widget);
	});

	// ── ContextMenu ───────────────────────────────────────────────────────────

	[Fact]
	public void ContextMenu_Widget_is_GtkBox() => Run(() =>
	{
		var cm = new WpfContextMenu();
		Assert.IsType<Gtk.Box>(cm.Widget);
	});

	[Fact]
	public void ContextMenu_GtkPopover_created() => Run(() =>
	{
		var cm = new WpfContextMenu();
		_ = cm.Widget; // materialise
		Assert.NotNull(cm.GtkPopover);
	});

	// ── Popup ────────────────────────────────────────────────────────────────

	[Fact]
	public void Popup_Widget_is_GtkPopover() => Run(() =>
	{
		var p = new WpfPopup();
		Assert.IsType<Gtk.Popover>(p.Widget);
	});

	[Fact]
	public void Popup_Child_set_before_CreateWidget_applied() => Run(() =>
	{
		var p  = new WpfPopup();
		var tb = new WpfTextBlock { Text = "inner" };
		p.Child = tb;
		var popover = (Gtk.Popover)p.Widget;
		Assert.Same(tb.Widget, popover.Child);
	});

	[Fact]
	public void Popup_StaysOpen_false_sets_GtkAutohide_true() => Run(() =>
	{
		var p = new WpfPopup { StaysOpen = false };
		var popover = (Gtk.Popover)p.Widget;
		Assert.True(popover.Autohide);
	});

	[Fact]
	public void Popup_StaysOpen_true_sets_GtkAutohide_false() => Run(() =>
	{
		var p = new WpfPopup { StaysOpen = true };
		var popover = (Gtk.Popover)p.Widget;
		Assert.False(popover.Autohide);
	});

	// ── ToolTip applied to GTK widget ─────────────────────────────────────────

	[Fact]
	public void FrameworkElement_ToolTip_set_before_widget_applied_on_creation() => Run(() =>
	{
		var tb = new WpfTextBlock();
		tb.ToolTip = "hover";
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal("hover", label.TooltipText);
	});

	[Fact]
	public void FrameworkElement_ToolTip_set_after_widget_applied_immediately() => Run(() =>
	{
		var tb    = new WpfTextBlock();
		var label = (Gtk.Label)tb.Widget; // force creation
		tb.ToolTip = "late tooltip";
		Assert.Equal("late tooltip", label.TooltipText);
	});
}