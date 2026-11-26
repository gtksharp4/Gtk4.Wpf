using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

public partial class PatternTests
{
	[Fact]
	public void ComboBox_selection_value_and_expand_collapse() => Run(() =>
	{
		var combo = new ComboBox();
		combo.Items.Add("one");
		combo.Items.Add("two");
		var peer = UIElementAutomationPeer.CreatePeerForElement(combo)!;
		var items = peer.GetChildren()!;
		Assert.Equal(new[] { "one", "two" }, new[] { items[0].GetName(), items[1].GetName() });

		var item = (ISelectionItemProvider)items[1].GetPattern(PatternInterface.SelectionItem)!;
		item.Select();
		Assert.Equal("two", combo.SelectedItem);
		Assert.Equal(1, combo.SelectedIndex);
		Assert.True(item.IsSelected);
		var selection = (ISelectionProvider)peer.GetPattern(PatternInterface.Selection)!;
		Assert.Single(selection.GetSelection());
		Assert.False(selection.CanSelectMultiple);

		var expand = (IExpandCollapseProvider)peer.GetPattern(PatternInterface.ExpandCollapse)!;
		Assert.Equal(ExpandCollapseState.Collapsed, expand.ExpandCollapseState);
		expand.Expand();
		Assert.True(combo.IsDropDownOpen);
		Assert.Equal(ExpandCollapseState.Expanded, expand.ExpandCollapseState);
		expand.Collapse();
		Assert.False(combo.IsDropDownOpen);

		Assert.Null(peer.GetPattern(PatternInterface.Value));
	});

	[Fact]
	public void Editable_ComboBox_exposes_Value_that_follows_WPF() => Run(() =>
	{
		var combo = new ComboBox { IsEditable = true };
		var peer = UIElementAutomationPeer.CreatePeerForElement(combo)!;
		var value = (IValueProvider)peer.GetPattern(PatternInterface.Value)!;
		Assert.False(value.IsReadOnly);
		value.SetValue("typed");
		Assert.Equal("typed", combo.Text);
		Assert.Equal("typed", value.Value);
		Assert.Throws<ArgumentNullException>(() => value.SetValue(null!));
		combo.IsReadOnly = true;
		Assert.True(value.IsReadOnly);
		combo.IsReadOnly = false;
		combo.IsEnabled = false;
		Assert.True(value.IsReadOnly);
		Assert.Throws<ElementNotEnabledException>(() => value.SetValue("x"));
	});

	[Fact]
	public void ListBox_items_select_through_the_provider_and_change_the_widget() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add("a");
		list.Items.Add("b");
		list.Items.Add("c");
		var window = new Window { Content = list };
		window.Show();
		Assert.True(GtkPump.Until(() => window.Widget.IsMapped));
		var peer = UIElementAutomationPeer.CreatePeerForElement(list)!;
		var items = peer.GetChildren()!;
		Assert.Equal(3, items.Count);
		Assert.Same(items[0], peer.GetChildren()![0]);
		Assert.Same(peer, items[0].GetParent());

		var b = (ISelectionItemProvider)items[1].GetPattern(PatternInterface.SelectionItem)!;
		b.Select();
		Assert.Equal("b", list.SelectedItem);
		Assert.True(b.IsSelected);
		Assert.False(((ISelectionItemProvider)items[0].GetPattern(PatternInterface.SelectionItem)!).IsSelected);
		var selected = ((ISelectionProvider)peer.GetPattern(PatternInterface.Selection)!).GetSelection();
		Assert.Single(selected);
		Assert.Same(items[1], ((PeerProvider)selected[0]).Peer);
		Assert.True(((Gtk.ListBoxRow)list.ContainerAt(1)!.Widget).IsSelected);
		Assert.False(((Gtk.ListBoxRow)list.ContainerAt(0)!.Widget).IsSelected);
	});

	[Fact]
	public void ListBox_multiple_selection_adds_and_removes() => Run(() =>
	{
		var list = new ListBox { SelectionMode = SelectionMode.Multiple };
		list.Items.Add("a");
		list.Items.Add("b");
		var window = new Window { Content = list };
		_ = list.Widget;
		var peer = UIElementAutomationPeer.CreatePeerForElement(list)!;
		var items = peer.GetChildren()!;
		var a = (ISelectionItemProvider)items[0].GetPattern(PatternInterface.SelectionItem)!;
		var b = (ISelectionItemProvider)items[1].GetPattern(PatternInterface.SelectionItem)!;
		a.AddToSelection();
		b.AddToSelection();
		Assert.True(a.IsSelected && b.IsSelected);
		Assert.Equal(2, list.SelectedItems.Count);
		a.RemoveFromSelection();
		Assert.False(a.IsSelected);
		Assert.Single(list.SelectedItems);
		Assert.True(((ISelectionProvider)peer.GetPattern(PatternInterface.Selection)!).CanSelectMultiple);
	});
}
