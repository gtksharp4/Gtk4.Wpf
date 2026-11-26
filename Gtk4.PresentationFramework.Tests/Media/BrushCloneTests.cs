using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>Clone() copies the CLR-held state of every brush type.</summary>
public class BrushCloneTests
{
	[Fact]
	public void LinearGradientBrush_clone_copies_stops_deeply_and_points()
	{
		var src = new LinearGradientBrush(Colors.Red, Colors.Blue, 90)
		{
			MappingMode = BrushMappingMode.Absolute, SpreadMethod = GradientSpreadMethod.Reflect,
		};
		var clone = (LinearGradientBrush)src.Clone();
		Assert.Equal(2, clone.GradientStops.Count);
		for (var i = 0; i < 2; i++)
		{
			Assert.Equal(src.GradientStops[i].Color, clone.GradientStops[i].Color);
			Assert.Equal(src.GradientStops[i].Offset, clone.GradientStops[i].Offset);
			Assert.NotSame(src.GradientStops[i], clone.GradientStops[i]);
		}
		Assert.Equal(src.StartPoint, clone.StartPoint);
		Assert.Equal(src.EndPoint, clone.EndPoint);
		Assert.Equal(BrushMappingMode.Absolute, clone.MappingMode);
		Assert.Equal(GradientSpreadMethod.Reflect, clone.SpreadMethod);
	}

	[Fact]
	public void RadialGradientBrush_clone_copies_geometry()
	{
		var src = new RadialGradientBrush(Colors.White, Colors.Black)
		{
			Center = new Point(0.2, 0.3), GradientOrigin = new Point(0.4, 0.1), RadiusX = 0.7, RadiusY = 0.6,
		};
		var clone = (RadialGradientBrush)src.Clone();
		Assert.Equal(2, clone.GradientStops.Count);
		Assert.Equal(new Point(0.2, 0.3), clone.Center);
		Assert.Equal(new Point(0.4, 0.1), clone.GradientOrigin);
		Assert.Equal(0.7, clone.RadiusX);
		Assert.Equal(0.6, clone.RadiusY);
	}

	[Fact]
	public void ImageBrush_clone_copies_source_and_tile_state()
	{
		var source = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[4], 4);
		var src = new ImageBrush(source)
		{
			Stretch = Stretch.Uniform, Viewport = new Rect(0, 0, 0.5, 0.5), TileMode = TileMode.Tile,
			ViewportUnits = BrushMappingMode.Absolute, Viewbox = new Rect(0, 0, 2, 2),
			ViewboxUnits = BrushMappingMode.Absolute, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top,
		};
		var clone = (ImageBrush)src.Clone();
		Assert.Same(source, clone.ImageSource);
		Assert.Equal(Stretch.Uniform, clone.Stretch);
		Assert.Equal(new Rect(0, 0, 0.5, 0.5), clone.Viewport);
		Assert.Equal(TileMode.Tile, clone.TileMode);
		Assert.Equal(BrushMappingMode.Absolute, clone.ViewportUnits);
		Assert.Equal(new Rect(0, 0, 2, 2), clone.Viewbox);
		Assert.Equal(BrushMappingMode.Absolute, clone.ViewboxUnits);
		Assert.Equal(AlignmentX.Left, clone.AlignmentX);
		Assert.Equal(AlignmentY.Top, clone.AlignmentY);
	}

	[Fact]
	public void VisualBrush_clone_keeps_the_visual_reference()
	{
		var visual = new DrawingVisual();
		var clone = (VisualBrush)new VisualBrush(visual) { Stretch = Stretch.None }.Clone();
		Assert.Same(visual, clone.Visual);
		Assert.Equal(Stretch.None, clone.Stretch);
	}
}
