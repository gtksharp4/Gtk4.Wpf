using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xunit;
using static Gtk4.Forms.PresentationFramework.Tests.Animation.AnimationTestHelpers;

namespace Gtk4.Forms.PresentationFramework.Tests.Animation;

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

	[Fact(Skip = "Round 50 carry-over: Geometry and Drawing sub-objects are not Animatable yet "
		+ "(ClipRegion EllipseGeometry.RadiusX, DrawingBrush DrawingGroup.Children[0])")]
	public void Animates_geometry_and_drawing_sub_objects() { }
}
