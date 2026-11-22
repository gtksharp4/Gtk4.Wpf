using System.Windows;
using System.Windows.Media;
using Xunit;
using FillRule = System.Windows.Shapes.FillRule;

namespace Gtk4.Forms.PresentationFramework.Tests.Media;

/// <summary>PathGeometry.FillRule is a dependency property, as in WPF.</summary>
public class PathGeometryFillRulePropertyTests
{
	[Fact]
	public void The_property_exists_with_an_EvenOdd_default()
	{
		Assert.NotNull(PathGeometry.FillRuleProperty);
		Assert.Equal(FillRule.EvenOdd, PathGeometry.FillRuleProperty.DefaultMetadata.DefaultValue);
		Assert.Equal(FillRule.EvenOdd, new PathGeometry().FillRule);
	}

	[Fact]
	public void SetValue_is_reflected_by_the_CLR_property()
	{
		var pg = new PathGeometry();
		pg.SetValue(PathGeometry.FillRuleProperty, FillRule.Nonzero);
		Assert.Equal(FillRule.Nonzero, pg.FillRule);
	}

	[Fact]
	public void ClearValue_returns_to_the_default()
	{
		var pg = new PathGeometry { FillRule = FillRule.Nonzero };
		pg.ClearValue(PathGeometry.FillRuleProperty);
		Assert.Equal(FillRule.EvenOdd, pg.FillRule);
	}

	[Fact]
	public void Clone_keeps_Nonzero()
	{
		var pg = new PathGeometry { FillRule = FillRule.Nonzero };
		Assert.Equal(FillRule.Nonzero, ((PathGeometry)pg.Clone()).FillRule);
		Assert.Equal(FillRule.Nonzero, ((PathGeometry)pg.CloneCurrentValue()).FillRule);
	}

	[Fact]
	public void Parse_F1_gives_Nonzero_and_F0_EvenOdd()
	{
		Assert.Equal(FillRule.Nonzero, ((PathGeometry)Geometry.Parse("F1 M0,0 L10,0 L10,10Z")).FillRule);
		Assert.Equal(FillRule.EvenOdd, ((PathGeometry)Geometry.Parse("F0 M0,0 L10,0 L10,10Z")).FillRule);
	}
}
