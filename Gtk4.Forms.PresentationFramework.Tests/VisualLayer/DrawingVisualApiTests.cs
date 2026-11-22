using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.VisualLayer;

public class DrawingVisualApiTests : WpfGtkTestBase
{
	[Fact]
	public void RenderOpen_returns_a_DrawingContext_and_Close_commits_once() => Run(() =>
	{
		var dv = new DrawingVisual();
		DrawingContext dc = dv.RenderOpen();
		dc.DrawRectangle(Brushes.Red, null, new Rect(0, 0, 5, 5));
		Assert.Null(dv.Drawing);
		dc.Close();
		Assert.Same(dc, dv.Drawing);
		dc.DrawEllipse(Brushes.Red, null, new Point(1, 1), 1, 1);
		dc.Close();
		dc.Dispose();
		Assert.Same(dc, dv.Drawing);
	});

	[Fact]
	public void DrawingVisual_is_a_ContainerVisual_with_opacity_transform_and_offset() => Run(() =>
	{
		ContainerVisual c = new DrawingVisual { Opacity = 0.4, Offset = new Vector(3, 4) };
		Assert.Equal(0.4, c.Opacity);
		Assert.Equal(new Vector(3, 4), c.Offset);
		Assert.Equal(new Matrix(1, 0, 0, 1, 3, 4), ((ContainerVisual)c).TransformToAncestor_Matrix());
		Assert.Equal(1.0, new ContainerVisual().Opacity);
	});

	[Fact]
	public void ContentBounds_and_DescendantBounds_follow_the_drawing() => Run(() =>
	{
		var root = new ContainerVisual();
		var child = new RectVisual(new Rect(0, 0, 10, 10), new Vector(5, 5));
		root.Children.Add(child);
		Assert.True(root.ContentBounds.IsEmpty);
		Assert.Equal(new Rect(0, 0, 10, 10), child.ContentBounds);
		Assert.Equal(new Rect(5, 5, 10, 10), root.DescendantBounds);
	});

	[Fact]
	public void FormattedText_keeps_the_values_of_the_six_argument_constructor() => Run(() =>
	{
		var face = new Typeface("Verdana");
#pragma warning disable CS0618
		var t = new FormattedText("Hi", CultureInfo.GetCultureInfo("en-us"), FlowDirection.RightToLeft, face, 36,
			Brushes.Black);
#pragma warning restore CS0618
		Assert.Equal("Hi", t.Text);
		Assert.Equal(FlowDirection.RightToLeft, t.FlowDirection);
		Assert.Same(face, t.Typeface);
		Assert.Equal(36, t.FontSize);
		Assert.Same(Brushes.Black.GetType(), t.Foreground!.GetType());
		Assert.True(t.Width > 0);
		Assert.True(t.Height > 0);
	});

	[Fact]
	public void The_six_argument_FormattedText_constructor_is_obsolete() =>
		Assert.NotNull(Attribute.GetCustomAttribute(typeof(FormattedText).GetConstructor(new[]
		{
			typeof(string), typeof(CultureInfo), typeof(FlowDirection), typeof(Typeface), typeof(double), typeof(Brush),
		})!, typeof(ObsoleteAttribute)));

	[Fact]
	public void Drawn_text_widens_the_hit_bounds() => Run(() =>
	{
		var dv = new DrawingVisual();
		using (var dc = dv.RenderOpen())
			dc.DrawText(new FormattedText("Click Me!", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
				new Typeface("Verdana"), 36, Brushes.Black, 1.0), new Point(200, 116));
		Assert.NotNull(VisualTreeHelper.HitTest(dv, new Point(210, 130)));
		Assert.Null(VisualTreeHelper.HitTest(dv, new Point(10, 10)));
	});

	[Fact]
	public void Hosted_drawing_visuals_paint_through_the_render_layer_with_offset_and_opacity() => Run(() =>
	{
		var host = new VisualHost { Width = 40, Height = 40 };
		var dv = new RectVisual(new Rect(0, 0, 10, 10), new Vector(10, 10));
		host.Visuals.Add(dv);
		_ = host.Widget;
		host.AddV(dv);
		Assert.True(host.HasRenderLayer);
		Assert.Equal(255, PixelAlpha(host, 15, 15));
		Assert.Equal(0, PixelAlpha(host, 5, 5));
		dv.Opacity = 0.5;
		int half = PixelAlpha(host, 15, 15);
		Assert.InRange(half, 120, 135);
	});

	[Fact]
	public void OnRender_overrider_gets_a_render_layer_and_InvalidateVisual_reruns_it() => Run(() =>
	{
		var el = new RenderingElement();
		_ = el.Widget;
		Assert.True(el.HasRenderLayer);
		Assert.Equal(255, PixelAlpha(el, 5, 5));
		Assert.Equal(1, el.Renders);
		PixelAlpha(el, 5, 5);
		Assert.Equal(1, el.Renders);
		el.InvalidateVisual();
		PixelAlpha(el, 5, 5);
		Assert.Equal(2, el.Renders);
	});

	[Fact]
	public void Plain_elements_get_no_render_layer() => Run(() =>
	{
		var el = new VisualHost();
		_ = el.Widget;
		Assert.False(el.HasRenderLayer);
	});

	static int PixelAlpha(UIElement el, int x, int y)
	{
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 40, 40);
		using (var cr = new Cairo.Context(surface)) el.PaintRenderLayer(cr);
		surface.Flush();
		var data = surface.Data;
		return data[y * surface.Stride + x * 4 + 3];
	}
}

static class MatrixExt
{
	internal static Matrix TransformToAncestor_Matrix(this ContainerVisual v)
	{
		var parent = new ContainerVisual();
		parent.Children.Add(v);
		return ((MatrixTransform)v.TransformToAncestor(parent)).Matrix;
	}
}
