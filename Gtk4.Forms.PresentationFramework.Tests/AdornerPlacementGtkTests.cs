using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;
using static Gtk4.Forms.PresentationFramework.Tests.WindowAdornerLayerTests;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Adorner widgets are overlay children placed over the adorned element.</summary>
public class AdornerPlacementGtkTests : WpfGtkTestBase
{
	static (int x, int y) Origin(UIElement el, Gtk.Widget main)
	{
		var p = new Graphene.Point();
		p.X = 0;
		p.Y = 0;
		Assert.True(el.Widget.ComputePoint(main, p, out var to));
		return ((int)Math.Round(to.X), (int)Math.Round(to.Y));
	}

	[Fact]
	public void Adorner_added_after_show_is_placed_over_adorned_element() => Run(() =>
	{
		var (win, _, target, _) = Build();
		try
		{
			ShowAndLayout(win, target);
			var adorner = new WidgetAdorner(target);
			AdornerLayer.GetAdornerLayer(target)!.Add(adorner);
			GtkPump.For(100);
			var overlay = OverlayOf(win);
			Assert.Same(overlay, adorner.Widget.Parent);
			var (x, y) = Origin(target, overlay.Child);
			Assert.True(y >= 40);
			Assert.Equal(x, adorner.Widget.MarginStart);
			Assert.Equal(y, adorner.Widget.MarginTop);
			Assert.Equal(Gtk.Align.Start, adorner.Widget.Halign);
			Assert.Equal(Gtk.Align.Start, adorner.Widget.Valign);
			Assert.Equal((int)Math.Ceiling(target.ActualWidth), adorner.Widget.WidthRequest);
			Assert.Equal((int)Math.Ceiling(target.ActualHeight), adorner.Widget.HeightRequest);
			Assert.False(overlay.GetMeasureOverlay(adorner.Widget));
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Adorner_added_before_show_is_placed_at_show() => Run(() =>
	{
		var (win, _, target, _) = Build();
		var adorner = new WidgetAdorner(target);
		AdornerLayer.GetAdornerLayer(target)!.Add(adorner);
		var overlay = OverlayOf(win);
		Assert.Same(overlay, adorner.Widget.Parent);
		try
		{
			ShowAndLayout(win, target);
			var (x, y) = Origin(target, overlay.Child);
			Assert.Equal(x, adorner.Widget.MarginStart);
			Assert.Equal(y, adorner.Widget.MarginTop);
			Assert.Equal((int)Math.Ceiling(target.ActualHeight), adorner.Widget.HeightRequest);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Adorner_added_before_widget_exists_is_placed_when_overlay_assigned() => Run(() =>
	{
		var target = new Button();
		var win = new WpfWindow { Content = target };
		var adorner = new WidgetAdorner(target);
		win.ImplicitAdornerLayer.Add(adorner);
		Assert.Null(adorner.Widget.Parent);
		Assert.Same(OverlayOf(win), adorner.Widget.Parent);
	});

	[Fact]
	public void Remove_detaches_overlay_child() => Run(() =>
	{
		var (win, _, target, _) = Build();
		var adorner = new WidgetAdorner(target);
		var layer = win.ImplicitAdornerLayer;
		_ = win.Widget;
		layer.Add(adorner);
		Assert.NotNull(adorner.Widget.Parent);
		layer.Remove(adorner);
		Assert.Null(adorner.Widget.Parent);
		Assert.Null(layer.GetAdorners(target));
	});

	[Fact]
	public void Remove_of_unplaced_adorner_is_safe() => Run(() =>
	{
		var target = new Button();
		var win = new WpfWindow { Content = target };
		var adorner = new WidgetAdorner(target);
		win.ImplicitAdornerLayer.Add(adorner);
		win.ImplicitAdornerLayer.Remove(adorner);
		_ = win.Widget;
		Assert.Null(adorner.Widget.Parent);
	});

	[Fact]
	public void Update_repositions_after_adorned_element_moves() => Run(() =>
	{
		var (win, _, target, spacer) = Build();
		try
		{
			ShowAndLayout(win, target);
			var layer = AdornerLayer.GetAdornerLayer(target)!;
			var adorner = new WidgetAdorner(target);
			layer.Add(adorner);
			GtkPump.For(100);
			var overlay = OverlayOf(win);
			int before = adorner.Widget.MarginTop;
			spacer.Widget.SetSizeRequest(-1, 90);
			GtkPump.For(300);
			layer.Update(target);
			var (_, y) = Origin(target, overlay.Child);
			Assert.True(y > before);
			Assert.Equal(y, adorner.Widget.MarginTop);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Adorner_resizes_with_adorned_element() => Run(() =>
	{
		var (win, _, target, spacer) = Build();
		try
		{
			ShowAndLayout(win, target);
			var adorner = new WidgetAdorner(spacer);
			AdornerLayer.GetAdornerLayer(spacer)!.Add(adorner);
			GtkPump.For(100);
			spacer.Widget.SetSizeRequest(-1, 90);
			GtkState.Require(() => adorner.Widget.HeightRequest == 90, "adorner height follows", spacer);
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
			double before = panel.ActualHeight;
			var big = new Border { Width = 5000, Height = 5000 };
			AdornerLayer.GetAdornerLayer(target)!.Add(new WidgetAdorner(big));
			GtkPump.For(100);
			Assert.Equal(before, panel.ActualHeight);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Duplicate_add_still_throws_on_window_layer() => Run(() =>
	{
		var target = new Button();
		var win = new WpfWindow { Content = target };
		var adorner = new WidgetAdorner(target);
		win.ImplicitAdornerLayer.Add(adorner);
		Assert.Throws<InvalidOperationException>((Action)(() => win.ImplicitAdornerLayer.Add(adorner)));
	});

	[Fact]
	public void Adorner_in_explicit_decorator_is_placed_in_its_overlay() => Run(() =>
	{
		var target = new Button { Content = "t" };
		var dec = new AdornerDecorator { Child = target };
		var adorner = new WidgetAdorner(target);
		dec.AdornerLayer.Add(adorner);
		Assert.Same((Gtk.Overlay)dec.Widget, adorner.Widget.Parent);
	});

	[Fact]
	public void Overlay_unassigned_unplaces_adorners() => Run(() =>
	{
		var target = new Button();
		var win = new WpfWindow { Content = target };
		var adorner = new WidgetAdorner(target);
		win.ImplicitAdornerLayer.Add(adorner);
		_ = win.Widget;
		Assert.NotNull(adorner.Widget.Parent);
		win.ImplicitAdornerLayer.GtkOverlay = null;
		Assert.Null(adorner.Widget.Parent);
	});
}
