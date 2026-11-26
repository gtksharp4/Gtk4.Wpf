using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>WPF defaults of the store-only brush and geometry members.</summary>
public class BrushMembersTests : WpfGtkTestBase
{
	[Fact]
	public void GradientBrush_MappingMode_and_SpreadMethod_default_like_WPF() => Run(() =>
	{
		var b = new LinearGradientBrush();
		Assert.Equal(BrushMappingMode.RelativeToBoundingBox, b.MappingMode);
		Assert.Equal(GradientSpreadMethod.Pad, b.SpreadMethod);
		Assert.Equal(BrushMappingMode.RelativeToBoundingBox, new RadialGradientBrush().MappingMode);
	});

	[Fact]
	public void GradientSpreadMethod_has_the_WPF_values() => Run(() =>
	{
		Assert.Equal(new[] { "Pad", "Reflect", "Repeat" }, System.Enum.GetNames(typeof(GradientSpreadMethod)));
		var b = new LinearGradientBrush
		{
			MappingMode = BrushMappingMode.Absolute, SpreadMethod = GradientSpreadMethod.Reflect,
		};
		Assert.Equal(BrushMappingMode.Absolute, b.MappingMode);
		Assert.Equal(GradientSpreadMethod.Reflect, b.SpreadMethod);
	});

	[Fact]
	public void TileBrush_Viewbox_defaults_like_WPF() => Run(() =>
	{
		var b = new DrawingBrush();
		Assert.Equal(new Rect(0, 0, 1, 1), b.Viewbox);
		Assert.Equal(BrushMappingMode.RelativeToBoundingBox, b.ViewboxUnits);
	});

	[Fact]
	public void GeometryGroup_defaults_and_children() => Run(() =>
	{
		var g = new GeometryGroup();
		Assert.Equal(FillRule.EvenOdd, g.FillRule);
		Assert.Empty(g.Children);
		g.Children.Add(new EllipseGeometry(new Point(5, 5), 5, 5));
		g.Children.Add(new RectangleGeometry(new Rect(20, 0, 10, 10)));
		Assert.Equal(new Rect(0, 0, 30, 10), g.Bounds);
	});

	[Fact]
	public void PathFigureCollection_Parse_and_DrawingImage() => Run(() =>
	{
		var figures = PathFigureCollection.Parse("M0,0 L10,10");
		Assert.Single(figures);
		Assert.Single(new PathGeometry { Figures = figures }.Figures);
		var d = new GeometryDrawing();
		Assert.Same(d, new DrawingImage(d).Drawing);
		Assert.Null(new DrawingImage().Drawing);
	});
}
