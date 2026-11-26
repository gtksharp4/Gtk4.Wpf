using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Real-GTK: the Grid built the way the markup compiler emits its row definitions.</summary>
public class GridDefinitionGtkTests : WpfGtkTestBase
{
	static GridLength Length(string text)
		=> (GridLength)XamlValueConverter.ConvertByType(typeof(GridLength), text)!;

	[Fact]
	public void A_three_row_grid_lays_the_children_out_in_different_rows() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var grid = new Grid();
		foreach (var h in new[] { "Auto", "*", "2*" })
			grid.RowDefinitions.Add(new RowDefinition { Height = Length(h) });
		var rows = new Button[3];
		for (int i = 0; i < 3; i++)
		{
			rows[i] = new Button { Content = "row " + i, Height = 20 };
			Grid.SetRow(rows[i], i);
			grid.Children.Add(rows[i]);
		}
		var win = new WpfWindow { Width = 400, Height = 300, Content = grid };
		try
		{
			win.Show();
			GtkState.Require(() => grid.ActualHeight > 0, "grid layout", win, grid);
			GtkPump.For(100);

			var ys = new double[3];
			for (int i = 0; i < 3; i++)
			{
				Assert.True(rows[i].Widget.ComputeBounds(grid.Widget, out var b));
				ys[i] = b.Y;
			}
			Assert.True(ys[0] < ys[1] && ys[1] < ys[2], $"row Y positions were {ys[0]}, {ys[1]}, {ys[2]}");
			Assert.Equal(grid.RowDefinitions[2].ActualHeight, 2 * grid.RowDefinitions[1].ActualHeight, 1);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Length_text_converts_to_the_grid_units()
	{
		Assert.Equal(GridUnitType.Auto, Length("Auto").GridUnitType);
		Assert.Equal(new GridLength(2, GridUnitType.Star), Length("2*"));
		Assert.Equal(new GridLength(1, GridUnitType.Star), Length("*"));
		Assert.Equal(new GridLength(100), Length("100"));
	}
}
