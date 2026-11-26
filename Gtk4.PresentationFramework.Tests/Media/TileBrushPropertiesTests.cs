using System;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>TileBrush and VisualBrush members are dependency properties with the WPF metadata.</summary>
public class TileBrushPropertiesTests
{
	[Fact]
	public void Defaults_match_WPF()
	{
		var b = new VisualBrush();
		Assert.Equal(Stretch.Fill, b.Stretch);
		Assert.Equal(TileMode.None, b.TileMode);
		Assert.Equal(new Rect(0, 0, 1, 1), b.Viewport);
		Assert.Equal(BrushMappingMode.RelativeToBoundingBox, b.ViewportUnits);
		Assert.Equal(new Rect(0, 0, 1, 1), b.Viewbox);
		Assert.Equal(BrushMappingMode.RelativeToBoundingBox, b.ViewboxUnits);
		Assert.Equal(AlignmentX.Center, b.AlignmentX);
		Assert.Equal(AlignmentY.Center, b.AlignmentY);
		Assert.Null(b.Visual);
		Assert.True(b.AutoLayoutContent);
	}

	[Fact]
	public void Every_member_is_registered_as_a_dependency_property()
	{
		Assert.Equal("Stretch", TileBrush.StretchProperty.Name);
		Assert.Equal(typeof(TileBrush), TileBrush.TileModeProperty.OwnerType);
		Assert.Equal(typeof(Rect), TileBrush.ViewportProperty.PropertyType);
		Assert.Equal(typeof(Rect), TileBrush.ViewboxProperty.PropertyType);
		Assert.Equal(typeof(BrushMappingMode), TileBrush.ViewportUnitsProperty.PropertyType);
		Assert.Equal(typeof(BrushMappingMode), TileBrush.ViewboxUnitsProperty.PropertyType);
		Assert.Equal(typeof(AlignmentX), TileBrush.AlignmentXProperty.PropertyType);
		Assert.Equal(typeof(AlignmentY), TileBrush.AlignmentYProperty.PropertyType);
		Assert.Equal(typeof(Visual), VisualBrush.VisualProperty.PropertyType);
		Assert.Equal(typeof(bool), VisualBrush.AutoLayoutContentProperty.PropertyType);
	}

	[Fact]
	public void Undefined_enum_values_are_rejected()
	{
		var b = new DrawingBrush();
		Assert.Throws<ArgumentException>(() => b.Stretch = (Stretch)99);
		Assert.Throws<ArgumentException>(() => b.TileMode = (TileMode)99);
		Assert.Throws<ArgumentException>(() => b.ViewportUnits = (BrushMappingMode)99);
		Assert.Throws<ArgumentException>(() => b.ViewboxUnits = (BrushMappingMode)99);
		Assert.Throws<ArgumentException>(() => b.AlignmentX = (AlignmentX)99);
		Assert.Throws<ArgumentException>(() => b.AlignmentY = (AlignmentY)99);
	}

	[Fact]
	public void Each_property_raises_Changed()
	{
		var b = new VisualBrush();
		int n = 0;
		b.Changed += (s, e) => n++;
		b.Stretch = Stretch.Uniform;
		b.TileMode = TileMode.Tile;
		b.Viewport = new Rect(0, 0, 2, 2);
		b.ViewportUnits = BrushMappingMode.Absolute;
		b.Viewbox = new Rect(0, 0, 3, 3);
		b.ViewboxUnits = BrushMappingMode.Absolute;
		b.AlignmentX = AlignmentX.Left;
		b.AlignmentY = AlignmentY.Top;
		b.AutoLayoutContent = false;
		b.Visual = new DrawingVisual();
		Assert.Equal(10, n);
	}

	[Fact]
	public void Clone_copies_every_property()
	{
		var visual = new DrawingVisual();
		var src = new VisualBrush(visual)
		{
			Stretch = Stretch.UniformToFill, TileMode = TileMode.FlipXY, Viewport = new Rect(1, 2, 3, 4),
			ViewportUnits = BrushMappingMode.Absolute, Viewbox = new Rect(5, 6, 7, 8),
			ViewboxUnits = BrushMappingMode.Absolute, AlignmentX = AlignmentX.Right, AlignmentY = AlignmentY.Bottom,
			AutoLayoutContent = false, Opacity = 0.5,
		};
		var c = (VisualBrush)src.Clone();
		Assert.Same(visual, c.Visual);
		Assert.Equal(Stretch.UniformToFill, c.Stretch);
		Assert.Equal(TileMode.FlipXY, c.TileMode);
		Assert.Equal(new Rect(1, 2, 3, 4), c.Viewport);
		Assert.Equal(BrushMappingMode.Absolute, c.ViewportUnits);
		Assert.Equal(new Rect(5, 6, 7, 8), c.Viewbox);
		Assert.Equal(BrushMappingMode.Absolute, c.ViewboxUnits);
		Assert.Equal(AlignmentX.Right, c.AlignmentX);
		Assert.Equal(AlignmentY.Bottom, c.AlignmentY);
		Assert.False(c.AutoLayoutContent);
		Assert.Equal(0.5, c.Opacity);
	}
}
