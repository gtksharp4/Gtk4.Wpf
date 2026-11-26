using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

/// <summary>Phase 4 review fixes: selection items, ScrollViewer view, DataGrid events and label relations.</summary>
public class Phase4FixTests : WpfGtkTestBase
{
	public sealed class Person
	{
		public string Name { get; set; } = "";
	}

	static AutomationPeer PeerOf(UIElement e) => UIElementAutomationPeer.CreatePeerForElement(e)!;

	static DataGrid Grid(params Person[] people)
	{
		var grid = new DataGrid { AutoGenerateColumns = false };
		grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name") });
		grid.ItemsSource = new ObservableCollection<Person>(people);
		_ = grid.Widget;
		return grid;
	}

	[Fact]
	public void Row_AddToSelection_selects_when_nothing_or_itself_is_selected() => Run(() =>
	{
		var people = new[] { new Person { Name = "Ada" }, new Person { Name = "Alan" } };
		var grid = Grid(people);
		var rows = PeerOf(grid).GetChildren()!.FindAll(c => c.GetClassName() == "DataGridRow");
		var first = (ISelectionItemProvider)rows[0].GetPattern(PatternInterface.SelectionItem)!;
		first.AddToSelection();
		Assert.Same(people[0], grid.SelectedItem);
		first.AddToSelection();
		var second = (ISelectionItemProvider)rows[1].GetPattern(PatternInterface.SelectionItem)!;
		Assert.Throws<InvalidOperationException>(second.AddToSelection);
	});

	[Fact]
	public void TreeViewItem_AddToSelection_selects_when_nothing_is_selected() => Run(() =>
	{
		var a = new TreeViewItem { Header = "a" };
		var b = new TreeViewItem { Header = "b" };
		var tree = new TreeView();
		tree.Items.Add(a);
		tree.Items.Add(b);
		_ = tree.Widget;
		var provider = (ISelectionItemProvider)PeerOf(a).GetPattern(PatternInterface.SelectionItem)!;
		provider.AddToSelection();
		Assert.True(provider.IsSelected);
		var other = (ISelectionItemProvider)PeerOf(b).GetPattern(PatternInterface.SelectionItem)!;
		Assert.Throws<InvalidOperationException>(other.AddToSelection);
	});

	[Fact]
	public void A_standalone_ScrollViewer_is_in_the_control_view() => Run(() =>
	{
		var viewer = new ScrollViewer { Content = new Border() };
		Assert.True(PeerOf(viewer).IsControlElement());
	});

	[Fact]
	public void DataGrid_clearing_the_selection_and_sorting_raise_their_events() => Run(() =>
	{
		var people = new[] { new Person { Name = "b" }, new Person { Name = "a" } };
		var grid = Grid(people);
		var peer = PeerOf(grid);
		peer.GetChildren();
		using var log = new EventLog();
		grid.SelectedItem = people[0];
		log.Entries.Clear();
		grid.SelectedItem = null;
		Assert.Contains(log.Entries, e => e.StartsWith("SelectionItemPatternOnElementRemovedFromSelection"));
		log.Entries.Clear();
		grid.ToggleSort(grid.Columns[0]);
		Assert.Contains(log.Entries, e => e.StartsWith("StructureChanged"));
	});

	[Fact]
	public void LabeledBy_reaches_a_field_whose_widget_was_created_before_the_label() => Run(() =>
	{
		var label = new Label { Content = "Name" };
		var box = new TextBox();
		AutomationProperties.SetLabeledBy(box, label);
		var target = box.FocusWidget;
		PeerOf(box);
		Assert.False(GtkA11yProbe.HasRelation(target, Gtk.AccessibleRelation.LabelledBy));
		_ = label.Widget;
		Assert.True(GtkA11yProbe.HasRelation(target, Gtk.AccessibleRelation.LabelledBy));
		Assert.False(GtkA11yProbe.RelationTargetEquals(target, Gtk.AccessibleRelation.LabelledBy, target));
		Assert.True(GtkA11yProbe.RelationTargetEquals(
			target, Gtk.AccessibleRelation.LabelledBy, ((UIElementAutomationPeer)PeerOf(label)).Target(label.Widget)));
	});
}
