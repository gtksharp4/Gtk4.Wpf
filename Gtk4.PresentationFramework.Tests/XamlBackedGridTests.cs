using System.Windows;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Gtk4.PresentationFramework.Tests.XamlBacked;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Real-GTK: a window whose InitializeComponent the markup compiler generated at build time.</summary>
public class XamlBackedGridTests : WpfGtkTestBase
{
	[Fact]
	public void The_generated_RowDefinitions_place_three_rows_at_increasing_Y() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new GridWindow();
		try
		{
			Assert.Equal(3, win.Root.RowDefinitions.Count);
			Assert.Equal(new GridLength(40), win.Root.RowDefinitions[0].Height);
			win.Show();
			GtkState.Require(() => win.Root.ActualHeight > 0, "grid layout", win, win.Root);
			GtkPump.For(100);

			var ys = new double[3];
			var rows = new[] { win.Row0, win.Row1, win.Row2 };
			for (int i = 0; i < 3; i++)
			{
				Assert.True(rows[i].Widget.ComputeBounds(win.Root.Widget, out var b));
				ys[i] = b.Y;
			}
			Assert.True(ys[0] < ys[1] && ys[1] < ys[2], $"row Y positions were {ys[0]}, {ys[1]}, {ys[2]}");
			Assert.Equal(40, win.Root.RowDefinitions[0].ActualHeight, 1);
			// Known gap: the Gtk.Grid backing treats a pixel row as Auto, so row 1 starts at the first button's natural
			// height (34), not at the 40px track. Flip this to >= 40 when pixel rows are honoured.
			Assert.True(ys[1] < 40, $"row 1 now starts at {ys[1]}: pixel rows are honoured, update this test");
		}
		finally { win.Close(); }
	});
}

public class XamlBackedDynamicResourceTests : WpfGtkTestBase
{
	[Fact]
	public void A_DynamicResource_on_a_media_property_sees_a_resource_of_an_enclosing_element() => Run(() =>
	{
		var win = new DynamicBrushWindow();
		try
		{
			var drawing = Assert.IsType<System.Windows.Media.GeometryDrawing>(win.Root.Resources["g"]);
			var brush = Assert.IsType<System.Windows.Media.SolidColorBrush>(drawing.Brush);
			Assert.Equal(System.Windows.Media.Colors.Red, brush.Color);
		}
		finally { win.Close(); }
	});
}
