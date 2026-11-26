using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// Pure (no GTK) tests for the drawing object model:
/// <see cref="GeometryDrawing"/>, <see cref="DrawingGroup"/>, and <see cref="DrawingBrush"/>.
/// </summary>
public class DrawingTests
{
	// ── GeometryDrawing: defaults ─────────────────────────────────────────────

	[Fact]
	public void GeometryDrawing_default_Geometry_is_null()
	{
		var d = new GeometryDrawing();
		Assert.Null(d.Geometry);
	}

	[Fact]
	public void GeometryDrawing_default_Brush_is_null()
	{
		var d = new GeometryDrawing();
		Assert.Null(d.Brush);
	}

	[Fact]
	public void GeometryDrawing_default_Pen_is_null()
	{
		var d = new GeometryDrawing();
		Assert.Null(d.Pen);
	}

	[Fact]
	public void GeometryDrawing_null_Geometry_Bounds_is_empty()
	{
		var d = new GeometryDrawing();
		Assert.True(d.Bounds.IsEmpty);
	}

	// ── GeometryDrawing: properties ───────────────────────────────────────────

	[Fact]
	public void GeometryDrawing_Geometry_roundtrip()
	{
		var geom = new RectangleGeometry(new Rect(0, 0, 50, 30));
		var d = new GeometryDrawing { Geometry = geom };
		Assert.Same(geom, d.Geometry);
	}

	[Fact]
	public void GeometryDrawing_Brush_roundtrip()
	{
		var brush = new SolidColorBrush(Colors.Red);
		var d = new GeometryDrawing { Brush = brush };
		Assert.Same(brush, d.Brush);
	}

	[Fact]
	public void GeometryDrawing_Pen_roundtrip()
	{
		var pen = new Pen(new SolidColorBrush(Colors.Black), 2);
		var d = new GeometryDrawing { Pen = pen };
		Assert.Same(pen, d.Pen);
	}

	[Fact]
	public void GeometryDrawing_Bounds_reflects_geometry_bounds()
	{
		var geom = new RectangleGeometry(new Rect(10, 20, 100, 50));
		var d = new GeometryDrawing { Geometry = geom };
		Assert.Equal(new Rect(10, 20, 100, 50), d.Bounds);
	}

	[Fact]
	public void GeometryDrawing_is_a_Drawing()
	{
		Assert.IsAssignableFrom<Drawing>(new GeometryDrawing());
	}

	// ── GeometryDrawing: Cairo draw (headless surface) ────────────────────────

	[Fact]
	public void GeometryDrawing_Draw_with_null_geometry_does_not_throw()
	{
		var d = new GeometryDrawing { Brush = new SolidColorBrush(Colors.Blue) };
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using var cr = new Cairo.Context(surface);
		var ex = Record.Exception(() => d.Draw(cr));
		Assert.Null(ex);
	}

	[Fact]
	public void GeometryDrawing_Draw_fills_and_strokes_without_throw()
	{
		var d = new GeometryDrawing
		{
			Geometry = new RectangleGeometry(new Rect(0, 0, 50, 30)),
			Brush = new SolidColorBrush(Colors.Blue),
			Pen = new Pen(new SolidColorBrush(Colors.Black), 1),
		};
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using var cr = new Cairo.Context(surface);
		var ex = Record.Exception(() => d.Draw(cr));
		Assert.Null(ex);
	}

	[Fact]
	public void GeometryDrawing_Draw_null_pen_brush_does_not_throw()
	{
		var d = new GeometryDrawing
		{
			Geometry = new EllipseGeometry(new Point(50, 50), 25, 25),
		};
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using var cr = new Cairo.Context(surface);
		var ex = Record.Exception(() => d.Draw(cr));
		Assert.Null(ex);
	}

	// ── DrawingGroup: defaults ────────────────────────────────────────────────

	[Fact]
	public void DrawingGroup_Children_starts_empty()
	{
		var g = new DrawingGroup();
		Assert.Empty(g.Children);
	}

	[Fact]
	public void DrawingGroup_default_Opacity_is_one()
	{
		var g = new DrawingGroup();
		Assert.Equal(1.0, g.Opacity);
	}

	[Fact]
	public void DrawingGroup_default_Transform_is_null()
	{
		var g = new DrawingGroup();
		Assert.Null(g.Transform);
	}

	[Fact]
	public void DrawingGroup_default_ClipGeometry_is_null()
	{
		var g = new DrawingGroup();
		Assert.Null(g.ClipGeometry);
	}

	// ── DrawingGroup: Children add/remove ─────────────────────────────────────

	[Fact]
	public void DrawingGroup_Children_add_increases_count()
	{
		var g = new DrawingGroup();
		g.Children.Add(new GeometryDrawing());
		Assert.Single(g.Children);
	}

