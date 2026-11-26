using System.Windows;
using System.Windows.Media;
using Xunit;
using FillRule = System.Windows.Shapes.FillRule;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>The outermost GeometryGroup's FillRule decides, as with WPF's flattened combined shape.</summary>
public class GeometryGroupFillRuleTests
{
	static GeometryGroup Nested(FillRule outer, FillRule inner)
	{
		var group = new GeometryGroup { FillRule = inner };
		group.Children.Add(new RectangleGeometry(new Rect(0, 0, 60, 60)));
		group.Children.Add(new RectangleGeometry(new Rect(30, 30, 60, 60)));
		var root = new GeometryGroup { FillRule = outer };
		root.Children.Add(group);
		return root;
	}

	[Fact]
	public void The_outer_group_rule_survives_a_nested_group()
	{
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using var cr = new Cairo.Context(surface);

		Nested(FillRule.Nonzero, FillRule.EvenOdd).BuildPath(cr);

		Assert.Equal(Cairo.FillRule.Winding, cr.FillRule);
	}

	[Fact]
	public void Overlapping_rectangles_of_a_Nonzero_outer_group_are_filled_in_the_overlap()
	{
		Assert.NotEqual(0, AlphaAt(Nested(FillRule.Nonzero, FillRule.EvenOdd), 45, 45));
	}

	[Fact]
	public void Overlapping_rectangles_of_an_EvenOdd_outer_group_leave_the_overlap_empty()
	{
		Assert.Equal(0, AlphaAt(Nested(FillRule.EvenOdd, FillRule.Nonzero), 45, 45));
		Assert.NotEqual(0, AlphaAt(Nested(FillRule.EvenOdd, FillRule.Nonzero), 10, 10));
	}

	static int AlphaAt(Geometry geometry, int x, int y)
	{
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using (var cr = new Cairo.Context(surface))
		{
			geometry.BuildPath(cr);
			cr.SetSourceRGBA(1, 0, 0, 1);
			cr.Fill();
		}
		surface.Flush();
		return surface.Data[y * surface.Stride + x * 4 + 3];
	}
}
