using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Media;

/// <summary>The pure WPF TileBrush content-to-tile mapping.</summary>
public class TileBrushMappingTests
{
	static readonly Rect Content = new Rect(0, 0, 40, 20);
	static readonly Rect Box = new Rect(0, 0, 200, 100);

	static TileMapping Map(DrawingBrush b, Rect? content = null, Rect? box = null) =>
		TileMapping.Compute(content ?? Content, box ?? Box, b);

	[Fact]
	public void Defaults_map_the_content_onto_the_whole_fill_area()
	{
		var m = Map(new DrawingBrush());
		Assert.True(m.IsValid);
		Assert.Equal(Box, m.Viewport);
		Assert.Equal(5, m.ScaleX);
		Assert.Equal(5, m.ScaleY);
		Assert.Equal(new Point(200, 100), m.Map(new Point(40, 20)));
	}

	[Fact]
	public void Relative_viewport_is_a_fraction_of_the_bounding_box()
	{
		var m = Map(new DrawingBrush { Viewport = new Rect(0.5, 0.25, 0.25, 0.5) });
		Assert.Equal(new Rect(100, 25, 50, 50), m.Viewport);
	}

	[Fact]
	public void Absolute_viewport_ignores_the_bounding_box()
	{
		var m = Map(new DrawingBrush
		{
			Viewport = new Rect(3, 4, 20, 10), ViewportUnits = BrushMappingMode.Absolute,
		});
		Assert.Equal(new Rect(3, 4, 20, 10), m.Viewport);
	}

	[Fact]
	public void Relative_viewbox_is_a_fraction_of_the_content_bounds()
	{
		var m = Map(new DrawingBrush { Viewbox = new Rect(0.5, 0, 0.5, 1) }, new Rect(10, 0, 40, 20));
		Assert.Equal(new Rect(30, 0, 20, 20), m.Viewbox);
		Assert.Equal(10, m.ScaleX);
		Assert.Equal(5, m.ScaleY);
	}

	[Fact]
	public void Absolute_viewbox_is_taken_as_is()
	{
		var m = Map(new DrawingBrush
		{
			Viewbox = new Rect(5, 5, 20, 20), ViewboxUnits = BrushMappingMode.Absolute,
		});
		Assert.Equal(new Rect(5, 5, 20, 20), m.Viewbox);
		Assert.Equal(new Point(0, 0), m.Map(new Point(5, 5)));
	}

	[Fact]
	public void Stretch_None_keeps_scale_one()
	{
		var m = Map(new DrawingBrush { Stretch = Stretch.None });
		Assert.Equal(1, m.ScaleX);
		Assert.Equal(1, m.ScaleY);
	}

	[Fact]
	public void Stretch_Uniform_fits_and_UniformToFill_covers()
	{
		var wide = new Rect(0, 0, 100, 50);
		var square = new Rect(0, 0, 100, 100);
		var fit = Map(new DrawingBrush { Stretch = Stretch.Uniform }, wide, square);
		Assert.Equal(1, fit.ScaleX);
		Assert.Equal(1, fit.ScaleY);
		var cover = Map(new DrawingBrush { Stretch = Stretch.UniformToFill }, wide, square);
		Assert.Equal(2, cover.ScaleX);
		Assert.Equal(2, cover.ScaleY);
	}

	[Theory]
	[InlineData(AlignmentX.Left, 0)]
	[InlineData(AlignmentX.Center, 25)]
	[InlineData(AlignmentX.Right, 50)]
	public void AlignmentX_places_a_narrow_uniform_content(AlignmentX alignment, double expected)
	{
		var m = Map(new DrawingBrush { Stretch = Stretch.Uniform, AlignmentX = alignment },
			new Rect(0, 0, 50, 100), new Rect(0, 0, 100, 100));
		Assert.Equal(expected, m.OffsetX);
		Assert.Equal(0, m.OffsetY);
	}

	[Theory]
	[InlineData(AlignmentY.Top, 0)]
	[InlineData(AlignmentY.Center, 25)]
	[InlineData(AlignmentY.Bottom, 50)]
	public void AlignmentY_places_a_short_uniform_content(AlignmentY alignment, double expected)
	{
		var m = Map(new DrawingBrush { Stretch = Stretch.Uniform, AlignmentY = alignment },
			new Rect(0, 0, 100, 50), new Rect(0, 0, 100, 100));
		Assert.Equal(expected, m.OffsetY);
	}

	[Fact]
	public void Stretch_None_content_is_aligned_inside_the_viewport()
	{
		var b = new DrawingBrush
		{
			Stretch = Stretch.None, AlignmentX = AlignmentX.Right, AlignmentY = AlignmentY.Bottom,
		};
		var m = Map(b, new Rect(0, 0, 10, 10), new Rect(0, 0, 100, 100));
		Assert.Equal(new Point(100, 100), m.Map(new Point(10, 10)));
	}

	[Fact]
	public void Absolute_viewport_shifts_where_the_content_lands()
	{
		var m = Map(new DrawingBrush
		{
			Viewport = new Rect(10, 20, 40, 20), ViewportUnits = BrushMappingMode.Absolute,
		});
		Assert.Equal(new Point(10, 20), m.Map(new Point(0, 0)));
		Assert.Equal(new Point(50, 40), m.Map(new Point(40, 20)));
	}

	[Theory]
	[InlineData(0, 10, 10, 10)]
	[InlineData(10, 0, 10, 10)]
	[InlineData(10, 10, 0, 10)]
	[InlineData(10, 10, 10, 0)]
	public void Degenerate_input_gives_an_invalid_mapping(double cw, double ch, double bw, double bh)
	{
		var m = Map(new DrawingBrush(), new Rect(0, 0, cw, ch), new Rect(0, 0, bw, bh));
		Assert.False(m.IsValid);
	}

	[Fact]
	public void An_empty_viewbox_or_viewport_gives_an_invalid_mapping()
	{
		Assert.False(Map(new DrawingBrush { Viewbox = new Rect(0, 0, 0, 0) }).IsValid);
		Assert.False(Map(new DrawingBrush { Viewport = new Rect(0, 0, 0, 1) }).IsValid);
		Assert.False(Map(new DrawingBrush { Viewbox = Rect.Empty }).IsValid);
	}
}
