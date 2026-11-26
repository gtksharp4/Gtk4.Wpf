using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>An animated source reaches the brush every frame; the first capture of a source is never blank.</summary>
public class VisualBrushFreshnessTests : WpfGtkTestBase
{
	static VisualBrush Corner(UIElement source) => new VisualBrush(source)
	{
		Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top,
	};

	[Fact]
	public void A_source_changing_every_frame_raises_Changed_for_most_changes() => Run(() =>
	{
		var panel = new StackPanel();
		var win = new WpfWindow { Content = panel, Width = 200, Height = 200 };
		win.Show();
		GtkPump.For(500);
		try
		{
			var source = new Border { Width = 40, Height = 40, Background = Brushes.Red };
			var brush = Corner(source);
			int changed = 0;
			brush.Changed += (_, _) => changed++;
			panel.Children.Add(new Rectangle { Width = 60, Height = 60, Fill = brush });
			GtkPump.For(1000);
			var colors = new[] { Brushes.Red, Brushes.Blue, Brushes.Lime, Brushes.Yellow };
			int ticks = 0, before = changed;
			uint id = GLib.Timeout.Add(16, new GLib.TimeoutHandler(() =>
			{
				source.Background = colors[++ticks % 4];
				return true;
			}));
			GtkPump.For(2000);
			GLib.Source.Remove(id);
			int raised = changed - before;
			Assert.True(ticks > 40, "ticks " + ticks);
			Assert.True(raised >= ticks * 0.8, raised + " Changed for " + ticks + " source changes");
		}
		finally { win.Close(); }
	});

	[Fact]
	public void The_first_paint_of_a_detached_source_shows_it_while_a_window_is_open() => Run(() =>
	{
		var win = new WpfWindow { Content = new StackPanel(), Width = 200, Height = 200 };
		win.Show();
		GtkPump.For(500);
		try
		{
			var source = new Border { Width = 40, Height = 40, Background = Brushes.Red };
			var rect = new Rectangle { Width = 60, Height = 60, Fill = Corner(source) };
			using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 60, 60);
			using (var cr = new Cairo.Context(surface))
				rect.PaintTo(cr, 60, 60);
			surface.Flush();
			var d = surface.Data;
			int i = 10 * surface.Stride + 10 * 4;
			Assert.True(d[i + 2] > 200 && d[i] < 50 && d[i + 3] > 200, $"({d[i + 2]}, {d[i + 1]}, {d[i]}, {d[i + 3]})");
		}
		finally { win.Close(); }
	});
}
