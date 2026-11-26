using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

/// <summary>Peers of the Phase 4 controls: type, name, tree and patterns, on real widgets.</summary>
public class BreadthPeerTests : WpfGtkTestBase
{
	static AutomationPeer PeerOf(UIElement e) => UIElementAutomationPeer.CreatePeerForElement(e)!;

	static T Pattern<T>(AutomationPeer p, PatternInterface kind) where T : class => (T)p.GetPattern(kind)!;

	[Fact]
	public void ListView_reports_ListView_with_ListViewItem_children() => Run(() =>
	{
		var list = new ListView();
		list.Items.Add("a");
		list.Items.Add("b");
		var peer = PeerOf(list);
		Assert.IsType<ListViewAutomationPeer>(peer);
		Assert.Equal("ListView", peer.GetClassName());
		Assert.Equal(AutomationControlType.List, peer.GetAutomationControlType());
		var items = peer.GetChildren()!;
		Assert.All(items, i => Assert.Equal("ListViewItem", i.GetClassName()));
		Pattern<ISelectionItemProvider>(items[1], PatternInterface.SelectionItem).Select();
		Assert.Equal("b", list.SelectedItem);
	});

	[Fact]
	public void TabControl_items_are_tabs_followed_by_the_selected_content() => Run(() =>
	{
		var button = new Button { Content = "inside" };
		var tabs = new TabControl();
		tabs.Items.Add(new TabItem { Header = "One", Content = button });
		tabs.Items.Add(new TabItem { Header = "Two", Content = new CheckBox { Content = "other" } });
		var peer = PeerOf(tabs);
		Assert.Equal(AutomationControlType.Tab, peer.GetAutomationControlType());
		tabs.SelectedIndex = 0;
		var children = peer.GetChildren()!;
		Assert.Equal("One,Two,inside", string.Join(",", children.ConvertAll(c => c.GetName())));
		Assert.Equal(AutomationControlType.TabItem, children[0].GetAutomationControlType());
		Assert.True(Pattern<ISelectionProvider>(peer, PatternInterface.Selection).IsSelectionRequired);

		Pattern<ISelectionItemProvider>(children[1], PatternInterface.SelectionItem).Select();
		Assert.Equal(1, tabs.SelectedIndex);
		Assert.True(Pattern<ISelectionItemProvider>(peer.GetChildren()![1], PatternInterface.SelectionItem).IsSelected);
		Assert.Equal("other", peer.GetChildren()![2].GetName());
	});

	[Fact]
	public void TreeView_nodes_expand_select_and_report_leaf_state() => Run(() =>
	{
		var leaf = new TreeViewItem { Header = "leaf" };
		var root = new TreeViewItem { Header = "root" };
		root.Items.Add(leaf);
		var tree = new TreeView();
		tree.Items.Add(root);
		var treePeer = PeerOf(tree);
		Assert.Equal(AutomationControlType.Tree, treePeer.GetAutomationControlType());
		var rootPeer = treePeer.GetChildren()![0];
		Assert.Equal("root", rootPeer.GetName());
		Assert.Equal(AutomationControlType.TreeItem, rootPeer.GetAutomationControlType());

		var expand = Pattern<IExpandCollapseProvider>(rootPeer, PatternInterface.ExpandCollapse);
		Assert.Equal(ExpandCollapseState.Collapsed, expand.ExpandCollapseState);
		expand.Expand();
		Assert.True(root.IsExpanded);
		Assert.Equal(ExpandCollapseState.Expanded, expand.ExpandCollapseState);

		var leafPeer = rootPeer.GetChildren()![0];
		var leafExpand = Pattern<IExpandCollapseProvider>(leafPeer, PatternInterface.ExpandCollapse);
		Assert.Equal(ExpandCollapseState.LeafNode, leafExpand.ExpandCollapseState);
		Assert.Throws<InvalidOperationException>(leafExpand.Expand);

		var select = Pattern<ISelectionItemProvider>(rootPeer, PatternInterface.SelectionItem);
		select.Select();
		Assert.Same(root, tree.SelectedItem);
		Assert.True(select.IsSelected);
		Assert.Single(Pattern<ISelectionProvider>(treePeer, PatternInterface.Selection).GetSelection());
	});

