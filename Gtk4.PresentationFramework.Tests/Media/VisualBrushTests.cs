using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// Pure (no GTK) tests for <see cref="VisualBrush"/>, <see cref="DrawingContextRenderer"/>,
/// and the object model contracts (Visual get/set, TileBrush inheritance, headless no-op).
/// </summary>
public class VisualBrushTests
{
	// ── Object model: construction / properties ───────────────────────────────

	[Fact]
	public void Default_Visual_is_null()
	{
		var b = new VisualBrush();
		Assert.Null(b.Visual);
	}

	[Fact]
	public void Ctor_with_visual_sets_Visual()
	{
		var dv = new DrawingVisual();
		var b = new VisualBrush(dv);
		Assert.Same(dv, b.Visual);
	}

	[Fact]
	public void Visual_roundtrip()
	{
		var dv = new DrawingVisual();
		var b = new VisualBrush();
		b.Visual = dv;
		Assert.Same(dv, b.Visual);
	}

	[Fact]
	public void Visual_accepts_null()
	{
		var b = new VisualBrush(new DrawingVisual());
		b.Visual = null;
		Assert.Null(b.Visual);
	}

	// ── TileBrush inheritance ─────────────────────────────────────────────────

	[Fact]
	public void Stretch_default_is_Fill()
	{
		var b = new VisualBrush();
		Assert.Equal(Stretch.Fill, b.Stretch);
	}

	[Fact]
	public void TileMode_default_is_None()
	{
		var b = new VisualBrush();
		Assert.Equal(TileMode.None, b.TileMode);
	}

	[Fact]
	public void Stretch_can_be_set_to_Uniform()
	{
		var b = new VisualBrush { Stretch = Stretch.Uniform };
		Assert.Equal(Stretch.Uniform, b.Stretch);
	}

	[Fact]
	public void Opacity_default_is_one()
	{
		var b = new VisualBrush();
		Assert.Equal(1.0, b.Opacity);
	}

	[Fact]
	public void Is_subtype_of_TileBrush()
	{
		Assert.True(new VisualBrush() is TileBrush);
	}

	// ── Headless no-op / no-throw ─────────────────────────────────────────────

	[Fact]
	public void Assign_DrawingVisual_to_Shape_Fill_does_not_throw()
	{
		var dv = new DrawingVisual();
		using var scope = dv.RenderOpen();
		scope.Context.DrawRectangle(new SolidColorBrush(Colors.Blue), null, new Rect(0, 0, 40, 30));

		var r = new Rectangle { Fill = new VisualBrush(dv) };
		// No Cairo surface involved; object model only.
		Assert.NotNull(r.Fill);
	}

	[Fact]
	public void DrawingVisual_no_committed_drawing_does_not_throw()
	{
		var dv = new DrawingVisual();  // no RenderOpen/commit
		var b = new VisualBrush(dv);
		// ApplyToCairo should be a no-op; we just verify no exception via object model.
		Assert.Null(dv.Drawing);
	}

	[Fact]
	public void General_Visual_subclass_does_not_throw_on_assign()
	{
		// UIElement is a Visual; VisualBrush accepts it (no-paint contract).
		var elem = new System.Windows.Controls.Button();
		var b = new VisualBrush(elem);
		Assert.Same(elem, b.Visual);
	}

	// ── DrawingContextRenderer: bounds computation ────────────────────────────

	[Fact]
	public void ComputeBounds_empty_context_returns_Empty()
	{
		var ctx = new DrawingContext();
		var bounds = DrawingContextRenderer.ComputeBounds(ctx);
		Assert.True(bounds.IsEmpty);
	}

	[Fact]
	public void ComputeBounds_rectangle_command_returns_rect_bounds()
	{
		var ctx = new DrawingContext();
		ctx.DrawRectangle(null, null, new Rect(10, 20, 100, 50));
		var bounds = DrawingContextRenderer.ComputeBounds(ctx);
		Assert.Equal(new Rect(10, 20, 100, 50), bounds);
	}

	[Fact]
	public void ComputeBounds_ellipse_command_returns_bounding_box()
	{
		var ctx = new DrawingContext();
		ctx.DrawEllipse(null, null, new Point(50, 50), 30, 20);
		var bounds = DrawingContextRenderer.ComputeBounds(ctx);
		Assert.Equal(new Rect(20, 30, 60, 40), bounds);
	}

	[Fact]
	public void ComputeBounds_multiple_commands_unions_all()
	{
		var ctx = new DrawingContext();
		ctx.DrawRectangle(null, null, new Rect(0, 0, 50, 50));
		ctx.DrawRectangle(null, null, new Rect(60, 0, 40, 30));
		var bounds = DrawingContextRenderer.ComputeBounds(ctx);
		Assert.Equal(new Rect(0, 0, 100, 50), bounds);
	}

	[Fact]
	public void ComputeBounds_geometry_command_uses_geometry_bounds()
	{
		var ctx = new DrawingContext();
		ctx.DrawGeometry(null, null, new RectangleGeometry(new Rect(5, 5, 90, 70)));
		var bounds = DrawingContextRenderer.ComputeBounds(ctx);
		Assert.Equal(new Rect(5, 5, 90, 70), bounds);
	}

	[Fact]
	public void ComputeBounds_DrawText_command_does_not_contribute_to_bounds()
	{
		var ctx = new DrawingContext();
		ctx.DrawText(new FormattedText("hello"), new Point(0, 0));
		var bounds = DrawingContextRenderer.ComputeBounds(ctx);
		Assert.True(bounds.IsEmpty);
	}

	// ── VisualBrush content-bounds drive surface size ─────────────────────────

	[Fact]
	public void DrawingVisual_with_rect_has_computable_bounds()
	{
		var dv = new DrawingVisual();
		using (var scope = dv.RenderOpen())
			scope.Context.DrawRectangle(new SolidColorBrush(Colors.Red), null, new Rect(0, 0, 80, 60));
		// scope disposed here → dv.Drawing is committed
		var bounds = DrawingContextRenderer.ComputeBounds(dv.Drawing!);
		Assert.Equal(80, bounds.Width);
		Assert.Equal(60, bounds.Height);
	}

	[Fact]
	public void StretchMath_Fill_dest_rect_covers_full_area_for_visual_bounds()
	{
		// 80×60 visual in 200×150 dest → Fill covers all of dest.
		var (x, y, w, h) = StretchMath.ComputeDestRect(80, 60, 200, 150, Stretch.Fill);
		Assert.Equal(0, x);
		Assert.Equal(0, y);
		Assert.Equal(200, w);
		Assert.Equal(150, h);
	}

	[Fact]
	public void StretchMath_Uniform_scales_visual_bounds_correctly()
	{
		// 80×60 visual in 200×200 dest → Uniform: scale=min(200/80,200/60)=2.5 → 200×150, offset (0,25).
		var (x, y, w, h) = StretchMath.ComputeDestRect(80, 60, 200, 200, Stretch.Uniform);
		Assert.Equal(0, x, 1e-9);
		Assert.Equal(25, y, 1e-9);
		Assert.Equal(200, w, 1e-9);
		Assert.Equal(150, h, 1e-9);
	}
}
