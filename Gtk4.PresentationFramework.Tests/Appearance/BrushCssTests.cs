using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Appearance;

public class BrushCssTests
{
	[Fact]
	public void Solid_brush_is_rgba_background()
	{
		Assert.Equal("background-color: rgba(255,0,0,1);",
			BrushCss.Background(new SolidColorBrush(Colors.Red)));
	}

	[Fact]
	public void Opacity_multiplies_into_alpha()
	{
		var b = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)) { Opacity = 0.5 };
		Assert.Contains("rgba(0,0,255,0.25", BrushCss.Background(b));
	}

	[Fact]
	public void Foreground_and_border_color_use_color_property()
	{
		var b = new SolidColorBrush(Colors.Blue);
		Assert.Equal("color: rgba(0,0,255,1);", BrushCss.Foreground(b));
		Assert.Equal("border-color: rgba(0,0,255,1);", BrushCss.BorderColor(b));
	}

	[Theory]
	[InlineData(1, 0, 90.0)]
	[InlineData(0, 1, 180.0)]
	[InlineData(1, 1, 135.0)]
	[InlineData(-1, 0, 270.0)]
	[InlineData(0, -1, 0.0)]
	public void Linear_gradient_angle(double ex, double ey, double expected)
	{
		var g = new LinearGradientBrush(Colors.Red, Colors.Blue)
			{ StartPoint = new Point(0, 0), EndPoint = new Point(ex, ey) };
		Assert.Equal(expected, BrushCss.LinearAngle(g), 3);
	}

	[Fact]
	public void Linear_gradient_stops_are_percentages_in_offset_order()
	{
		var g = new LinearGradientBrush(
			new GradientStopCollection
			{
				new GradientStop(Colors.Blue, 1), new GradientStop(Colors.Red, 0),
				new GradientStop(Colors.Lime, 0.5),
			},
			new Point(0, 0), new Point(1, 0));
		Assert.Equal(
			"background-image: linear-gradient(90deg, rgba(255,0,0,1) 0%, rgba(0,255,0,1) 50%, " +
			"rgba(0,0,255,1) 100%);",
			BrushCss.Background(g));
	}

	[Fact]
	public void Radial_gradient_has_center_and_radii()
	{
		var g = new RadialGradientBrush(Colors.White, Colors.Black)
			{ Center = new Point(0.25, 0.75), RadiusX = 0.4, RadiusY = 0.6 };
		Assert.StartsWith("background-image: radial-gradient(ellipse 40% 60% at 25% 75%, ",
			BrushCss.Background(g));
	}

	[Fact]
	public void Image_brush_null_and_empty_gradient_produce_no_css()
	{
		Assert.Equal(string.Empty, BrushCss.Background(null));
		Assert.Equal(string.Empty, BrushCss.Background(new ImageBrush()));
		Assert.Equal(string.Empty, BrushCss.Background(new LinearGradientBrush()));
		Assert.Equal(string.Empty, BrushCss.Foreground(null));
		Assert.Equal(string.Empty, BrushCss.BorderColor(new ImageBrush()));
	}

	[Fact]
	public void Gradient_foreground_falls_back_to_first_stop()
	{
		var g = new LinearGradientBrush(Colors.Red, Colors.Blue);
		Assert.Equal("color: rgba(255,0,0,1);", BrushCss.Foreground(g));
	}

	[Fact]
	public void Border_width_and_radius_use_css_corner_order()
	{
		Assert.Equal("border-width: 1px 2px 3px 4px;", BrushCss.BorderWidth(new Thickness(4, 1, 2, 3)));
		Assert.Equal("border-radius: 1px 2px 3px 4px;", BrushCss.BorderRadius(new CornerRadius(1, 2, 3, 4)));
	}

	[Fact]
	public void Partial_linear_span_keeps_its_extent()
	{
		var g = new LinearGradientBrush(Colors.Red, Colors.Blue)
			{ StartPoint = new Point(0, 0), EndPoint = new Point(0, 0.5) };
		string css = BrushCss.Background(g);
		Assert.Contains("180deg", css);
		Assert.Contains("0%", css);
		Assert.Contains("50%", css);
		Assert.DoesNotContain("100%", css);
	}
}
