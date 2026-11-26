using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Animation.AnimationTestHelpers;

namespace Gtk4.PresentationFramework.Tests.Animation;

public class GraphicsAnimatableTests
{
	[Fact]
	public void Transform_dependency_properties_have_the_WPF_defaults()
	{
		var s = new ScaleTransform();
		Assert.Equal(1.0, s.ScaleX);
		Assert.Equal(1.0, s.ScaleY);
		Assert.Equal(0.0, s.CenterX);
		Assert.Equal(0.0, new RotateTransform().Angle);
		Assert.Equal(0.0, new SkewTransform().AngleX);
		Assert.Equal(0.0, new TranslateTransform().X);
		Assert.Equal(Colors.Transparent, new SolidColorBrush().Color);
		Assert.Equal(1.0, ScaleTransform.ScaleXProperty.GetMetadata(typeof(ScaleTransform)).DefaultValue);
	}

	[Fact]
	public void Rotate_angle_is_animatable_and_changes_the_matrix()
	{
		var r = new RotateTransform(0, 10, 10);
		r.BeginAnimation(RotateTransform.AngleProperty, Linear(0, 90));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(1));
		Assert.Equal(90.0, r.Angle, 6);
		Assert.Equal(-1.0, r.Value.M21, 6);
		r.BeginAnimation(RotateTransform.AngleProperty, null);
		Assert.Equal(0.0, r.Angle);
	}

	[Fact]
	public void Frozen_transforms_and_geometries_reject_writes()
	{
		var s = new ScaleTransform(2, 2);
		s.Freeze();
		Assert.Throws<InvalidOperationException>(() => s.ScaleX = 3);
		var e = new EllipseGeometry(new Point(1, 1), 2, 3);
		e.Freeze();
		Assert.Throws<InvalidOperationException>(() => e.RadiusX = 9);
		Assert.Throws<InvalidOperationException>(() => e.BeginAnimation(EllipseGeometry.RadiusXProperty, Linear(0, 1)));
		Assert.Throws<InvalidOperationException>(() => Geometry.Empty.Transform = new TranslateTransform());
	}

	[Fact]
	public void Clone_copies_dependency_property_state()
	{
		var e = new EllipseGeometry(new Point(1, 2), 3, 4) { Transform = new TranslateTransform(5, 6) };
		var clone = (EllipseGeometry)e.Clone();
		Assert.Equal(new Point(1, 2), clone.Center);
		Assert.Equal(3.0, clone.RadiusX);
		Assert.NotSame(e.Transform, clone.Transform);
		Assert.Equal(5.0, ((TranslateTransform)clone.Transform).X);
		var group = new DrawingGroup();
		group.Children.Add(new GeometryDrawing(Brushes.Red, null, e));
		var copy = (DrawingGroup)group.Clone();
		Assert.Single(copy.Children);
		Assert.NotSame(group.Children[0], copy.Children[0]);
		var brush = new DrawingBrush(group) { TileMode = TileMode.FlipXY, Viewport = new Rect(0, 0, 10, 10) };
		var brushCopy = (DrawingBrush)brush.Clone();
		Assert.Equal(TileMode.FlipXY, brushCopy.TileMode);
		Assert.Equal(new Rect(0, 0, 10, 10), brushCopy.Viewport);
	}

	[Fact]
	public void Geometry_group_clones_children_and_freezes_them()
	{
		var g = new GeometryGroup();
		g.Children.Add(new LineGeometry(new Point(0, 0), new Point(3, 4)));
		var copy = (GeometryGroup)g.Clone();
		Assert.NotSame(g.Children[0], copy.Children[0]);
		g.Freeze();
		Assert.True(g.Children[0].IsFrozen);
	}

	[Fact]
	public void Changing_a_nested_freezable_raises_Changed_on_every_owner()
	{
		var e = new EllipseGeometry(new Point(), 1, 1);
		var drawing = new GeometryDrawing(Brushes.Red, null, e);
		var group = new DrawingGroup();
		group.Children.Add(drawing);
		var brush = new DrawingBrush(group);
		var count = 0;
		brush.Changed += (s, a) => count++;
		e.RadiusX = 5;
		Assert.Equal(1, count);
		group.Children.Clear();
		Assert.Equal(2, count);
		e.RadiusX = 6;
		Assert.Equal(2, count);
	}

	[Fact]
	public void PointAnimation_interpolates_from_to_and_by()
	{
		var e = new EllipseGeometry(new Point(10, 10), 1, 1);
		e.BeginAnimation(EllipseGeometry.CenterProperty,
			new PointAnimation { By = new Point(10, 20), Duration = TimeSpan.FromSeconds(1) });
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(0.5));
		Assert.Equal(new Point(15, 20), e.Center);
		Assert.Equal(new Point(10, 10), (Point)e.GetAnimationBaseValue(EllipseGeometry.CenterProperty)!);
		Assert.Equal(typeof(Point), new PointAnimation().TargetPropertyType);
	}

	[Fact]
	public void Solid_color_brush_animates_through_the_animatable_api()
	{
		var b = new SolidColorBrush(Colors.Black);
		b.BeginAnimation(SolidColorBrush.ColorProperty,
			new ColorAnimation(Colors.Black, Colors.White, TimeSpan.FromSeconds(1)));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(0.5));
		Assert.Equal(Color.FromArgb(255, 128, 128, 128), b.Color);
		Assert.True(b.HasAnimatedProperties);
	}
}