	[Fact]
	public void DrawingGroup_Children_remove_decreases_count()
	{
		var g = new DrawingGroup();
		var child = new GeometryDrawing();
		g.Children.Add(child);
		g.Children.Remove(child);
		Assert.Empty(g.Children);
	}

	[Fact]
	public void DrawingGroup_is_a_Drawing()
	{
		Assert.IsAssignableFrom<Drawing>(new DrawingGroup());
	}

	// ── DrawingGroup: Bounds ──────────────────────────────────────────────────

	[Fact]
	public void DrawingGroup_empty_children_Bounds_is_empty()
	{
		var g = new DrawingGroup();
		Assert.True(g.Bounds.IsEmpty);
	}

	[Fact]
	public void DrawingGroup_Bounds_is_union_of_children()
	{
		var g = new DrawingGroup();
		g.Children.Add(new GeometryDrawing { Geometry = new RectangleGeometry(new Rect(0, 0, 50, 50)) });
		g.Children.Add(new GeometryDrawing { Geometry = new RectangleGeometry(new Rect(50, 50, 50, 50)) });
		var b = g.Bounds;
		Assert.Equal(0, b.X);
		Assert.Equal(0, b.Y);
		Assert.Equal(100, b.Width);
		Assert.Equal(100, b.Height);
	}

	[Fact]
	public void DrawingGroup_Bounds_ignores_empty_child_bounds()
	{
		var g = new DrawingGroup();
		g.Children.Add(new GeometryDrawing()); // null geometry → empty bounds
		g.Children.Add(new GeometryDrawing { Geometry = new RectangleGeometry(new Rect(10, 10, 20, 20)) });
		var b = g.Bounds;
		Assert.Equal(new Rect(10, 10, 20, 20), b);
	}

	// ── DrawingGroup: Cairo draw ──────────────────────────────────────────────

	[Fact]
	public void DrawingGroup_Draw_empty_children_does_not_throw()
	{
		var g = new DrawingGroup();
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using var cr = new Cairo.Context(surface);
		var ex = Record.Exception(() => g.Draw(cr));
		Assert.Null(ex);
	}

	[Fact]
	public void DrawingGroup_Draw_multiple_children_does_not_throw()
	{
		var g = new DrawingGroup();
		g.Children.Add(new GeometryDrawing
		{
			Geometry = new RectangleGeometry(new Rect(0, 0, 50, 50)),
			Brush = new SolidColorBrush(Colors.Red),
		});
		g.Children.Add(new GeometryDrawing
		{
			Geometry = new EllipseGeometry(new Point(50, 50), 20, 20),
			Brush = new SolidColorBrush(Colors.Blue),
		});
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using var cr = new Cairo.Context(surface);
		var ex = Record.Exception(() => g.Draw(cr));
		Assert.Null(ex);
	}

	[Fact]
	public void DrawingGroup_Draw_with_opacity_does_not_throw()
	{
		var g = new DrawingGroup { Opacity = 0.5 };
		g.Children.Add(new GeometryDrawing
		{
			Geometry = new RectangleGeometry(new Rect(0, 0, 40, 40)),
			Brush = new SolidColorBrush(Colors.Green),
		});
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using var cr = new Cairo.Context(surface);
		var ex = Record.Exception(() => g.Draw(cr));
		Assert.Null(ex);
	}

	// ── DrawingGroupMath.Union (pure helper) ──────────────────────────────────

	[Fact]
	public void DrawingGroupMath_Union_empty_collection_is_empty()
	{
		var result = DrawingGroupMath.Union(System.Array.Empty<Drawing>());
		Assert.True(result.IsEmpty);
	}

	[Fact]
	public void DrawingGroupMath_Union_single_rect_returns_it()
	{
		var d = new GeometryDrawing { Geometry = new RectangleGeometry(new Rect(5, 10, 40, 20)) };
		var result = DrawingGroupMath.Union(new Drawing[] { d });
		Assert.Equal(new Rect(5, 10, 40, 20), result);
	}

	[Fact]
	public void DrawingGroupMath_Union_two_rects_returns_bounding_box()
	{
		var d1 = new GeometryDrawing { Geometry = new RectangleGeometry(new Rect(0, 0, 30, 20)) };
		var d2 = new GeometryDrawing { Geometry = new RectangleGeometry(new Rect(20, 10, 30, 20)) };
		var result = DrawingGroupMath.Union(new Drawing[] { d1, d2 });
		Assert.Equal(0, result.X);
		Assert.Equal(0, result.Y);
		Assert.Equal(50, result.Width);
		Assert.Equal(30, result.Height);
	}

	// ── Rect.Empty / Rect.IsEmpty / Rect.Union ────────────────────────────────

	[Fact]
	public void Rect_Empty_IsEmpty_is_true()
	{
		Assert.True(Rect.Empty.IsEmpty);
	}

