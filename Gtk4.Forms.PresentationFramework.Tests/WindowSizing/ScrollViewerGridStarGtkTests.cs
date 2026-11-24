using System.Windows;
using System.Windows.Controls;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests.WindowSizing;

/// <summary>
/// WPF Grid splits the space by star weight whatever a child's desired size is; a ScrollViewer around tall
/// content must not take a whole star track (R68E-1).
/// </summary>
public class ScrollViewerGridStarGtkTests : WpfGtkTestBase
{
	static RowDefinition Row(double stars) => new RowDefinition { Height = new GridLength(stars, GridUnitType.Star) };

	static (ScrollViewer sv, TextBlock tb, WpfWindow win) Show(double firstStars, double secondStars)
	{
		var tall = new Border { Width = 200, Height = 2000 };
		var sv = new ScrollViewer { Content = tall };
		var tb = new TextBlock { Text = "detail" };
		var grid = new Grid();
		grid.RowDefinitions.Add(Row(firstStars));
		grid.RowDefinitions.Add(Row(secondStars));
		Grid.SetRow(tb, 1);
		grid.Children.Add(sv);
		grid.Children.Add(tb);
		var win = new WpfWindow { Width = 600, Height = 400, Content = grid };
		win.Show();
		GtkPump.For(400);
		return (sv, tb, win);
	}

	[Fact]
	public void Two_equal_star_rows_split_the_space_equally_around_a_ScrollViewer() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (sv, tb, win) = Show(1, 1);
		try
		{
			int a = sv.Widget.Height, b = tb.Widget.Height;
			Assert.True(a > 0 && b > 0, $"{a}/{b}");
			Assert.InRange(a, (a + b) / 2.0 - 40, (a + b) / 2.0 + 40);
			var adj = ((Gtk.ScrolledWindow)sv.Widget).Vadjustment;
			Assert.True(adj.Upper > adj.PageSize);
		}
		finally { win.Close(); }
	});
}
