using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;
public class BreadthEventTests : WpfGtkTestBase
{
	static AutomationPeer PeerOf(UIElement e) => UIElementAutomationPeer.CreatePeerForElement(e)!;

	static ListBox List(SelectionMode mode, int count)
	{
		var list = new ListBox { SelectionMode = mode };
		for (int i = 0; i < count; i++) list.Items.Add("item" + i);
		_ = list.Widget;
		return list;
	}

	[Fact]
	public void Single_selection_raises_ElementSelected_once_per_change() => Run(() =>
	{
		var list = List(SelectionMode.Single, 3);
		PeerOf(list).GetChildren();
		using var log = new EventLog();
		list.SelectedItem = "item1";
		Assert.Equal(new[] { "SelectionItemPatternOnElementSelected item1" }, log.Entries);
		log.Entries.Clear();
		list.SelectedItem = "item2";
		Assert.Equal(new[] { "SelectionItemPatternOnElementSelected item2" }, log.Entries);
	});

	[Fact]
	public void Multiple_selection_raises_added_then_removed_events() => Run(() =>
	{
		var list = List(SelectionMode.Multiple, 3);
		var peer = PeerOf(list);
		var items = peer.GetChildren()!;
		var select = (ISelectionItemProvider)items[0].GetPattern(PatternInterface.SelectionItem)!;
		var add = (ISelectionItemProvider)items[1].GetPattern(PatternInterface.SelectionItem)!;
		select.Select();
		using var log = new EventLog();
		add.AddToSelection();
		Assert.Equal(new[] { "SelectionItemPatternOnElementAddedToSelection item1" }, log.Entries);
		log.Entries.Clear();
		add.RemoveFromSelection();
		Assert.Equal(new[] { "SelectionItemPatternOnElementRemovedFromSelection item1" }, log.Entries);
	});

	[Fact]
	public void A_large_selection_change_raises_one_Invalidated_event() => Run(() =>
	{
		var list = List(SelectionMode.Extended, 30);
		PeerOf(list).GetChildren();
		using var log = new EventLog();
		list.SelectAll();
		Assert.Equal(new[] { "SelectionPatternOnInvalidated " }, log.Entries);
	});

	[Fact]
	public void Tab_selection_raises_ElementSelected_for_the_tab() => Run(() =>
	{
		var tabs = new TabControl();
		tabs.Items.Add(new TabItem { Header = "One", Content = new Button() });
		tabs.Items.Add(new TabItem { Header = "Two", Content = new Button() });
		var peer = PeerOf(tabs);
		var items = peer.GetChildren()!;
		using var log = new EventLog();
		((ISelectionItemProvider)items[1].GetPattern(PatternInterface.SelectionItem)!).Select();
		Assert.Equal(new[] { "SelectionItemPatternOnElementSelected Two" }, log.Entries);
	});

	[Fact]
	public void Item_changes_raise_the_matching_structure_event_once() => Run(() =>
	{
		var list = List(SelectionMode.Single, 2);
		var peer = PeerOf(list);
		peer.GetChildren();
		using var log = new EventLog();
		list.Items.Add("added");
		Assert.Equal(new[] { "StructureChanged:ChildAdded added" }, log.Entries);
		log.Entries.Clear();
		list.Items.Remove("item0");
		Assert.Equal(new[] { "StructureChanged:ChildRemoved " }, log.Entries);
	});

	[Fact]
	public void Bulk_additions_and_removals_are_reported_as_bulk() => Run(() =>
	{
		var list = new ListBox();
		var peer = PeerOf(list);
		peer.GetChildren();
		using var log = new EventLog();
		list.ItemsSource = new List<string> { "a", "b", "c" };
		Assert.Equal(new[] { "StructureChanged:ChildrenBulkAdded " + peer.GetName() }, log.Entries);
		log.Entries.Clear();
		list.ItemsSource = new List<string>();
		Assert.Equal(new[] { "StructureChanged:ChildrenBulkRemoved " + peer.GetName() }, log.Entries);
	});

