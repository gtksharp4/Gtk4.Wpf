using System;
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
		if (node == null) return new Shot(new Cairo.ImageSurface(Cairo.Format.Argb32, Math.Max(1, w), Math.Max(1, h)));
		var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, w, h);
		using var cr = new Cairo.Context(surface);
		node.Draw(cr);
		return new Shot(surface);
	});

	// The element as its parent draws it (parent-applied OpacityMask included), cropped to the element's bounds.
	Shot CaptureWithin(UIElement container, UIElement element) => App.Invoke(() =>
	{
		using var full = Capture(container).Surface;
		element.Widget.ComputeBounds(container.Widget, out var b);
		int w = (int)Math.Ceiling(b.Width), h = (int)Math.Ceiling(b.Height);
		var crop = new Cairo.ImageSurface(Cairo.Format.Argb32, w, h);
		using var cr = new Cairo.Context(crop);
		cr.SetSourceSurface(full, -(int)Math.Round(b.X), -(int)Math.Round(b.Y));
		cr.Paint();
		return new Shot(crop);
	});

	// A parent snapshot taken right after another can come back empty; retry with a pause until it has content.
	Shot CapturePainted(UIElement container, UIElement element)
	{
		var shot = CaptureWithin(container, element);
		for (int i = 0; i < 30 && shot.Count(p => p.A > 40) <= 1000; i++)
		{
			System.Threading.Thread.Sleep(300);
			shot = CaptureWithin(container, element);
		}
		return shot;
	}

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
		var shot = CapturePainted((StackPanel)page.Content, lower);
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
		// The TextBox sits near the bottom of the source, so mirrored it is near the top of the reflection, and the
		// lorem text (starting at y 40 of the source) is mirrored to end at y 260, below the visible half. An
		// unflipped reflection would put full-width text lines at the top instead. The text height depends on the
		// font, so only the rows above the mirrored text are checked for text-free ink.
		double textHeight = App.Invoke(() => ((TextBlock)upper.Children[0]).ActualHeight);
		int clearRows = (int)Math.Min(150, 260 - textHeight);
		int wide = 0;
		for (int y = 0; y < clearRows; y++)
			for (int x = 220; x < shot.Surface.Width; x++)
			{
				var p = shot.At(x, y);
				if (p.A >= 60 && p.R * 255 / p.A < 90 && p.G * 255 / p.A < 90 && p.B * 255 / p.A < 90) wide++;
			}
		Assert.True(wide < 40, $"the reflection is not flipped: {wide} dark pixels on the right of the top "
			+ $"{clearRows} rows (text height {textHeight})");
		int box = 0;
		for (int y = 10; y < 70; y++)
			for (int x = 75; x < 200; x++)
			{
				var p = shot.At(x, y);
				var bg = shot.At(60, y);
				if (Math.Abs(p.R - bg.R) + Math.Abs(p.G - bg.G) + Math.Abs(p.B - bg.B) > 40) box++;
			}
		Assert.True(box > 15, $"the mirrored TextBox is missing near the top: {box}");
		Assert.NotNull(upper);
	}

	[WpfUiFact]
	public void The_95_by_35_tile_repeats_at_its_period()
	{
		var page = SelectTab(0);
		var rect = App.Invoke(() => (Rectangle)((StackPanel)((StackPanel)page.Content).Children[2]).Children[0]);
		App.Invoke(() => Assert.True(rect.Fill is System.Windows.Media.VisualBrush { Viewport.Width: 95, Viewport.Height: 35 }));
		var shot = Capture(rect);
		int ink = shot.Count(p => p.A > 60 && p.R < 140 && p.G < 140 && p.B < 140);
		Assert.True(ink > 100, $"the 95x35 tile has no text: {ink}");
		int mismatched = 0, compared = 0;
		for (int y = 5; y < 70; y++)
			for (int x = 5; x < 50; x++)
			{
				compared++;
				if (shot.At(x, y) != shot.At(x + 95, y + 35)) mismatched++;
			}
		Assert.True(mismatched < compared / 50, $"the tile does not repeat every 95x35 pixels: {mismatched}/{compared}");
	}

	[WpfUiFact]
	public void The_page_background_paints_the_faint_text_tiles()
	{
		var page = SelectTab(0);
		var root = App.Invoke(() => (StackPanel)page.Content);
		var shot = Capture(root);
		int faint = shot.Count(p => p.A is > 4 and < 45);
		Assert.True(faint > 300, $"the 0.1-opacity text tiles are missing: {faint}");
	}

	[WpfUiFact]
	public void Painting_the_tabs_never_adds_a_window_or_steals_activation()
	{
		App.Invoke(() => Viewer.Activate());
		App.WaitUntil(() => App.Invoke(() => Viewer.IsActive), description: "app window active");
		var gtkWindow = App.Invoke(() => (Gtk.Window)Viewer.Widget);
		bool OthersActive() => App.Invoke(() => Gtk.Window.ListToplevels().Any(w => w != gtkWindow && w.IsVisible && w.IsActive));
		string Describe() => App.Invoke(() => string.Join("; ", Gtk.Window.ListToplevels()
			.Select(w => $"{w.GetType().Name} '{w.Title}' active={w.IsActive} visible={w.IsVisible} app={w == gtkWindow}")));
		int before = App.Invoke(() => Gtk.Window.ListToplevels().Count(w => w.IsVisible));
		for (int tab = 0; tab < 3; tab++)
		{
			SelectTab(tab);
			Assert.False(OthersActive(), $"tab {tab}: another toplevel took activation: " + Describe());
		}
		App.Invoke(() => Assert.Equal(before, Gtk.Window.ListToplevels().Count(w => w.IsVisible)));
	}
}
