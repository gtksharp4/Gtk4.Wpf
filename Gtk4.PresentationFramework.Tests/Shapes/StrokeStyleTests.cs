using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;
using WpfRectangle = System.Windows.Shapes.Rectangle;

namespace Gtk4.PresentationFramework.Tests.Shapes;

/// <summary>
/// Pure (no GTK) tests for stroke-style DP defaults and round-trips.
/// </summary>
public class StrokeStyleDpTests
{
	[Fact]
	public void StrokeDashArray_default_is_null()
	{
		var r = new WpfRectangle();
		Assert.Null(r.StrokeDashArray);
	}

	[Fact]
	public void StrokeDashOffset_default_is_zero()
	{
		var r = new WpfRectangle();
		Assert.Equal(0.0, r.StrokeDashOffset);
	}

	[Fact]
	public void StrokeMiterLimit_default_is_ten()
	{
		var r = new WpfRectangle();
		Assert.Equal(10.0, r.StrokeMiterLimit);
	}

	[Fact]
	public void StrokeStartLineCap_default_is_Flat()
	{
		var r = new WpfRectangle();
		Assert.Equal(PenLineCap.Flat, r.StrokeStartLineCap);
	}

	[Fact]
	public void StrokeEndLineCap_default_is_Flat()
	{
		var r = new WpfRectangle();
		Assert.Equal(PenLineCap.Flat, r.StrokeEndLineCap);
	}

	[Fact]
	public void StrokeDashCap_default_is_Flat()
	{
		var r = new WpfRectangle();
		Assert.Equal(PenDashCap.Flat, r.StrokeDashCap);
	}

	[Fact]
	public void StrokeLineJoin_default_is_Miter()
	{
		var r = new WpfRectangle();
		Assert.Equal(PenLineJoin.Miter, r.StrokeLineJoin);
	}

	[Fact]
	public void StrokeDashArray_roundtrip()
	{
		var r = new WpfRectangle();
		var col = new DoubleCollection { 2, 1 };
		r.StrokeDashArray = col;
		Assert.Same(col, r.StrokeDashArray);
	}

	[Fact]
	public void StrokeDashOffset_roundtrip()
	{
		var r = new WpfRectangle();
		r.StrokeDashOffset = 1.5;
		Assert.Equal(1.5, r.StrokeDashOffset);
	}

	[Fact]
	public void StrokeMiterLimit_roundtrip()
	{
		var r = new WpfRectangle();
		r.StrokeMiterLimit = 4.0;
		Assert.Equal(4.0, r.StrokeMiterLimit);
	}

	[Fact]
	public void StrokeStartLineCap_roundtrip()
	{
		var r = new WpfRectangle();
		r.StrokeStartLineCap = PenLineCap.Round;
		Assert.Equal(PenLineCap.Round, r.StrokeStartLineCap);
	}

	[Fact]
	public void StrokeEndLineCap_roundtrip()
	{
		var r = new WpfRectangle();
		r.StrokeEndLineCap = PenLineCap.Square;
		Assert.Equal(PenLineCap.Square, r.StrokeEndLineCap);
	}

	[Fact]
	public void StrokeLineJoin_roundtrip()
	{
		var r = new WpfRectangle();
		r.StrokeLineJoin = PenLineJoin.Round;
		Assert.Equal(PenLineJoin.Round, r.StrokeLineJoin);
	}

	[Fact]
	public void Setting_all_stroke_props_does_not_throw_without_surface()
	{
		var ex = Record.Exception(() =>
		{
			var r = new WpfRectangle
			{
				Stroke = Brushes.Black,
				StrokeThickness = 2,
				StrokeDashArray = new DoubleCollection { 4, 2 },
				StrokeDashOffset = 1,
				StrokeDashCap = PenDashCap.Round,
				StrokeStartLineCap = PenLineCap.Round,
				StrokeEndLineCap = PenLineCap.Square,
				StrokeLineJoin = PenLineJoin.Bevel,
				StrokeMiterLimit = 5,
			};
			_ = r.StrokeDashArray;
			_ = r.StrokeMiterLimit;
		});
		Assert.Null(ex);
	}
}
