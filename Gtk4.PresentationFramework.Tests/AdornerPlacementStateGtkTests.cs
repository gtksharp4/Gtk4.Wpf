using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;
using static Gtk4.PresentationFramework.Tests.WindowAdornerLayerTests;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Placed adorners keep their visibility, hit-test and capture state through repositioning.</summary>
public class AdornerPlacementStateGtkTests : WpfGtkTestBase
{
	[Fact]
	public void Hidden_then_Visible_shows_the_placed_adorner_again() => Run(() =>
	{
		var (win, _, target, _) = Build();
		try
		{
			ShowAndLayout(win, target);
			var adorner = new WidgetAdorner(target);
			AdornerLayer.GetAdornerLayer(target)!.Add(adorner);
			Assert.True(GtkPump.Until(
				() => AdornerPlacementBoundsGtkTests.Covers(OverlayOf(win), target, adorner)),
				"adorner never placed");
			adorner.Visibility = Visibility.Hidden;
			GtkPump.Until(() => adorner.Widget.Opacity == 0.0);
			Assert.True(adorner.Widget.Visible);
			Assert.Equal(0.0, adorner.Widget.Opacity);
			adorner.Visibility = Visibility.Visible;
			GtkPump.Until(() => adorner.Widget.Opacity == 1.0);
			Assert.True(adorner.Widget.Visible);
			Assert.Equal(1.0, adorner.Widget.Opacity);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Visibility_and_IsHitTestVisible_changes_keep_a_placed_adorner_untargetable() => Run(() =>
	{
		var (win, _, target, _) = Build();
		try
		{
			ShowAndLayout(win, target);
			var adorner = new WidgetAdorner(target);
			AdornerLayer.GetAdornerLayer(target)!.Add(adorner);
			Assert.True(GtkPump.Until(
				() => AdornerPlacementBoundsGtkTests.Covers(OverlayOf(win), target, adorner)),
				"adorner never placed");
			adorner.Visibility = Visibility.Hidden;
			adorner.Visibility = Visibility.Visible;
			Assert.False(adorner.Widget.CanTarget);
			adorner.IsHitTestVisible = false;
			adorner.IsHitTestVisible = true;
			Assert.False(adorner.Widget.CanTarget);
			adorner.IsEnabled = false;
			adorner.IsEnabled = true;
			Assert.False(adorner.Widget.CanTarget);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Unplace_applies_the_current_IsHitTestVisible_not_a_stale_snapshot() => Run(() =>
	{
		var (win, _, target, _) = Build();
		var adorner = new WidgetAdorner(target);
		var layer = win.ImplicitAdornerLayer;
		_ = win.Widget;
		layer.Add(adorner);
		adorner.IsHitTestVisible = false;
		layer.Remove(adorner);
		Assert.False(adorner.Widget.CanTarget);
		adorner.IsHitTestVisible = true;
		Assert.True(adorner.Widget.CanTarget);
	});

	[Fact]
	public void Decorator_layer_adorner_returns_after_the_adorned_element_is_collapsed_and_shown() => Run(() =>
	{
		var (_, panel, target, _) = Build();
		var decorator = new AdornerDecorator { Child = panel };
		var win = new WpfWindow { Width = 400, Height = 300, Content = decorator };
		try
		{
			ShowAndLayout(win, target);
			var adorner = new WidgetAdorner(target);
			decorator.AdornerLayer.Add(adorner);
			GtkPump.Until(() => adorner.Widget.Visible
				&& AdornerPlacementBoundsGtkTests.Covers((Gtk.Overlay)decorator.Widget, target, adorner));
			Assert.True(adorner.Widget.Visible);
			target.Visibility = Visibility.Collapsed;
			GtkPump.Until(() => !adorner.Widget.Visible);
			target.Visibility = Visibility.Visible;
			GtkPump.Until(() => adorner.Widget.Visible);
			Assert.True(adorner.Widget.Visible);
			GtkPump.Until(() => AdornerPlacementBoundsGtkTests.Covers((Gtk.Overlay)decorator.Widget, target, adorner));
			AdornerPlacementBoundsGtkTests.AssertCovers(
				(Gtk.Overlay)decorator.Widget, target, adorner);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Decorator_layer_frame_layout_returns_an_adorner_after_a_GTK_only_show() => Run(() =>
	{
		var (_, panel, target, _) = Build();
		var decorator = new AdornerDecorator { Child = panel };
		var win = new WpfWindow { Width = 400, Height = 300, Content = decorator };
		try
		{
			ShowAndLayout(win, target);
			var adorner = new WidgetAdorner(target);
			decorator.AdornerLayer.Add(adorner);
			Assert.True(GtkPump.Until(
				() => AdornerPlacementBoundsGtkTests.Covers((Gtk.Overlay)decorator.Widget, target, adorner)),
				"adorner never placed");
			// No WPF event (LayoutUpdated, SizeChanged) fires for a widget-level hide/show; only the frame layout can
			// bring the adorner back.
			target.Widget.Visible = false;
			decorator.AdornerLayer.RepositionAll();
			Assert.False(adorner.Widget.Visible);
			target.Widget.Visible = true;
			GtkPump.Until(() => adorner.Widget.Visible);
			Assert.True(adorner.Widget.Visible);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Placed_adorner_keeps_mouse_capture_and_receives_retargeted_input() => Run(() =>
	{
		var (win, _, target, _) = Build();
		try
		{
			ShowAndLayout(win, target);
			var adorner = new WidgetAdorner(target);
			AdornerLayer.GetAdornerLayer(target)!.Add(adorner);
			Assert.True(GtkPump.Until(
				() => AdornerPlacementBoundsGtkTests.Covers(OverlayOf(win), target, adorner)),
				"adorner never placed");
			int moves = 0, ups = 0;
			adorner.MouseMove += (_, _) => moves++;
			adorner.MouseUp += (_, _) => ups++;
			Assert.True(adorner.CaptureMouse());
			// GTK delivers the pick over the adorned element to it; capture re-targets to the adorner.
			target.RaiseMouseMove(new Point(5, 5));
			target.RaiseMouseButton(MouseButton.Left, new Point(5, 5), 1, false);
			Assert.Equal(1, moves);
			Assert.Equal(1, ups);
			adorner.ReleaseMouseCapture();
			Assert.Null(Mouse.Captured);
		}
		finally { win.Close(); }
	});
}
