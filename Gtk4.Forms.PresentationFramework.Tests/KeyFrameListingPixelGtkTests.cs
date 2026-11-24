using System.Windows;
using System.Windows.Controls;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Gtk4.Forms.PresentationFramework.Tests.XamlBacked;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>The listing's rows, including the ones around the nested blocks, are visibly drawn.</summary>
public class KeyFrameListingPixelGtkTests : WpfGtkTestBase
{
	[Fact]
	public void Every_listing_row_has_drawn_pixels() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var page = new KeyFrameListingPage();
		var win = new WpfWindow { Width = 900, Height = 800, Content = page };
		win.Show();
		try
		{
			var first = page.FirstLinearTextBlock;
			GtkState.Require(() => first.IsWidgetCreated && first.Widget.IsMapped && first.Widget.Width > 0,
				"nested block mapped", first);
			GtkPump.For(200);
			var outer = (TextBlock)LogicalTreeHelper.GetParent(LogicalTreeHelper.GetParent(first)!)!;
			var widget = outer.Widget;
			int w = widget.Width, h = widget.Height;
			var paintable = new Gtk.WidgetPaintable(widget);
			var snapshot = new Gtk.Snapshot();
			paintable.Snapshot(snapshot, w, h);
			var node = snapshot.FreeToNode();
			var texture = widget.Native!.Renderer.RenderTexture(node, node.Bounds);
			var data = texture.Download();
			int stride = data.Length / texture.Height;
			int rows = 0, inked = 0;
			// 14 px bands: a band is inked when enough pixels differ from the band's corner (the background).
			for (int y0 = 0; y0 + 14 <= texture.Height; y0 += 14)
			{
				rows++;
				int diff = 0;
				for (int y = y0; y < y0 + 14; y++)
					for (int x = 0; x < texture.Width; x++)
					{
						int i = y * stride + x * 4;
						int j = y0 * stride + (texture.Width - 1) * 4;
						if (System.Math.Abs(data[i] - data[j]) + System.Math.Abs(data[i + 1] - data[j + 1])
							+ System.Math.Abs(data[i + 2] - data[j + 2]) > 60) diff++;
					}
				if (diff >= 20) inked++;
			}
			Assert.True(rows >= 8, $"{rows} bands");
			Assert.True(inked >= 8, $"{inked} of {rows} bands have text pixels");
		}
		finally { win.Close(); }
	});
}
