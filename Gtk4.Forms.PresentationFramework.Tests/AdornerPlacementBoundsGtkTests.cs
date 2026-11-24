using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;
using static Gtk4.Forms.PresentationFramework.Tests.WindowAdornerLayerTests;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Adorner placement measured in overlay coordinates, hit-testing and visibility.</summary>
public class AdornerPlacementBoundsGtkTests : WpfGtkTestBase
{
	sealed class HugeAdorner : Adorner
	{
		public HugeAdorner(UIElement adorned) : base(adorned) { }

		protected override Gtk.Widget CreateWidget()
		{
			var box = new Gtk.Box(Gtk.Orientation.Vertical, 0);
			box.SetSizeRequest(5000, 5000);
			return box;
		}
	}

	internal static Graphene.Rect BoundsIn(Gtk.Widget widget, Gtk.Overlay overlay)
	{
		Assert.True(widget.ComputeBounds(overlay, out var b));
		return b;
	}

	internal static void AssertCovers(Gtk.Overlay overlay, UIElement target, Adorner adorner)
	{
		var t = BoundsIn(target.Widget, overlay);
		var a = BoundsIn(adorner.Widget, overlay);
		Assert.InRange(a.X, t.X - 1, t.X + 1);
		Assert.InRange(a.Y, t.Y - 1, t.Y + 1);
		Assert.InRange(a.Width, t.Width - 1, t.Width + 1);
		Assert.InRange(a.Height, t.Height - 1, t.Height + 1);
	}

	[Fact]
	public void Adorner_covers_the_border_box_of_an_element_inside_border_and_padding() => Run(() =>
	{
		var button = new Button { Content = "b", Padding = new Thickness(15) };
		var stack = new StackPanel();
		stack.Children.Add(button);
		var border = new Border { Padding = new Thickness(20), BorderThickness = new Thickness(5), Child = stack };
		var win = new WpfWindow { Width = 400, Height = 300, Content = border };
		try
		{
			ShowAndLayout(win, button);
			var adorner = new WidgetAdorner(button);
			AdornerLayer.GetAdornerLayer(button)!.Add(adorner);
			GtkPump.For(150);
			AssertCovers(OverlayOf(win), button, adorner);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Placed_adorner_does_not_capture_picks_over_the_adorned_element() => Run(() =>
	{
		var button = new Button { Content = "b", Width = 100, Height = 40 };
		var win = new WpfWindow { Width = 400, Height = 300, Content = button };
		try
		{
			ShowAndLayout(win, button);
			var adorner = new WidgetAdorner(button);
			AdornerLayer.GetAdornerLayer(button)!.Add(adorner);
			GtkPump.For(150);
			var overlay = OverlayOf(win);
			var b = BoundsIn(button.Widget, overlay);
			var hit = overlay.Pick(b.X + b.Width / 2, b.Y + b.Height / 2, Gtk.PickFlags.Default);
			Assert.NotNull(hit);
			Assert.NotSame(adorner.Widget, hit);
			Assert.True(hit == button.Widget || hit!.IsAncestor(button.Widget));
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Unplace_restores_the_adorner_CanTarget() => Run(() =>
	{
		var (win, _, target, _) = Build();
		var adorner = new WidgetAdorner(target);
		var layer = win.ImplicitAdornerLayer;
		_ = win.Widget;
		var before = adorner.Widget.CanTarget;
		layer.Add(adorner);
		Assert.False(adorner.Widget.CanTarget);
		layer.Remove(adorner);
		Assert.Equal(before, adorner.Widget.CanTarget);
	});

	[Fact]
	public void Adorner_hides_while_the_adorned_element_is_collapsed_and_returns() => Run(() =>
	{
		var (win, _, target, _) = Build();
		try
		{
			ShowAndLayout(win, target);
			var adorner = new WidgetAdorner(target);
			AdornerLayer.GetAdornerLayer(target)!.Add(adorner);
			GtkPump.For(150);
			Assert.True(adorner.Widget.Visible);
			target.Visibility = Visibility.Collapsed;
			GtkPump.Until(() => !adorner.Widget.Visible);
			Assert.False(adorner.Widget.Visible);
			target.Visibility = Visibility.Visible;
			GtkPump.Until(() => adorner.Widget.Visible);
			Assert.True(adorner.Widget.Visible);
			GtkPump.For(100);
			AssertCovers(OverlayOf(win), target, adorner);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Adorner_does_not_grow_the_window_content() => Run(() =>
	{
		var (win, panel, target, _) = Build();
		try
		{
			ShowAndLayout(win, target);
			var overlay = OverlayOf(win);
			int w = overlay.Width, h = overlay.Height;
			double before = panel.ActualHeight;
			var adorner = new HugeAdorner(target);
			AdornerLayer.GetAdornerLayer(target)!.Add(adorner);
			adorner.Widget.SetSizeRequest(5000, 5000);
			GtkPump.For(200);
			Assert.Equal(w, overlay.Width);
			Assert.Equal(h, overlay.Height);
			Assert.Equal(before, panel.ActualHeight);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Adorner_follows_a_pure_GTK_move_of_the_adorned_element() => Run(() =>
	{
		var (win, _, target, spacer) = Build();
		try
		{
			ShowAndLayout(win, target);
			var adorner = new WidgetAdorner(target);
			AdornerLayer.GetAdornerLayer(target)!.Add(adorner);
			GtkPump.For(150);
			var overlay = OverlayOf(win);
			double y0 = BoundsIn(target.Widget, overlay).Y;
			// A GTK-only move: no WPF SizeChanged or LayoutUpdated is raised for the adorned element.
			spacer.Widget.SetSizeRequest(-1, 90);
			GtkPump.For(300);
			double y1 = BoundsIn(target.Widget, overlay).Y;
			Assert.True(y1 > y0 + 10, $"target moved {y0} -> {y1}");
			Assert.InRange(BoundsIn(adorner.Widget, overlay).Y, y1 - 1, y1 + 1);
		}
		finally { win.Close(); }
	});
}
