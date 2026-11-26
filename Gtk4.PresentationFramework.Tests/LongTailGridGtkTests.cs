using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Real-GTK: track sizes are published from the GTK allocation before SizeChanged reaches handlers.</summary>
public class LongTailGridGtkTests : WpfGtkTestBase
{
	[Fact]
	public void RowDefinition_ActualHeight_is_set_when_a_shown_window_raises_SizeChanged() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var grid = new Grid();
		grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
		grid.RowDefinitions.Add(new RowDefinition());
		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
		grid.ColumnDefinitions.Add(new ColumnDefinition());
		var win = new WpfWindow { Width = 400, Height = 300, Content = grid };
		var seen = new List<(double Row0, double Row1, double Col1)>();
		string firstEvent = "";
		win.SizeChanged += (_, _) =>
		{
			if (seen.Count == 0)
				firstEvent = $"grid IsLoaded={grid.IsLoaded} Mapped={grid.Widget.IsMapped} Render={grid.RenderSize}; "
					+ "raised from: " + string.Join(" <- ", new System.Diagnostics.StackTrace().GetFrames()
						.Skip(1).Take(8).Select(f => f.GetMethod()?.DeclaringType?.Name + "." + f.GetMethod()?.Name));
			seen.Add((grid.RowDefinitions[0].ActualHeight, grid.RowDefinitions[1].ActualHeight,
				grid.ColumnDefinitions[1].ActualWidth));
		};
		try
		{
			win.Show();
			GtkState.Require(() => seen.Count > 0 && grid.ActualHeight > 0, "window SizeChanged", win, grid);
			GtkPump.For(100);

			var first = seen[0];
			if (first.Row0 != 50)
				Assert.Fail($"row 0 was {first.Row0} (expected 50) in the first window SizeChanged; {firstEvent}");
			Assert.True(first.Row1 > 0, $"row 1 was {first.Row1} inside the window's SizeChanged");
			Assert.True(first.Col1 > 0);
			Assert.Equal(grid.ActualHeight - 50, grid.RowDefinitions[1].ActualHeight, 1);
		}
		finally { win.Close(); }
	});
}
