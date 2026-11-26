using System;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Shapes;

/// <summary>
/// Pure (no GTK) tests for WPF→Cairo mapping helpers in <see cref="StrokeStyle"/>
/// and <see cref="DoubleCollection.Parse"/>.
/// </summary>
public class StrokeMappingTests
{
	// ── ToCairoDashes ─────────────────────────────────────────────────────────

	[Fact]
	public void ToCairoDashes_null_returns_empty()
	{
		Assert.Empty(StrokeStyle.ToCairoDashes(null, 3.0));
	}

	[Fact]
	public void ToCairoDashes_empty_collection_returns_empty()
	{
		Assert.Empty(StrokeStyle.ToCairoDashes(new DoubleCollection(), 3.0));
	}

	[Fact]
	public void ToCairoDashes_multiplies_by_thickness()
	{
		var dashes = new DoubleCollection { 2, 1 };
		var result = StrokeStyle.ToCairoDashes(dashes, 3.0);
		Assert.Equal(new double[] { 6.0, 3.0 }, result);
	}

	[Fact]
	public void ToCairoDashes_three_values()
	{
		var dashes = new DoubleCollection { 2, 1, 3 };
		var result = StrokeStyle.ToCairoDashes(dashes, 2.0);
		Assert.Equal(new double[] { 4.0, 2.0, 6.0 }, result);
	}

	[Fact]
	public void ToCairoDashes_thickness_zero_gives_zero_dashes()
	{
		var dashes = new DoubleCollection { 2, 1 };
		var result = StrokeStyle.ToCairoDashes(dashes, 0.0);
		Assert.Equal(new double[] { 0.0, 0.0 }, result);
	}

	// ── ToCairoLineCap ────────────────────────────────────────────────────────

	[Fact]
	public void ToCairoLineCap_Flat_maps_to_Butt()
	{
		Assert.Equal(Cairo.LineCap.Butt, StrokeStyle.ToCairoLineCap(PenLineCap.Flat));
	}

	[Fact]
	public void ToCairoLineCap_Square_maps_to_Square()
	{
		Assert.Equal(Cairo.LineCap.Square, StrokeStyle.ToCairoLineCap(PenLineCap.Square));
	}

	[Fact]
	public void ToCairoLineCap_Round_maps_to_Round()
	{
		Assert.Equal(Cairo.LineCap.Round, StrokeStyle.ToCairoLineCap(PenLineCap.Round));
	}

	[Fact]
	public void ToCairoLineCap_Triangle_falls_back_to_Butt()
	{
		// Cairo has no triangle cap; falls back to Butt.
		Assert.Equal(Cairo.LineCap.Butt, StrokeStyle.ToCairoLineCap(PenLineCap.Triangle));
	}

	// ── ToCairoLineJoin ───────────────────────────────────────────────────────

	[Fact]
	public void ToCairoLineJoin_Miter_maps_to_Miter()
	{
		Assert.Equal(Cairo.LineJoin.Miter, StrokeStyle.ToCairoLineJoin(PenLineJoin.Miter));
	}

	[Fact]
	public void ToCairoLineJoin_Bevel_maps_to_Bevel()
	{
		Assert.Equal(Cairo.LineJoin.Bevel, StrokeStyle.ToCairoLineJoin(PenLineJoin.Bevel));
	}

	[Fact]
	public void ToCairoLineJoin_Round_maps_to_Round()
	{
		Assert.Equal(Cairo.LineJoin.Round, StrokeStyle.ToCairoLineJoin(PenLineJoin.Round));
	}

	// ── DoubleCollection.Parse ────────────────────────────────────────────────

	[Fact]
	public void Parse_space_separated()
	{
		var col = DoubleCollection.Parse("2 1 3");
		Assert.Equal(3, col.Count);
		Assert.Equal(2.0, col[0]);
		Assert.Equal(1.0, col[1]);
		Assert.Equal(3.0, col[2]);
	}

	[Fact]
	public void Parse_comma_separated()
	{
		var col = DoubleCollection.Parse("2,1,3");
		Assert.Equal(3, col.Count);
		Assert.Equal(2.0, col[0]);
		Assert.Equal(1.0, col[1]);
		Assert.Equal(3.0, col[2]);
	}

	[Fact]
	public void Parse_empty_returns_empty()
	{
		Assert.Empty(DoubleCollection.Parse("   "));
	}

	[Fact]
	public void Parse_single_value()
	{
		var col = DoubleCollection.Parse("5");
		Assert.Single(col);
		Assert.Equal(5.0, col[0]);
	}

	[Fact]
	public void Parse_fractional_values()
	{
		var col = DoubleCollection.Parse("1.5 0.5");
		Assert.Equal(1.5, col[0]);
		Assert.Equal(0.5, col[1]);
	}

	[Fact]
	public void Parse_null_throws_ArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => DoubleCollection.Parse(null!));
	}
}
