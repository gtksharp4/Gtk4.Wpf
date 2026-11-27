using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.WindowSizing;

/// <summary>A ScrollViewer nested in a panel under a SizeToContent window: the whole content's WPF Measure sizes it.</summary>
public class WindowSizeToContentNestedScrollViewerGtkTests : WpfGtkTestBase
{
	static void Show(WpfWindow win, ScrollViewer sv)
	{
		win.Show();
		GtkPump.Until(() => sv.Widget.IsMapped && sv.Widget.Height > 0);
		GtkPump.For(300);
	}

	[Fact]
	public void ScrollViewer_in_a_Grid_with_an_Auto_and_a_star_row_gets_its_content_height() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var sv = new ScrollViewer { Content = new Border { Width = 300, Height = 400 } };
		var grid = new Grid();
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
		var header = new Border { Height = 30 };
		grid.Children.Add(header);
		grid.Children.Add(sv);
		Grid.SetRow(sv, 1);
		var win = new WpfWindow { SizeToContent = SizeToContent.WidthAndHeight, Content = grid };
		try
		{
			Show(win, sv);
			Assert.True(sv.Widget.Height >= 399, $"viewer height {sv.Widget.Height}");
			Assert.True(sv.Widget.Width >= 299, $"viewer width {sv.Widget.Width}");
		}
		finally { win.Close(); }
	});

	[Fact]
	public void ScrollViewer_in_a_StackPanel_gets_its_content_height() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var sv = new ScrollViewer { Content = new Border { Width = 300, Height = 400 } };
		var panel = new StackPanel();
		panel.Children.Add(new Border { Height = 30 });
		panel.Children.Add(sv);
		var win = new WpfWindow { SizeToContent = SizeToContent.WidthAndHeight, Content = panel };
		try
		{
			Show(win, sv);
			Assert.True(sv.Widget.Height >= 399, $"viewer height {sv.Widget.Height}");
			Assert.True(sv.Widget.Width >= 299, $"viewer width {sv.Widget.Width}");
		}
		finally { win.Close(); }
	});
}
