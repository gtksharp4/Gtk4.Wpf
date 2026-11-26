using Xunit;
using WpfContextMenu    = System.Windows.Controls.ContextMenu;
using WpfFrameworkElement = System.Windows.FrameworkElement;
using WpfMenu           = System.Windows.Controls.Menu;
using WpfMenuItem       = System.Windows.Controls.MenuItem;
using WpfToolBar        = System.Windows.Controls.ToolBar;
using WpfToolTipService = System.Windows.Controls.ToolTipService;
using WpfTreeView       = System.Windows.Controls.TreeView;
using WpfTreeViewItem   = System.Windows.Controls.TreeViewItem;
using WpfTextBlock      = System.Windows.Controls.TextBlock;
using WpfPopup          = System.Windows.Controls.Primitives.Popup;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure DP tests for TreeView, Menu, ToolBar, ContextMenu, Popup, ToolTip, ToolTipService.
/// No GTK required; each test fails without the feature.
/// </summary>
public class HierarchicalDpTests2
{
	// ── TreeView ──────────────────────────────────────────────────────────────

	[Fact]
	public void TreeView_SelectedItem_default_null()
	{
		var tv = new WpfTreeView();
		Assert.Null(tv.SelectedItem);
	}

	[Fact]
	public void TreeView_SelectedItem_set_roundtrip()
	{
		var tv  = new WpfTreeView();
		var tvi = new WpfTreeViewItem { Header = "X" };
		tv.ItemsSource = new[] { tvi };
		tv.SelectedItem = tvi;
		Assert.Same(tvi, tv.SelectedItem);
	}

	[Fact]
	public void TreeView_SelectedItemChanged_fires()
	{
		var tv  = new WpfTreeView();
		var tvi = new WpfTreeViewItem { Header = "X" };
		tv.ItemsSource = new[] { tvi };
		bool fired = false;
		tv.SelectedItemChanged += (s, e) => { fired = true; };
		tv.SelectedItem = tvi;
		Assert.True(fired);
	}

	[Fact]
	public void TreeView_SelectedItem_marks_TreeViewItem_IsSelected()
	{
		var tv  = new WpfTreeView();
		var tvi = new WpfTreeViewItem { Header = "Node" };
		tv.ItemsSource = new[] { tvi };
		tv.SelectedItem = tvi;
		Assert.True(tvi.IsSelected);
	}

	[Fact]
	public void TreeView_SelectedItem_change_deselects_old()
	{
		var tv   = new WpfTreeView();
		var tvi1 = new WpfTreeViewItem { Header = "A" };
		var tvi2 = new WpfTreeViewItem { Header = "B" };
		tv.ItemsSource = new[] { tvi1, tvi2 };
		tv.SelectedItem = tvi1;
		tv.SelectedItem = tvi2;
		Assert.False(tvi1.IsSelected);
		Assert.True(tvi2.IsSelected);
	}

	// ── Menu ──────────────────────────────────────────────────────────────────

	[Fact]
	public void Menu_Items_starts_empty()
	{
		var m = new WpfMenu();
		Assert.Empty(m.Items);
	}

	[Fact]
	public void Menu_ItemsSource_adds_MenuItems()
	{
		var m = new WpfMenu();
		m.ItemsSource = new[]
		{
			new WpfMenuItem { Header = "File" },
			new WpfMenuItem { Header = "Edit" },
		};
		Assert.Equal(2, m.Items.Count);
	}

	// ── ToolBar ───────────────────────────────────────────────────────────────

	[Fact]
	public void ToolBar_Items_starts_empty()
	{
		var tb = new WpfToolBar();
		Assert.Empty(tb.Items);
	}

	[Fact]
	public void ToolBar_Header_roundtrip()
	{
		var tb = new WpfToolBar { Header = "Main" };
		Assert.Equal("Main", tb.Header);
	}

