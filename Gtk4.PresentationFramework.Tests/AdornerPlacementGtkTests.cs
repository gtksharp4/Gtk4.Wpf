using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;
using static Gtk4.PresentationFramework.Tests.WindowAdornerLayerTests;
using static Gtk4.PresentationFramework.Tests.AdornerPlacementBoundsGtkTests;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Adorner widgets are overlay children placed over the adorned element.</summary>
public class AdornerPlacementGtkTests : WpfGtkTestBase
{
	[Fact]
	public void Adorner_added_after_show_is_placed_over_adorned_element() => Run(() =>
	{
		var (win, _, target, _) = Build();
		try
		{
			ShowAndLayout(win, target);
			var adorner = new WidgetAdorner(target);
			AdornerLayer.GetAdornerLayer(target)!.Add(adorner);
			GtkPump.Until(() => Covers(OverlayOf(win), target, adorner));
			var overlay = OverlayOf(win);
			Assert.Same(overlay, adorner.Widget.Parent);
			AssertCovers(overlay, target, adorner);
			Assert.True(adorner.Widget.MarginTop >= 40);
			Assert.Equal(Gtk.Align.Start, adorner.Widget.Halign);
			Assert.Equal(Gtk.Align.Start, adorner.Widget.Valign);
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
			AssertCovers(overlay, target, adorner);
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
			Assert.True(GtkPump.Until(() => Covers(OverlayOf(win), target, adorner)), "adorner never placed");
			var overlay = OverlayOf(win);
			int before = adorner.Widget.MarginTop;
			spacer.Widget.SetSizeRequest(-1, 90);
			GtkPump.Until(() => spacer.Widget.Height >= 90);
			layer.Update(target);
			Assert.True(adorner.Widget.MarginTop > before);
			AssertCovers(overlay, target, adorner);
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
			Assert.True(GtkPump.Until(() => Covers(OverlayOf(win), spacer, adorner)), "adorner never placed");
			spacer.Widget.SetSizeRequest(-1, 90);
			GtkState.Require(() => adorner.Widget.HeightRequest == 90, "adorner height follows", spacer);
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
