using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.VisualLayer;

/// <summary>The render layer sits under the children and never changes how a Panel arranges them.</summary>
public class RenderLayerHostTests : WpfGtkTestBase
{
	class RenderingStack : StackPanel
	{
		protected override void OnRender(DrawingContext dc) =>
			dc.DrawRectangle(Brushes.Red, null, new Rect(0, 0, 10, 10));
	}

	class VisualStack : StackPanel
	{
		public readonly System.Collections.Generic.List<Visual> Extra = new();
		public void AddV(Visual v) { Extra.Add(v); AddVisualChild(v); }
		protected internal override int VisualChildrenCount => Children.Count + Extra.Count;
		protected internal override Visual GetVisualChild(int index) =>
			index < Children.Count ? Children[index] : Extra[index - Children.Count];
	}

	static void Fill(StackPanel p)
	{
		for (int i = 0; i < 3; i++) p.Children.Add(new Border { Height = 20 });
		_ = p.Widget;
		p.Widget.Allocate(200, 300, -1, null);
	}

	static double[] Ys(StackPanel p)
	{
		var ys = new double[p.Children.Count];
		for (int i = 0; i < ys.Length; i++)
		{
			Assert.True(p.Children[i].Widget.ComputePoint(p.Widget, new Graphene.Point { X = 0, Y = 0 }, out var at));
			ys[i] = at.Y;
		}
		return ys;
	}

	static void AssertStacked(StackPanel p)
	{
		var ys = Ys(p);
		Assert.True(ys[0] < ys[1] && ys[1] < ys[2], $"children overlay: {string.Join(",", ys)}");
		Assert.IsNotType<Gtk.BinLayout>(p.Widget.LayoutManager);
	}

	[Fact]
	public void A_StackPanel_that_overrides_OnRender_still_stacks_and_keeps_its_layout() => Run(() =>
	{
		var p = new RenderingStack();
		Fill(p);
		AssertStacked(p);
	});

	[Fact]
	public void A_StackPanel_hosting_a_DrawingVisual_still_stacks() => Run(() =>
	{
		var p = new VisualStack();
		p.AddV(new RectVisual(new Rect(0, 0, 5, 5)));
		Fill(p);
		AssertStacked(p);
	});

	[Fact]
	public void The_render_layer_does_not_make_the_host_expand() => Run(() =>
	{
		var el = new RenderingElement();
		_ = el.Widget;
		Assert.True(el.HasRenderLayer);
		Assert.False(el.Widget.ComputeExpand(Gtk.Orientation.Horizontal));
		Assert.False(el.Widget.ComputeExpand(Gtk.Orientation.Vertical));
	});

	[Fact]
	public void A_plain_element_that_overrides_OnRender_still_paints() => Run(() =>
	{
		var el = new RenderingElement { Width = 40, Height = 40 };
		_ = el.Widget;
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 40, 40);
		using (var cr = new Cairo.Context(surface)) el.PaintRenderLayer(cr);
		surface.Flush();
		Assert.Equal(255, surface.Data[(5 * 40 + 5) * 4 + 3]);
	});
}
