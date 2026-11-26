using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Animation.AnimationTestHelpers;

namespace Gtk4.PresentationFramework.Tests.Animation;

public class StoryboardPathResolverTests
{
	static Storyboard Run(AnimTarget el, string path, Timeline anim)
	{
		var sb = new Storyboard();
		Storyboard.SetTargetProperty(anim, new PropertyPath(path));
		sb.Children.Add(anim);
		sb.Begin(el, true);
		return sb;
	}

	[Fact]
	public void Animates_a_translate_transform_inside_RenderTransform()
	{
		var el = new AnimTarget();
		var t = new TranslateTransform();
		el.RenderTransform = t;
		Run(el, "(UIElement.RenderTransform).(TranslateTransform.X)", Linear(0, 80));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(0.5));
		Assert.Equal(40.0, t.X, 6);
		Assert.Same(t, el.RenderTransform);
	}

	[Fact]
	public void Animates_a_solid_color_brush_inside_Background()
	{
		var el = new AnimTarget();
		var brush = new SolidColorBrush(Colors.Black);
		el.SetValue(AnimTarget.FillProperty, brush);
		Run(el, "(AnimTarget.FillX).(SolidColorBrush.Color)",
			new ColorAnimation(Colors.Black, Colors.White, TimeSpan.FromSeconds(1)));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(0.5));
		Assert.Equal(Color.FromArgb(255, 128, 128, 128), brush.Color);
	}

	[Fact]
	public void Clones_a_frozen_intermediate_value_and_sets_the_clone_back()
	{
		var el = new AnimTarget();
		var frozen = new TranslateTransform(5, 0);
		frozen.Freeze();
		el.RenderTransform = frozen;
		Run(el, "(UIElement.RenderTransform).(TranslateTransform.X)", Linear(0, 80));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(0.5));
		var current = (TranslateTransform)el.RenderTransform!;
		Assert.NotSame(frozen, current);
		Assert.False(current.IsFrozen);
		Assert.Equal(40.0, current.X, 6);
		Assert.Equal(5.0, frozen.X);
	}

	[Fact]
	public void Reads_an_indexed_step_from_a_transform_group()
	{
		var el = new AnimTarget();
		var group = new TransformGroup();
		group.Children.Add(new ScaleTransform());
		var move = new TranslateTransform();
		group.Children.Add(move);
		el.RenderTransform = group;
		Run(el, "(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)", Linear(0, 10));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(1));
		Assert.Equal(10.0, move.X, 6);
	}

	[Fact]
	public void Unqualified_and_dotted_names_resolve_on_the_target()
	{
		var el = new AnimTarget();
		Run(el, "Opacity", Linear(0, 0.5));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(1));
		Assert.Equal(0.5, el.Opacity, 6);
	}

	[Theory]
	[InlineData("(UIElement.RenderTransform).(TranslateTransform.X)")]
	[InlineData("(UIElement.Nope)")]
	[InlineData("(UIElement.RenderTransform)[3].(TranslateTransform.X)")]
	[InlineData("")]
	public void Unresolvable_steps_throw_naming_the_path(string path)
	{
		var el = new AnimTarget();
		var sb = new Storyboard();
		var anim = Linear(0, 1);
		Storyboard.SetTargetProperty(anim, new PropertyPath(path));
		sb.Children.Add(anim);
		var ex = Assert.Throws<InvalidOperationException>(() => sb.Begin(el));
		Assert.Contains(path, ex.Message);
	}
	[Fact]
	public void Animates_geometry_and_drawing_sub_objects()
	{
		var el = new AnimTarget();
		var ellipse = new EllipseGeometry(new Point(100, 75), 100, 75);
		el.Clip = ellipse;
		Run(el, "(UIElement.Clip).(EllipseGeometry.RadiusX)", Linear(0, 150));
		Run(el, "(UIElement.Clip).(EllipseGeometry.Center)",
			new PointAnimation(new Point(0, 0), new Point(200, 150), TimeSpan.FromSeconds(1)));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(0.5));
		Assert.Equal(75.0, ellipse.RadiusX, 6);
		Assert.Equal(new Point(100, 75), ellipse.Center);
		Assert.Same(ellipse, el.Clip);

		var inner = new EllipseGeometry(new Point(0.5, 0.5), 0.45, 0.2);
		var group = new DrawingGroup();
		group.Children.Add(new GeometryDrawing(Brushes.Red, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
		group.Children.Add(new GeometryDrawing(null, null, inner));
		var brush = new DrawingBrush(group);
		el.SetValue(AnimTarget.FillProperty, brush);
		Run(el, "(AnimTarget.FillX).(DrawingBrush.Drawing).(DrawingGroup.Children)[1]"
			+ ".(GeometryDrawing.Geometry).(EllipseGeometry.RadiusX)", Linear(0.01, 0.5));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(1));
		Assert.Equal(0.5, inner.RadiusX, 6);
	}

	[Fact]
	public void Cloning_a_frozen_drawing_chain_leaves_the_frozen_original_untouched()
	{
		var el = new AnimTarget();
		var inner = new EllipseGeometry(new Point(0.5, 0.5), 0.45, 0.2);
		var brush = new DrawingBrush(new GeometryDrawing(null, null, inner));
		brush.Freeze();
		el.SetValue(AnimTarget.FillProperty, brush);
		Run(el, "(AnimTarget.FillX).(DrawingBrush.Drawing).(GeometryDrawing.Geometry).(EllipseGeometry.RadiusX)",
			Linear(0.2, 0.4));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(1));
		var live = (DrawingBrush)el.GetValue(AnimTarget.FillProperty)!;
		Assert.NotSame(brush, live);
		Assert.Equal(0.4, ((EllipseGeometry)((GeometryDrawing)live.Drawing!).Geometry!).RadiusX, 6);
		Assert.Equal(0.45, inner.RadiusX);
	}

	[Fact]
	public void Animating_a_nested_object_repaints_the_owner()
	{
		var el = new AnimTarget();
		var brush = new DrawingBrush(new GeometryDrawing(null, null, new EllipseGeometry(new Point(), 1, 1)));
		el.SetValue(AnimTarget.FillProperty, brush);
		var before = el.FillCallbacks;
		Run(el, "(AnimTarget.FillX).(DrawingBrush.Drawing).(GeometryDrawing.Geometry).(EllipseGeometry.RadiusY)",
			Linear(1, 3));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(0.5));
		Assert.True(el.FillCallbacks > before);
	}
}
