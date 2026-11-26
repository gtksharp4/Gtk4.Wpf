using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>OpacityMask is applied in the element's own space, so it turns with a RenderTransform.</summary>
public class BackdropRotatedMaskTests : WpfGtkTestBase
{
	static byte AlphaAt(Cairo.ImageSurface s, int x, int y)
	{
		s.Flush();
		return s.Data[y * s.Stride + x * 4 + 3];
	}

	[Fact]
	public void A_rotated_masked_Border_fades_along_the_rotated_axis() => Run(() =>
	{
		var mask = new LinearGradientBrush(Colors.Black, Colors.Transparent, 0);
		var panel = new StackPanel { Width = 100, Height = 100 };
		panel.Children.Add(new Border
		{
			Width = 100, Height = 100, Background = Brushes.Red, OpacityMask = mask,
			RenderTransformOrigin = new Point(0.5, 0.5), RenderTransform = new RotateTransform(90),
		});
		var win = new WpfWindow { Content = panel, Width = 200, Height = 200 };
		try
		{
			win.Show();
			GtkPump.For(400);
			using var paintable = new Gtk.WidgetPaintable(panel.Widget);
			using var snapshot = new Gtk.Snapshot();
			paintable.Snapshot(snapshot, 100, 100);
			using var node = snapshot.FreeToNode();
			using var s = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
			using (var cr = new Cairo.Context(s)) node.Draw(cr);
			byte top = AlphaAt(s, 50, 5), bottom = AlphaAt(s, 50, 94), left = AlphaAt(s, 5, 50), right = AlphaAt(s, 94, 50);
			Assert.True(top > 200 && bottom < 60, $"top {top} bottom {bottom}");
			Assert.InRange(left, 90, 170);
			Assert.InRange(right, 90, 170);
		}
		finally { win.Close(); }
	});
}
