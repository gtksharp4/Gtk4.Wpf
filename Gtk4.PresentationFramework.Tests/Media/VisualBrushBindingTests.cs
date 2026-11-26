using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>An ElementName binding on a VisualBrush nested in a DrawingBrush (the magnifier) resolves.</summary>
public class VisualBrushBindingTests : WpfGtkTestBase
{
	[Fact]
	public void A_VisualBrush_inside_a_DrawingGroup_binds_Visual_by_ElementName() => Run(() =>
	{
		var panel = new StackPanel { Name = "panel", Width = 50, Height = 50, Background = Brushes.Red };
		var brush = new VisualBrush();
		System.Windows.Markup.NonElementBinding.TrySetBinding(brush, VisualBrush.VisualProperty,
			new Binding { ElementName = "panel" });
		var group = new DrawingGroup();
		group.Children.Add(new GeometryDrawing { Brush = brush, Geometry = new RectangleGeometry(new Rect(0, 0, 1, 1)) });
		var ellipse = new Ellipse { Width = 40, Height = 40, Fill = new DrawingBrush(group) };
		var root = new Canvas();
		root.Children.Add(ellipse);
		var host = new StackPanel();
		host.Children.Add(panel);
		host.Children.Add(root);
		var win = new WpfWindow { Content = host, Width = 300, Height = 300 };
		NameScope.SetNameScope(win, new NameScope());
		win.RegisterName("panel", panel);
		try
		{
			win.Show();
			GtkPump.Until(() => ReferenceEquals(brush.Visual, panel));
			Assert.Same(panel, brush.Visual);
		}
		finally { win.Close(); }
	});
}