	[Fact]
	public void ToolBar_ItemsSource_populates()
	{
		var tb = new WpfToolBar();
		tb.ItemsSource = new[] { new WpfTextBlock { Text = "New" } };
		Assert.Single(tb.Items);
	}

	// ── ContextMenu ───────────────────────────────────────────────────────────

	[Fact]
	public void ContextMenu_IsOpen_default_false()
	{
		var cm = new WpfContextMenu();
		Assert.False(cm.IsOpen);
	}

	[Fact]
	public void ContextMenu_IsOpen_roundtrip_pure()
	{
		// Pure DP set without GTK — just verifies the DP is wired.
		var cm = new WpfContextMenu();
		cm.SetValue(WpfContextMenu.IsOpenProperty, true);
		Assert.True(cm.IsOpen);
	}

	[Fact]
	public void ContextMenu_Items_populates()
	{
		var cm = new WpfContextMenu();
		cm.ItemsSource = new[]
		{
			new WpfMenuItem { Header = "Copy" },
			new WpfMenuItem { Header = "Paste" },
		};
		Assert.Equal(2, cm.Items.Count);
	}

	[Fact]
	public void FrameworkElement_ContextMenu_attached_dp_roundtrip()
	{
		var tb = new WpfTextBlock();
		var cm = new WpfContextMenu();
		WpfFrameworkElement.SetContextMenu(tb, cm);
		Assert.Same(cm, WpfFrameworkElement.GetContextMenu(tb));
	}

	[Fact]
	public void FrameworkElement_ContextMenu_instance_property_roundtrip()
	{
		var tb = new WpfTextBlock();
		var cm = new WpfContextMenu();
		tb.ContextMenu = cm;
		Assert.Same(cm, tb.ContextMenu);
	}

	// ── ToolTip / ToolTipService ──────────────────────────────────────────────

	[Fact]
	public void FrameworkElement_ToolTip_attached_dp_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfFrameworkElement.SetToolTip(tb, "hover text");
		Assert.Equal("hover text", WpfFrameworkElement.GetToolTip(tb));
	}

	[Fact]
	public void FrameworkElement_ToolTip_instance_property_roundtrip()
	{
		var tb = new WpfTextBlock();
		tb.ToolTip = "hint";
		Assert.Equal("hint", tb.ToolTip);
	}

	[Fact]
	public void ToolTipService_GetSet_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetToolTip(tb, "service tooltip");
		Assert.Equal("service tooltip", WpfToolTipService.GetToolTip(tb));
	}

	[Fact]
	public void ToolTipService_set_reads_via_FrameworkElement_GetToolTip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetToolTip(tb, "shared dp");
		Assert.Equal("shared dp", WpfFrameworkElement.GetToolTip(tb));
	}

	// ── Popup ────────────────────────────────────────────────────────────────

	[Fact]
	public void Popup_IsOpen_default_false()
	{
		var p = new WpfPopup();
		Assert.False(p.IsOpen);
	}

	[Fact]
	public void Popup_IsOpen_roundtrip_pure()
	{
		var p = new WpfPopup();
		p.SetValue(WpfPopup.IsOpenProperty, true);
		Assert.True(p.IsOpen);
	}

	[Fact]
	public void Popup_Child_roundtrip()
	{
		var p  = new WpfPopup();
		var tb = new WpfTextBlock { Text = "pop" };
		p.Child = tb;
		Assert.Same(tb, p.Child);
	}

	[Fact]
	public void Popup_StaysOpen_default_true()
	{
		var p = new WpfPopup();
		Assert.True(p.StaysOpen);
	}

	[Fact]
	public void Popup_StaysOpen_roundtrip()
	{
		var p = new WpfPopup { StaysOpen = false };
		Assert.False(p.StaysOpen);
	}

	[Fact]
	public void Popup_PlacementTarget_roundtrip()
	{
		var p  = new WpfPopup();
		var tb = new WpfTextBlock { Text = "anchor" };
		p.PlacementTarget = tb;
		Assert.Same(tb, p.PlacementTarget);
	}
}