using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class StoryboardUnanimatableTargetTests : WpfGtkTestBase
{
	[Fact]
	public void GradientStopColorPath_IsSkipped_AndSiblingsStillAnimate() => Run(() =>
	{
		var rect = new Rectangle { Name = "r", Opacity = 1 };
		rect.Fill = new LinearGradientBrush();
		((LinearGradientBrush)rect.Fill).GradientStops.Add(new GradientStop(Colors.Red, 0));
		((LinearGradientBrush)rect.Fill).GradientStops.Add(new GradientStop(Colors.Blue, 1));
		var color = new ColorAnimation { To = Colors.Green, Duration = TimeSpan.FromSeconds(1) };
		Storyboard.SetTargetName(color, "r");
		Storyboard.SetTargetProperty(color, new PropertyPath("(Rectangle.Fill).(LinearGradientBrush.GradientStops)[1].(GradientStop.Color)"));
		var fade = new DoubleAnimation { To = 0, Duration = TimeSpan.FromSeconds(1) };
		Storyboard.SetTargetName(fade, "r");
		Storyboard.SetTargetProperty(fade, new PropertyPath("(UIElement.Opacity)"));
		var sb = new Storyboard();
		sb.Children.Add(color);
		sb.Children.Add(fade);
		var host = new System.Windows.Controls.Canvas();
		host.Children.Add(rect);
		host.RegisterName("r", rect);

		sb.Begin(host);

		Assert.Equal(1, sb.ActiveClocks.Count);
		sb.ActiveClocks[0].Tick(TimeSpan.FromSeconds(0.5));
		Assert.Equal(0.5, rect.Opacity, precision: 6);
	});

	class Poco
	{
		public Poco? Inner { get; set; }
	}

	[Fact]
	public void PathThroughNonDependencyObject_IsRejected() => Run(() =>
	{
		var border = new System.Windows.Controls.Border { Name = "b", DataContext = new Poco { Inner = new Poco() } };
		var fade = new DoubleAnimation { To = 0, Duration = TimeSpan.FromSeconds(1) };
		Storyboard.SetTargetName(fade, "b");
		Storyboard.SetTargetProperty(fade, new PropertyPath("(FrameworkElement.DataContext).(UIElement.Opacity)"));
		var sb = new Storyboard();
		sb.Children.Add(fade);
		var host = new System.Windows.Controls.Canvas();
		host.Children.Add(border);
		host.RegisterName("b", border);

		Assert.Throws<InvalidOperationException>(() => sb.Begin(host));
	});

	[Fact]
	public void PathFigureStartPointPath_IsSkipped_AndSiblingsStillAnimate() => Run(() =>
	{
		var figure = new PathFigure { StartPoint = new System.Windows.Point(1, 1) };
		var geometry = new PathGeometry();
		geometry.Figures.Add(figure);
		var path = new Path { Name = "p", Opacity = 1, Data = geometry };
		var move = new PointAnimation { To = new System.Windows.Point(5, 5), Duration = TimeSpan.FromSeconds(1) };
		Storyboard.SetTargetName(move, "p");
		Storyboard.SetTargetProperty(move,
			new PropertyPath("(Path.Data).(PathGeometry.Figures)[0].(PathFigure.StartPoint)"));
		var fade = new DoubleAnimation { To = 0, Duration = TimeSpan.FromSeconds(1) };
		Storyboard.SetTargetName(fade, "p");
		Storyboard.SetTargetProperty(fade, new PropertyPath("(UIElement.Opacity)"));
		var sb = new Storyboard();
		sb.Children.Add(move);
		sb.Children.Add(fade);
		var host = new System.Windows.Controls.Canvas();
		host.Children.Add(path);
		host.RegisterName("p", path);

		sb.Begin(host);

		Assert.Equal(1, sb.ActiveClocks.Count);
	});
}
