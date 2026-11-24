using System;
using System.Windows;
using System.Windows.Controls;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests.WindowSizing;

/// <summary>
/// WPF Grid splits the space by star weight whatever a child's desired size is; a ScrollViewer around big
/// content must not take a whole star track, however it is wrapped (R68E-1).
/// </summary>
public class ScrollViewerGridStarGtkTests : WpfGtkTestBase
{
	static ScrollViewer Viewer(bool wide) => new ScrollViewer
	{
		Content = new Border { Width = wide ? 3000 : 200, Height = wide ? 50 : 2000 },
		HorizontalScrollBarVisibility = wide ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled,
	};

	static UIElement Wrap(string how, ScrollViewer sv)
	{
		switch (how)
		{
			case "border": return new Border { Child = sv };
			case "stack":
				var sp = new StackPanel();
				sp.Children.Add(sv);
				return new Border { Child = sp };
			default: return sv;
		}
	}

	static double Split(string how, bool columns)
	{
		var sv = Viewer(columns);
		var top = Wrap(how, sv);
		var tb = new TextBlock { Text = "detail" };
		var grid = new Grid();
		for (int i = 0; i < 2; i++)
		{
			if (columns)
				grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			else
				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
		}
		if (columns) Grid.SetColumn(tb, 1); else Grid.SetRow(tb, 1);
		grid.Children.Add(top);
		grid.Children.Add(tb);
		var win = new WpfWindow { Width = 600, Height = 400, Content = grid };
		win.Show();
		GtkPump.For(400);
		try
		{
			double a = columns ? top.Widget.Width : top.Widget.Height;
			double b = columns ? tb.Widget.Width : tb.Widget.Height;
			Assert.True(a > 0 && b > 0, $"{a}/{b}");
			return a / (a + b);
		}
		finally { win.Close(); }
	}

	[Theory]
	[InlineData("direct", false)]
	[InlineData("border", false)]
	[InlineData("stack", false)]
	[InlineData("direct", true)]
	[InlineData("border", true)]
	public void Two_equal_star_tracks_split_equally_around_a_ScrollViewer(string how, bool columns) => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		Assert.InRange(Split(how, columns), 0.4, 0.6);
	});

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void SizeToContent_still_sizes_to_the_content(bool wrapped) => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var sv = new ScrollViewer { Content = new Border { Width = 300, Height = 200 } };
		var win = new WpfWindow
		{
			SizeToContent = SizeToContent.WidthAndHeight,
			Content = wrapped ? new Border { Child = sv } : sv,
		};
		win.Show();
		GtkPump.For(300);
		try
		{
			Assert.InRange(sv.Widget.Width, 295, 420);
			Assert.InRange(sv.Widget.Height, 195, 260);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Propagation_follows_SizeToContent_changes() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var sv = new ScrollViewer { Content = new Border { Width = 300, Height = 200 } };
		var win = new WpfWindow { Width = 400, Height = 300, Content = sv };
		win.Show();
		GtkPump.For(200);
		try
		{
			var sw = (Gtk.ScrolledWindow)sv.Widget;
			Assert.False(sw.PropagateNaturalWidth || sw.PropagateNaturalHeight);
			win.SizeToContent = SizeToContent.Height;
			Assert.False(sw.PropagateNaturalWidth);
			Assert.True(sw.PropagateNaturalHeight);
		}
		finally { win.Close(); }
	});
}