	[Fact]
	public void Invoke_raises_Invoked_once_and_a_toggle_raises_only_its_state() => Run(() =>
	{
		var button = new Button { Content = "Go" };
		var check = new CheckBox { Content = "Tick" };
		using var log = new EventLog();
		((IInvokeProvider)PeerOf(button).GetPattern(PatternInterface.Invoke)!).Invoke();
		Assert.True(GtkPump.Until(() => log.Entries.Count > 0));
		Assert.Equal(new[] { "InvokePatternOnInvoked Go" }, log.Entries);
		log.Entries.Clear();
		((IToggleProvider)PeerOf(check).GetPattern(PatternInterface.Toggle)!).Toggle();
		Assert.Equal(new[] { "TogglePatternIdentifiers.ToggleStateProperty Tick Off->On" }, log.Entries);
	});

	[Fact]
	public void Menu_item_invoke_raises_Invoked_and_a_checkable_one_also_raises_its_state() => Run(() =>
	{
		var item = new MenuItem { Header = "Open" };
		var check = new MenuItem { Header = "Wrap", IsCheckable = true };
		using var log = new EventLog();
		((IInvokeProvider)PeerOf(item).GetPattern(PatternInterface.Invoke)!).Invoke();
		Assert.True(GtkPump.Until(() => log.Entries.Count > 0));
		Assert.Equal(new[] { "InvokePatternOnInvoked Open" }, log.Entries);
		log.Entries.Clear();
		((IInvokeProvider)PeerOf(check).GetPattern(PatternInterface.Invoke)!).Invoke();
		Assert.True(GtkPump.Until(() => log.Entries.Count > 1));
		Assert.Equal(new[]
		{
			"TogglePatternIdentifiers.ToggleStateProperty Wrap Off->On", "InvokePatternOnInvoked Wrap",
		}, log.Entries);
	});

	[Fact]
	public void Expander_tree_node_progress_and_scroll_raise_their_property_events() => Run(() =>
	{
		var expander = new Expander { Header = "More" };
		var node = new TreeViewItem { Header = "root" };
		node.Items.Add(new TreeViewItem { Header = "kid" });
		var bar = new ProgressBar { Maximum = 10 };
		PeerOf(expander);
		PeerOf(node);
		PeerOf(bar);
		using var log = new EventLog();
		expander.IsExpanded = true;
		node.IsExpanded = true;
		bar.Value = 3;
		Assert.Equal(new[]
		{
			"ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty More Collapsed->Expanded",
			"ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty root Collapsed->Expanded",
			"RangeValuePatternIdentifiers.ValueProperty  0->3",
		}, log.Entries);
	});

	[Fact]
	public void Text_changes_and_selection_raise_text_events_once() => Run(() =>
	{
		var box = new TextBox { Text = "hello world" };
		PeerOf(box);
		using var log = new EventLog();
		box.Select(0, 5);
		Assert.Equal(new[] { "TextPatternOnTextSelectionChanged " }, log.Entries);
		log.Entries.Clear();
		box.Text = "changed";
		Assert.Equal(1, log.Entries.FindAll(e => e.StartsWith("TextPatternOnTextChanged")).Count);
		Assert.Equal(1, log.Entries.FindAll(e => e.StartsWith("ValuePatternIdentifiers.ValueProperty")).Count);
		var rich = new RichTextBox();
		rich.AppendText("hello world");
		var richText = (ITextProvider)PeerOf(rich).GetPattern(PatternInterface.Text)!;
		log.Entries.Clear();
		richText.DocumentRange.FindText("world", false, false).Select();
		Assert.Equal(new[] { "TextPatternOnTextSelectionChanged " }, log.Entries);
	});

	[Fact]
	public void Focus_changes_raise_one_AutomationFocusChanged_each_in_order() => Run(() =>
	{
		var app = new Application();
		var first = new TextBox { Text = "first" };
		AutomationProperties.SetName(first, "first");
		var second = new Button { Content = "second" };
		var panel = new StackPanel();
		panel.Children.Add(first);
		panel.Children.Add(second);
		var window = new Window { Content = panel, Width = 300, Height = 200 };
		window.Show();
		Assert.True(GtkPump.Until(() => second.Widget.Width > 0));
		using var log = new EventLog();
		PeerOf(second).SetFocus();
		PeerOf(first).SetFocus();
		var focus = log.Entries.FindAll(e => e.StartsWith("AutomationFocusChanged"));
		Assert.Equal(new[] { "AutomationFocusChanged second", "AutomationFocusChanged first" }, focus);
		window.Close();
		Assert.NotNull(app);
	});
}