	[Fact]
	public void Menu_items_invoke_toggle_and_expose_sub_items() => Run(() =>
	{
		int clicks = 0;
		var open = new MenuItem { Header = "_Open" };
		open.Click += (s, e) => clicks++;
		var check = new MenuItem { Header = "Wrap", IsCheckable = true };
		var file = new MenuItem { Header = "File" };
		file.Items.Add(open);
		file.Items.Add(check);
		var menu = new Menu();
		menu.Items.Add(file);
		var menuPeer = PeerOf(menu);
		Assert.Equal(AutomationControlType.MenuBar, menuPeer.GetAutomationControlType());
		var filePeer = menuPeer.GetChildren()![0];
		Assert.Equal(AutomationControlType.MenuItem, filePeer.GetAutomationControlType());
		Assert.Null(filePeer.GetPattern(PatternInterface.Invoke));
		Assert.NotNull(filePeer.GetPattern(PatternInterface.ExpandCollapse));

		var subs = filePeer.GetChildren()!;
		Assert.Equal(new[] { "Open", "Wrap" }, subs.ConvertAll(s => s.GetName()).ToArray());
		Pattern<IInvokeProvider>(subs[0], PatternInterface.Invoke).Invoke();
		Assert.True(GtkPump.Until(() => clicks == 1));
		Assert.Null(subs[0].GetPattern(PatternInterface.Toggle));

		var toggle = Pattern<IToggleProvider>(subs[1], PatternInterface.Toggle);
		toggle.Toggle();
		Assert.True(check.IsChecked);
		Assert.Equal(ToggleState.On, toggle.ToggleState);
	});

	[Fact]
	public void ProgressBar_is_a_read_only_range_and_indeterminate_has_none() => Run(() =>
	{
		var bar = new ProgressBar { Minimum = 0, Maximum = 10, Value = 4 };
		var peer = PeerOf(bar);
		Assert.Equal(AutomationControlType.ProgressBar, peer.GetAutomationControlType());
		var range = Pattern<IRangeValueProvider>(peer, PatternInterface.RangeValue);
		Assert.True(range.IsReadOnly);
		Assert.Equal(4, range.Value);
		Assert.Equal(10, range.Maximum);
		Assert.True(double.IsNaN(range.LargeChange));
		Assert.Throws<InvalidOperationException>(() => range.SetValue(5));
		bar.IsIndeterminate = true;
		Assert.Null(peer.GetPattern(PatternInterface.RangeValue));
	});

	[Fact]
	public void ScrollViewer_scroll_pattern_reports_percent_and_validates() => Run(() =>
	{
		var viewer = new ScrollViewer { Width = 100, Height = 100, Content = new Border { Width = 400, Height = 800 } };
		var window = new Window { Content = viewer, Width = 300, Height = 300 };
		window.Show();
		Assert.True(GtkPump.Until(() => viewer.ScrollableHeight > 0));
		var peer = PeerOf(viewer);
		Assert.Equal("ScrollViewer", peer.GetClassName());
		var scroll = Pattern<IScrollProvider>(peer, PatternInterface.Scroll);
		Assert.True(scroll.VerticallyScrollable);
		Assert.Equal(0, scroll.VerticalScrollPercent);
		scroll.SetScrollPercent(ScrollPatternIdentifiers.NoScroll, 50);
		Assert.Equal(50, scroll.VerticalScrollPercent, 1);
		Assert.Throws<ArgumentOutOfRangeException>(() => scroll.SetScrollPercent(0, 101));
		scroll.Scroll(ScrollAmount.NoAmount, ScrollAmount.LargeIncrement);
		Assert.True(scroll.VerticalScrollPercent > 50);
		window.Close();
	});

	[Fact]
	public void Image_Expander_GroupBox_StatusBar_ToolBar_report_their_control_types() => Run(() =>
	{
		Assert.Equal(AutomationControlType.Image, PeerOf(new Image()).GetAutomationControlType());
		var expander = new Expander { Header = "More" };
		var peer = PeerOf(expander);
		Assert.Equal("More", peer.GetName());
		Assert.Equal(AutomationControlType.Group, peer.GetAutomationControlType());
		var state = Pattern<IExpandCollapseProvider>(peer, PatternInterface.ExpandCollapse);
		state.Expand();
		Assert.True(expander.IsExpanded);
		Assert.Equal("Box", PeerOf(new GroupBox { Header = "Box" }).GetName());
		Assert.Equal(AutomationControlType.StatusBar, PeerOf(new StatusBar()).GetAutomationControlType());
		var tools = new ToolBar();
		tools.Items.Add(new Button { Content = "Cut" });
		var toolPeer = PeerOf(tools);
		Assert.Equal(AutomationControlType.ToolBar, toolPeer.GetAutomationControlType());
		Assert.Equal("Cut", toolPeer.GetChildren()![0].GetName());
	});

	[Fact]
	public void Hyperlink_is_a_child_of_its_TextBlock_and_invokes() => Run(() =>
	{
		var link = new Hyperlink();
		link.Inlines.Add(new Run { Text = "docs" });
		int clicks = 0;
		link.Click += (s, e) => clicks++;
		var block = new TextBlock();
		block.Inlines.Add(new Run { Text = "see " });
		block.Inlines.Add(link);
		var peer = PeerOf(block);
		var child = Assert.Single(peer.GetChildren()!);
		Assert.Equal(AutomationControlType.Hyperlink, child.GetAutomationControlType());
		Assert.Equal("docs", child.GetName());
		Assert.Same(peer, child.GetParent());
		Pattern<IInvokeProvider>(child, PatternInterface.Invoke).Invoke();
		Assert.True(GtkPump.Until(() => clicks == 1));
	});
}