	[Fact]
	public void Rect_non_empty_IsEmpty_is_false()
	{
		Assert.False(new Rect(0, 0, 1, 1).IsEmpty);
	}

	[Fact]
	public void Rect_Union_two_non_empty_rects()
	{
		var a = new Rect(0, 0, 10, 10);
		var b = new Rect(5, 5, 20, 20);
		var u = Rect.Union(a, b);
		Assert.Equal(0, u.X);
		Assert.Equal(0, u.Y);
		Assert.Equal(25, u.Width);
		Assert.Equal(25, u.Height);
	}

	[Fact]
	public void Rect_Union_empty_with_real_returns_real()
	{
		var real = new Rect(10, 10, 50, 50);
		Assert.Equal(real, Rect.Union(Rect.Empty, real));
		Assert.Equal(real, Rect.Union(real, Rect.Empty));
	}

	// ── DrawingBrush: defaults ────────────────────────────────────────────────

	[Fact]
	public void DrawingBrush_default_Drawing_is_null()
	{
		var b = new DrawingBrush();
		Assert.Null(b.Drawing);
	}

	[Fact]
	public void DrawingBrush_default_Stretch_is_Fill()
	{
		var b = new DrawingBrush();
		Assert.Equal(Stretch.Fill, b.Stretch);
	}

	[Fact]
	public void DrawingBrush_default_TileMode_is_None()
	{
		var b = new DrawingBrush();
		Assert.Equal(TileMode.None, b.TileMode);
	}

	[Fact]
	public void DrawingBrush_default_Opacity_is_one()
	{
		var b = new DrawingBrush();
		Assert.Equal(1.0, b.Opacity);
	}

	[Fact]
	public void DrawingBrush_is_a_TileBrush()
	{
		Assert.IsAssignableFrom<TileBrush>(new DrawingBrush());
	}

	[Fact]
	public void DrawingBrush_is_a_Brush()
	{
		Assert.IsAssignableFrom<Brush>(new DrawingBrush());
	}

	// ── DrawingBrush: properties ──────────────────────────────────────────────

	[Fact]
	public void DrawingBrush_ctor_sets_Drawing()
	{
		var drawing = new GeometryDrawing();
		var b = new DrawingBrush(drawing);
		Assert.Same(drawing, b.Drawing);
	}

	[Fact]
	public void DrawingBrush_Drawing_roundtrip()
	{
		var b = new DrawingBrush();
		var drawing = new DrawingGroup();
		b.Drawing = drawing;
		Assert.Same(drawing, b.Drawing);
	}

	// ── DrawingBrush: headless assign to Shape.Fill ───────────────────────────

	[Fact]
	public void DrawingBrush_assigned_to_Rectangle_Fill_does_not_throw()
	{
		var r = new Rectangle();
		var brush = new DrawingBrush
		{
			Drawing = new GeometryDrawing
			{
				Geometry = new RectangleGeometry(new Rect(0, 0, 50, 50)),
				Brush = new SolidColorBrush(Colors.Blue),
			},
		};
		var ex = Record.Exception(() => r.Fill = brush);
		Assert.Null(ex);
		Assert.Same(brush, r.Fill);
	}

	[Fact]
	public void DrawingBrush_null_Drawing_assigned_to_Ellipse_Fill_does_not_throw()
	{
		var e = new Ellipse();
		var brush = new DrawingBrush();
		var ex = Record.Exception(() => e.Fill = brush);
		Assert.Null(ex);
	}

	// ── DrawingBrush: ApplyToCairo on headless surface ────────────────────────

	[Fact]
	public void DrawingBrush_ApplyToCairo_null_drawing_is_no_op()
	{
		var b = new DrawingBrush();
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using var cr = new Cairo.Context(surface);
		var ex = Record.Exception(() => b.ApplyToCairo(cr, 100, 100));
		Assert.Null(ex);
	}

	[Fact]
	public void DrawingBrush_ApplyToCairo_with_drawing_does_not_throw()
	{
		var drawing = new GeometryDrawing
		{
			Geometry = new RectangleGeometry(new Rect(0, 0, 40, 30)),
			Brush = new SolidColorBrush(Colors.Red),
		};
		var b = new DrawingBrush(drawing);
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using var cr = new Cairo.Context(surface);
		var ex = Record.Exception(() => b.ApplyToCairo(cr, 100, 100));
		Assert.Null(ex);
	}

	[Fact]
	public void DrawingBrush_ApplyToCairo_zero_bounds_drawing_is_no_op()
	{
		// GeometryDrawing with null geometry → zero/empty bounds.
		var b = new DrawingBrush(new GeometryDrawing());
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using var cr = new Cairo.Context(surface);
		var ex = Record.Exception(() => b.ApplyToCairo(cr, 100, 100));
		Assert.Null(ex);
	}
}
