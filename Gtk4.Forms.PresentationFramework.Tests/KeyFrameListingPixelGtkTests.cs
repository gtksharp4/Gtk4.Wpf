using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Gtk4.Forms.PresentationFramework.Tests.XamlBacked;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Each literal row and each nested block of the listing is visibly drawn.</summary>
public class KeyFrameListingPixelGtkTests : WpfGtkTestBase
{
	[Fact]
	public void Every_literal_row_and_nested_block_has_drawn_pixels() => Run(() =>
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
			int Ink(int x0, int y0, int x1, int y1)
			{
				x0 = Math.Max(0, x0);
				y0 = Math.Max(0, y0);
				x1 = Math.Min(texture.Width, x1);
				y1 = Math.Min(texture.Height, y1);
				int j = y0 * stride + (texture.Width - 1) * 4, diff = 0;
				for (int y = y0; y < y1; y++)
					for (int x = x0; x < x1; x++)
					{
						int i = y * stride + x * 4;
						if (Math.Abs(data[i] - data[j]) + Math.Abs(data[i + 1] - data[j + 1])
							+ Math.Abs(data[i + 2] - data[j + 2]) > 60) diff++;
					}
				return diff;
			}
			var label = (Gtk.Label)((Gtk.Overlay)widget).Child!;
			Assert.True(label.ComputeBounds(widget, out var origin));
			label.GetLayoutOffsets(out _, out int dy);
			var layout = label.Layout;
			var bytes = Encoding.UTF8.GetBytes(label.Text ?? "");
			int literal = 0;
			for (int i = 0; i < layout.LineCount; i++)
			{
				var line = layout.GetLineReadonly(i);
				var text = Encoding.UTF8.GetString(bytes, line.StartIndex, line.Length);
				if (text.Contains('￼') || string.IsNullOrWhiteSpace(text)) continue;
				var pos = layout.IndexToPos(line.StartIndex);
				int top = (int)(origin.Y + dy + pos.Y / 1024.0), bottom = top + (int)Math.Ceiling(pos.Height / 1024.0);
				literal++;
				Assert.True(Ink(0, top, w, bottom) >= 20, $"literal row {i} [{text.Trim()}] y {top}-{bottom} is blank; widget {w}x{h}, texture "
					+ $"{texture.Width}x{texture.Height}, bounds {node.Bounds.X},{node.Bounds.Y} {node.Bounds.Width}x{node.Bounds.Height}");
			}
			Assert.True(literal >= 6, $"{literal} literal rows");
			var nested = new[] { first, page.FirstSplineTextBlock, page.SecondLinearTextBlock,
				page.ThirdLinearTextBlock, page.SecondSplineTextBlock };
			foreach (var block in nested)
			{
				Assert.True(block.Widget.ComputeBounds(widget, out var r), "nested block bounds");
				Assert.True(r.Width > 0 && r.Height > 0, $"{block.Name} has an empty box");
				Assert.True(Ink((int)r.X, (int)r.Y, (int)(r.X + r.Width), (int)(r.Y + r.Height)) >= 20,
					$"nested block {block.Name} is blank");
			}
		}
		finally { win.Close(); }
	});
}
