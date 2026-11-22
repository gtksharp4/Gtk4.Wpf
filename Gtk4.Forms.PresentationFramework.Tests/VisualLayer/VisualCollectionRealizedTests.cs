using System;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.VisualLayer;

/// <summary>A VisualCollection child added after the host is realized must create the render layer.</summary>
public class VisualCollectionRealizedTests : WpfGtkTestBase
{
	class KidsHost : FrameworkElement
	{
		public readonly VisualCollection Kids;
		public KidsHost() { Kids = new VisualCollection(this); }
		protected internal override int VisualChildrenCount => Kids.Count;
		protected internal override Visual GetVisualChild(int index) => Kids[index];
	}

	static void Pump()
	{
		for (int i = 0; i < 200 && Gtk.Application.EventsPending(); i++)
			Gtk.Application.RunIteration(false);
	}

	static (int r, int a) Pixel(UIElement el, int x, int y)
	{
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 40, 40);
		using (var cr = new Cairo.Context(surface)) el.PaintRenderLayer(cr);
		surface.Flush();
		var d = surface.Data;
		int o = y * surface.Stride + x * 4;
		return (d[o + 2], d[o + 3]);
	}

	static void Realized(Action<KidsHost> body)
	{
		var host = new KidsHost { Width = 40, Height = 40 };
		var win = new Gtk.Window();
		win.SetDefaultSize(100, 100);
		win.Child = host.Widget;
		win.Present();
		try
		{
			Pump();
			Assert.False(host.HasRenderLayer);
			body(host);
		}
		finally { win.Destroy(); }
	}

	static void AssertRed(KidsHost host)
	{
		Pump();
		Assert.True(host.HasRenderLayer);
		Assert.Equal((255, 255), Pixel(host, 5, 5));
	}

	[Fact]
	public void Add_after_realization_creates_the_render_layer() => Run(() =>
		Realized(host =>
		{
			host.Kids.Add(new RectVisual(new Rect(0, 0, 10, 10)));
			AssertRed(host);
		}));

	[Fact]
	public void Insert_after_realization_creates_the_render_layer() => Run(() =>
		Realized(host =>
		{
			host.Kids.Insert(0, new RectVisual(new Rect(0, 0, 10, 10)));
			AssertRed(host);
		}));

	[Fact]
	public void Add_of_a_visual_parented_elsewhere_throws_and_leaves_Count_unchanged() => Run(() =>
	{
		var first = new KidsHost();
		var second = new KidsHost();
		var v = new RectVisual(new Rect(0, 0, 10, 10));
		first.Kids.Add(v);
		Assert.Throws<ArgumentException>(() => second.Kids.Add(v));
		Assert.Throws<ArgumentException>(() => second.Kids.Insert(0, v));
		Assert.Equal(0, second.Kids.Count);
		Assert.Equal(1, first.Kids.Count);
	});

	[Fact]
	public void Indexer_set_of_a_visual_parented_elsewhere_restores_the_old_child() => Run(() =>
	{
		var first = new KidsHost();
		var second = new KidsHost();
		var mine = new RectVisual(new Rect(0, 0, 10, 10));
		var theirs = new RectVisual(new Rect(0, 0, 10, 10));
		first.Kids.Add(theirs);
		second.Kids.Add(mine);
		Assert.Throws<ArgumentException>(() => second.Kids[0] = theirs);
		Assert.Same(mine, second.Kids[0]);
		Assert.Same(second, VisualTreeHelper.GetParent(mine));
	});
}
