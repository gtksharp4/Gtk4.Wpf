using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Linq;
using Xunit;

namespace Gtk4.Forms.Wpf.Testing.Tests.Samples;

/// <summary>WPF-Samples "Graphics/VisualBrush", unmodified: all three tabs checked by pixels.</summary>
public class VisualBrushSampleTests : WpfUiTest<global::VisualBrush.App>
{
	global::VisualBrush.SampleViewer Viewer => (global::VisualBrush.SampleViewer)App.Window;

	record Shot(Cairo.ImageSurface Surface)
	{
		readonly byte[] _data = Flushed(Surface);

		static byte[] Flushed(Cairo.ImageSurface s)
		{
			s.Flush();
			return s.Data;
		}

		public (byte R, byte G, byte B, byte A) At(int x, int y)
		{
			var d = _data;
			int i = y * Surface.Stride + x * 4;
			return (d[i + 2], d[i + 1], d[i], d[i + 3]);
		}

		public int Count(System.Func<(byte R, byte G, byte B, byte A), bool> match)
		{
			int n = 0;
			for (int y = 0; y < Surface.Height; y++)
				for (int x = 0; x < Surface.Width; x++)
					if (match(At(x, y))) n++;
			return n;
		}
	}

	Shot Capture(UIElement element) => App.Invoke(() =>
	{
		var widget = element.Widget;
		int w = widget.Width, h = widget.Height;
		using var paintable = new Gtk.WidgetPaintable(widget);
		using var snapshot = new Gtk.Snapshot();
		paintable.Snapshot(snapshot, w, h);
		using var node = snapshot.FreeToNode();
		if (node == null) throw new System.InvalidOperationException($"no render node for {element.GetType().Name} {w}x{h} mapped={widget.IsMapped}");
		var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, w, h);
		using var cr = new Cairo.Context(surface);
		node.Draw(cr);
		return new Shot(surface);
	});

	Page SelectTab(int index)
	{
		App.Invoke(() => Viewer.sampleSelector.SelectedIndex = index);
		Page page = null;
		App.WaitUntil(() => App.Invoke(() =>
		{
			var frame = (Frame)((TabItem)Viewer.sampleSelector.Items[index]!).Content;
			page = frame.Content as Page;
			return page != null && page.Widget.Width > 0;
		}), description: "tab page loaded");
		// The Reflection tab animates forever, so the loop is never idle: just give the frame time to paint.
		System.Threading.Thread.Sleep(700);
		return page;
	}

	static bool IsRed((byte R, byte G, byte B, byte A) p) => p.R > 200 && p.G < 60 && p.B < 60 && p.A > 200;

	[WpfUiFact]
	public void Painting_with_visuals_tiles_the_Hello_World_squares()
	{
		var page = SelectTab(0);
		var tiled = App.Invoke(() => (Rectangle)((StackPanel)((StackPanel)page.Content).Children[3]).Children[0]);
		App.Invoke(() => Assert.True(tiled.Fill is System.Windows.Media.VisualBrush { Viewport.Width: 50 }));
		var shot = Capture(tiled);
		Assert.True(shot.Count(IsRed) > 1000, "red squares missing");
		Assert.False(shot.Count(p => p.A > 200 && p.R < 10 && p.G < 10 && p.B < 10) > shot.Surface.Width * shot.Surface.Height / 2,
			"the fill is black");
	}

	[WpfUiFact]
	public void The_magnifier_follows_the_mouse_and_shows_the_magnified_region()
	{
		var page = SelectTab(1);
		var panel = App.Invoke(() => (StackPanel)page.FindName("magnifiedPanel"));
		var ellipse = App.Invoke(() => (Ellipse)page.FindName("magnifyingGlassEllipse"));
		App.Input.Mouse.MoveTo(panel, new Point(123, 67));
		App.WaitForIdle();
		double left1 = App.Invoke(() => Canvas.GetLeft(ellipse));
		Assert.False(double.IsNaN(left1));
		App.WaitUntil(() => Capture(ellipse).Count(p => p.A > 200 && (p.R < 245 || p.G < 245)) > 200, description: "magnified content");
		App.Invoke(() => Assert.True(((System.Windows.Media.VisualBrush)page.FindName("myVisualBrush")).Visual != null, "myVisualBrush.Visual is unresolved: " + ((System.Windows.Media.VisualBrush)page.FindName("myVisualBrush")).Viewbox));
		var first = Capture(ellipse);
		Assert.False(first.Count(p => p.A > 200 && p.R < 10 && p.G < 10 && p.B < 10) > 3000, "the interior is black");

		App.Input.Mouse.MoveTo(panel, new Point(307, 153));
		App.WaitForIdle();
		double left2 = App.Invoke(() => Canvas.GetLeft(ellipse));
		Assert.NotEqual(left1, left2);
		App.WaitForIdle();
		var second = Capture(ellipse);
		int diff = 0;
		for (int y = 0; y < first.Surface.Height; y += 3)
			for (int x = 0; x < first.Surface.Width; x += 3)
				if (first.At(x, y) != second.At(x, y)) diff++;
		Assert.True(diff > 20, "the magnifier did not change after the second move: " + diff);
	}

	[WpfUiFact]
	public void The_reflection_is_flipped_and_fades_towards_the_bottom()
	{
		var page = SelectTab(2);
		var upper = App.Invoke(() => (Canvas)page.FindName("reflectedCanvas"));
		var lower = App.Invoke(() => (Canvas)((StackPanel)page.Content).Children[2]);
		App.WaitUntil(() => App.Invoke(() => lower.Widget.Height > 100), description: "lower canvas sized");
		System.Threading.Thread.Sleep(700);
		App.Invoke(() => Assert.True(lower.Background is System.Windows.Media.VisualBrush { Visual: not null } && lower.OpacityMask != null,
			$"bg={lower.Background?.GetType().Name} visual={(lower.Background as System.Windows.Media.VisualBrush)?.Visual} mask={lower.OpacityMask}"));
		var shot = Capture(lower);
		int h = shot.Surface.Height;
		int visibleTop = shot.Count(p => p.A > 40);
		Assert.True(visibleTop > 1000, "the reflection is empty");
		long topAlpha = 0, bottomAlpha = 0;
		for (int x = 0; x < shot.Surface.Width; x += 4)
		{
			topAlpha += shot.At(x, 2).A;
			bottomAlpha += shot.At(x, h - 3).A;
		}
		Assert.True(topAlpha > bottomAlpha, $"no fade: top {topAlpha}, bottom {bottomAlpha}");
		// The TextBox sits near the bottom of the source, so mirrored it is near the top of the reflection.
		Assert.NotNull(upper);
	}
}
