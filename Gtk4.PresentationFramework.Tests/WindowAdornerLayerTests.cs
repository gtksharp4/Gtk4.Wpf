using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Adorner that relies on the default widget so it can be hosted in a real overlay.</summary>
sealed class WidgetAdorner : Adorner
{
	public WidgetAdorner(UIElement adorned) : base(adorned) { }
}

/// <summary>The window implicit adorner layer: lookup and overlay hosting.</summary>
public class WindowAdornerLayerTests : WpfGtkTestBase
{
	internal static (WpfWindow win, StackPanel panel, Button target, Border spacer) Build()
	{
		var spacer = new Border { Height = 40 };
		var target = new Button { Content = "target", Width = 80, Height = 30 };
		var panel = new StackPanel();
		panel.Children.Add(spacer);
		panel.Children.Add(target);
		return (new WpfWindow { Width = 400, Height = 300, Content = panel }, panel, target, spacer);
	}

	internal static void ShowAndLayout(WpfWindow win, FrameworkElement probe)
	{
		win.Show();
		GtkState.Require(() => probe.ActualHeight > 0, "layout", probe);
		GtkPump.For(100);
	}

	internal static Gtk.Overlay OverlayOf(WpfWindow win) =>
		WpfWindow.ContentOverlayOf((Gtk.Window)win.Widget) ?? win.ImplicitAdornerLayer.GtkOverlay!;

	// ── lookup ───────────────────────────────────────────────────────────────

	[Fact]
	public void Descendant_finds_window_layer_before_show() => Run(() =>
	{
		var (win, _, target, _) = Build();
		var layer = AdornerLayer.GetAdornerLayer(target);
		Assert.NotNull(layer);
		Assert.Same(win.ImplicitAdornerLayer, layer);
	});

	[Fact]
	public void Descendant_finds_same_window_layer_after_show() => Run(() =>
	{
		var (win, _, target, _) = Build();
		var before = AdornerLayer.GetAdornerLayer(target);
		try
		{
			ShowAndLayout(win, target);
			Assert.Same(before, AdornerLayer.GetAdornerLayer(target));
		}
		finally { win.Close(); }
	});

	[Fact]
	public void GetAdornerLayer_of_window_is_null() => Run(() =>
	{
		var (win, _, _, _) = Build();
		Assert.Null(AdornerLayer.GetAdornerLayer(win));
	});

	[Fact]
	public void Direct_content_finds_window_layer() => Run(() =>
	{
		var (win, panel, _, _) = Build();
		Assert.Same(win.ImplicitAdornerLayer, AdornerLayer.GetAdornerLayer(panel));
	});

	[Fact]
	public void Explicit_decorator_inside_window_wins() => Run(() =>
	{
		var target = new Button { Content = "t" };
		var dec = new AdornerDecorator { Child = target };
		var win = new WpfWindow { Content = dec };
		Assert.Same(dec.AdornerLayer, AdornerLayer.GetAdornerLayer(target));
		// The decorator itself sits directly under the window layer.
		Assert.Same(win.ImplicitAdornerLayer, AdornerLayer.GetAdornerLayer(dec));
	});

	[Fact]
	public void Element_outside_any_window_has_no_layer() => Run(() =>
	{
		var panel = new StackPanel();
		var child = new Button();
		panel.Children.Add(child);
		Assert.Null(AdornerLayer.GetAdornerLayer(child));
	});

	[Fact]
	public void Detached_content_loses_window_layer() => Run(() =>
	{
		var (win, _, target, _) = Build();
		win.Content = null;
		Assert.Null(AdornerLayer.GetAdornerLayer(target));
	});

	[Fact]
	public void Each_window_has_its_own_layer() => Run(() =>
	{
		Assert.NotSame(new WpfWindow().ImplicitAdornerLayer, new WpfWindow().ImplicitAdornerLayer);
	});

	// ── overlay hosting ──────────────────────────────────────────────────────

	[Fact]
	public void Gtk_window_child_is_overlay_with_content_widget() => Run(() =>
	{
		var (win, panel, _, _) = Build();
		var overlay = Assert.IsType<Gtk.Overlay>(((Gtk.Window)win.Widget).Child);
		Assert.Same(panel.Widget, overlay.Child);
		Assert.Same(overlay, win.ImplicitAdornerLayer.GtkOverlay);
	});

	[Fact]
	public void Content_set_after_widget_exists_is_overlay_child() => Run(() =>
	{
		var win = new WpfWindow();
		var overlay = OverlayOf(win);
		Assert.Null(overlay.Child);
		var content = new Button { Content = "c" };
		win.Content = content;
		Assert.Same(content.Widget, overlay.Child);
	});

	[Fact]
	public void Content_swap_keeps_layer_and_replaces_overlay_child() => Run(() =>
	{
		var (win, _, _, _) = Build();
		var layer = win.ImplicitAdornerLayer;
		var overlay = OverlayOf(win);
		var next = new Button { Content = "next" };
		win.Content = next;
		Assert.Same(next.Widget, overlay.Child);
		Assert.Same(layer, AdornerLayer.GetAdornerLayer(next));
		Assert.Same(overlay, layer.GtkOverlay);
	});

	[Fact]
	public void Content_set_to_null_clears_overlay_child() => Run(() =>
	{
		var (win, _, _, _) = Build();
		var overlay = OverlayOf(win);
		win.Content = null;
		Assert.Null(overlay.Child);
	});

	[Fact]
	public void String_content_leaves_overlay_child_empty() => Run(() =>
	{
		var win = new WpfWindow { Content = "just text" };
		Assert.Null(OverlayOf(win).Child);
	});

	[Fact]
	public void Show_keeps_content_as_the_overlay_child() => Run(() =>
	{
		var (win, panel, target, _) = Build();
		try
		{
			ShowAndLayout(win, target);
			var overlay = OverlayOf(win);
			Assert.Same(panel.Widget, overlay.Child);
			Assert.Same(overlay, panel.Widget.Parent);
		}
		finally { win.Close(); }
	});
}
