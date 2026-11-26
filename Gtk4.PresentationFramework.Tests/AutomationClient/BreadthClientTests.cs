using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Automation;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.AutomationClient;

/// <summary>The Phase 4 patterns read through the client API, as a WPF UI test would.</summary>
public class BreadthClientTests : UiaTestBase
{
	static (Window Window, DataGrid Grid, TextBox Box) Show()
	{
		new Application();
		var grid = Id(new DataGrid { AutoGenerateColumns = false }, "Grid");
		grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name") });
		grid.Columns.Add(new DataGridTextColumn { Header = "Age", Binding = new System.Windows.Data.Binding("Age") });
		grid.ItemsSource = new ObservableCollection<DataGridPeerTests.Person>
		{
			new DataGridPeerTests.Person { Name = "Ada", Age = 36 },
			new DataGridPeerTests.Person { Name = "Alan", Age = 41 },
		};
		var box = Id(new TextBox { Text = "alpha beta\ngamma" }, "Box");
		var panel = new StackPanel();
		panel.Children.Add(grid);
		panel.Children.Add(box);
		var window = new Window { Content = panel, Width = 500, Height = 500 };
		window.Show();
		GtkPump.Until(() => window.IsVisible && box.Widget.Width > 0);
		return (window, grid, box);
	}

	[Fact]
	public void Grid_and_table_patterns_read_cells_and_headers() => Run(() =>
	{
		var (window, _, _) = Show();
		var element = Find("Grid");
		var gridPattern = (GridPattern)element.GetCurrentPattern(GridPattern.Pattern);
		Assert.Equal(2, gridPattern.Current.RowCount);
		Assert.Equal(2, gridPattern.Current.ColumnCount);
		var cell = gridPattern.GetItem(0, 1);
		Assert.Equal("36", cell.Current.Name);
		var item = (GridItemPattern)cell.GetCurrentPattern(GridItemPattern.Pattern);
		Assert.Equal(1, item.Current.Column);
		Assert.Equal(element, item.Current.ContainingGrid);
		var table = (TablePattern)element.GetCurrentPattern(TablePattern.Pattern);
		Assert.Equal(RowOrColumnMajor.RowMajor, table.Current.RowOrColumnMajor);
		var tableItem = (TableItemPattern)cell.GetCurrentPattern(TableItemPattern.Pattern);
		Assert.Equal("Age", tableItem.Current.GetColumnHeaderItems()[0].Current.Name);
		window.Close();
	});

	[Fact]
	public void Text_pattern_reads_ranges_and_selects() => Run(() =>
	{
		var (window, _, box) = Show();
		var element = Find("Box");
		var text = (TextPattern)element.GetCurrentPattern(TextPattern.Pattern);
		Assert.Equal(SupportedTextSelection.Single, text.SupportedTextSelection);
		var document = text.DocumentRange;
		Assert.Equal("alpha beta\ngamma", document.GetText(-1));
		Assert.Equal("alph", document.GetText(4));

		var word = document.Clone();
		word.ExpandToEnclosingUnit(TextUnit.Word);
		Assert.Equal("alpha ", word.GetText(-1));
		Assert.Equal(1, word.Move(TextUnit.Word, 1));
		Assert.Equal("beta\n", word.GetText(-1));
		var line = word.Clone();
		line.ExpandToEnclosingUnit(TextUnit.Line);
		Assert.Equal("alpha beta\n", line.GetText(-1));
		Assert.Equal(1, line.Move(TextUnit.Line, 5));
		Assert.Equal("gamma", line.GetText(-1));

		var found = document.FindText("BETA", false, true)!;
		Assert.Equal("beta", found.GetText(-1));
		Assert.Null(document.FindText("zzz", false, false));
		found.Select();
		Assert.Equal("beta", box.SelectedText);
		Assert.Equal("beta", text.GetSelection()[0].GetText(-1));
		Assert.Equal(0, found.CompareEndpoints(TextPatternRangeEndpoint.Start, text.GetSelection()[0],
			TextPatternRangeEndpoint.Start));
		Assert.True(document.CompareEndpoints(
			TextPatternRangeEndpoint.Start, found, TextPatternRangeEndpoint.Start) < 0);
		Assert.Throws<System.InvalidOperationException>(found.AddToSelection);
		window.Close();
	});
}
