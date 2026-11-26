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

public class DataGridPeerTests : WpfGtkTestBase
{
	public sealed class Person
	{
		public string Name { get; set; } = "";

		public int Age { get; set; }
	}

	static DataGrid Grid(params Person[] people)
	{
		var grid = new DataGrid { AutoGenerateColumns = false };
		grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name") });
		grid.Columns.Add(new DataGridTextColumn { Header = "Age", Binding = new System.Windows.Data.Binding("Age") });
		grid.ItemsSource = new ObservableCollection<Person>(people);
		return grid;
	}

	static Person[] Two() => new[] { new Person { Name = "Ada", Age = 36 }, new Person { Name = "Alan", Age = 41 } };

	[Fact]
	public void Grid_and_table_patterns_report_size_cells_and_headers() => Run(() =>
	{
		var grid = Grid(Two());
		var peer = UIElementAutomationPeer.CreatePeerForElement(grid)!;
		Assert.Equal(AutomationControlType.DataGrid, peer.GetAutomationControlType());
		var provider = (IGridProvider)peer.GetPattern(PatternInterface.Grid)!;
		Assert.Equal(2, provider.RowCount);
		Assert.Equal(2, provider.ColumnCount);

		var cell = provider.GetItem(1, 0);
		var cellPeer = ((IRawElementProviderSimple)cell).GetPatternProvider(GridItemPatternIdentifiers.Pattern.Id);
		var item = (IGridItemProvider)cellPeer!;
		Assert.Equal(1, item.Row);
		Assert.Equal(0, item.Column);
		Assert.Equal(1, item.RowSpan);
		Assert.Equal(1, item.ColumnSpan);
		Assert.Equal("Alan", ((IValueProvider)cellPeer!).Value);
		Assert.Throws<ArgumentOutOfRangeException>(() => provider.GetItem(2, 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => provider.GetItem(0, -1));

		var table = (ITableProvider)peer.GetPattern(PatternInterface.Table)!;
		Assert.Equal(RowOrColumnMajor.RowMajor, table.RowOrColumnMajor);
		Assert.Equal(2, table.GetColumnHeaders().Length);
		Assert.Empty(table.GetRowHeaders());
		var headers = ((ITableItemProvider)cellPeer).GetColumnHeaderItems();
		var header = Assert.Single(headers);
		Assert.Equal("Name", header.GetPropertyValue(AutomationElementIdentifiers.NameProperty.Id));
	});

	[Fact]
	public void Rows_are_data_items_with_cells_and_select_through_the_grid() => Run(() =>
	{
		var people = Two();
		var grid = Grid(people);
		var peer = UIElementAutomationPeer.CreatePeerForElement(grid)!;
		var children = peer.GetChildren()!;
		Assert.Equal(new[] { "DataGridColumnHeader", "DataGridColumnHeader", "DataGridRow", "DataGridRow" },
			children.ConvertAll(c => c.GetClassName()).ToArray());
		var row = children[3];
		Assert.Equal(AutomationControlType.DataItem, row.GetAutomationControlType());
		Assert.Equal(new[] { "Alan", "41" }, row.GetChildren()!.ConvertAll(c => c.GetName()).ToArray());
		Assert.Same(row, row.GetChildren()![0].GetParent());

		var select = (ISelectionItemProvider)row.GetPattern(PatternInterface.SelectionItem)!;
		select.Select();
		Assert.Same(people[1], grid.SelectedItem);
		Assert.True(select.IsSelected);
		var selection = (ISelectionProvider)peer.GetPattern(PatternInterface.Selection)!;
		Assert.Single(selection.GetSelection());
		Assert.Throws<InvalidOperationException>(
			((ISelectionItemProvider)children[2].GetPattern(PatternInterface.SelectionItem)!).AddToSelection);
		select.RemoveFromSelection();
		Assert.Null(grid.SelectedItem);
	});

	[Fact]
	public void Cell_values_are_read_only() => Run(() =>
	{
		var peer = UIElementAutomationPeer.CreatePeerForElement(Grid(Two()))!;
		var cell = peer.GetChildren()![2].GetChildren()![0];
		var value = (IValueProvider)cell.GetPattern(PatternInterface.Value)!;
		Assert.True(value.IsReadOnly);
		Assert.Throws<InvalidOperationException>(() => value.SetValue("x"));
		Assert.Throws<ArgumentNullException>(() => value.SetValue(null!));
	});
}
