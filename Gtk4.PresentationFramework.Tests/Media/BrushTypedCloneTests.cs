using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

public class BrushTypedCloneTests
{
	[Fact]
	public void SolidColorBrush_Clone_returns_the_typed_brush_with_the_same_color()
	{
		var source = new SolidColorBrush(Colors.Red);
		SolidColorBrush clone = source.Clone();
		Assert.Equal(Colors.Red, clone.Color);
		Assert.NotSame(source, clone);
	}

	[Fact]
	public void Clone_through_a_base_reference_still_yields_the_typed_brush()
	{
		Brush source = new SolidColorBrush(Colors.Blue);
		Assert.Equal(Colors.Blue, ((SolidColorBrush)source.Clone()).Color);
		Freezable viaFreezable = source;
		Assert.IsType<SolidColorBrush>(viaFreezable.Clone());
	}

	[Fact]
	public void Clone_of_a_frozen_brush_is_not_frozen()
	{
		var source = new SolidColorBrush(Colors.Green);
		source.Freeze();
		SolidColorBrush clone = source.Clone();
		Assert.False(clone.IsFrozen);
		clone.Color = Colors.Black;
		Assert.Equal(Colors.Green, source.Color);
	}

	[Fact]
	public void CloneCurrentValue_is_typed_and_unfrozen()
	{
		var source = new SolidColorBrush(Colors.Orange);
		source.Freeze();
		SolidColorBrush clone = source.CloneCurrentValue();
		Assert.Equal(Colors.Orange, clone.Color);
		Assert.False(clone.IsFrozen);
	}

	[Fact]
	public void Every_brush_type_returns_its_own_type_from_Clone_and_CloneCurrentValue()
	{
		LinearGradientBrush linear = new LinearGradientBrush(Colors.Red, Colors.Blue, 0).Clone();
		RadialGradientBrush radial = new RadialGradientBrush(Colors.Red, Colors.Blue).CloneCurrentValue();
		GradientBrush gradient = ((GradientBrush)linear).Clone();
		ImageBrush image = new ImageBrush().Clone();
		DrawingBrush drawing = new DrawingBrush().CloneCurrentValue();
		VisualBrush visual = new VisualBrush().Clone();
		TileBrush tile = ((TileBrush)image).CloneCurrentValue();
		Brush brush = new SolidColorBrush(Colors.Red).CloneCurrentValue();
		Assert.NotNull(linear);
		Assert.NotNull(radial);
		Assert.NotNull(gradient);
		Assert.NotNull(drawing);
		Assert.NotNull(visual);
		Assert.NotNull(tile);
		Assert.NotNull(brush);
	}
}
