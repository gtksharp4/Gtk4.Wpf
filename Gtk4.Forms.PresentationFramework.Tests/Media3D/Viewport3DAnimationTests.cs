using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Xunit;
using static Gtk4.Forms.PresentationFramework.Tests.Media3D.TexturedSceneHelpers;

namespace Gtk4.Forms.PresentationFramework.Tests.Media3D;

/// <summary>The HexSphereDemo storyboard: named rotations nested in a model tree, animated through the window.</summary>
public class Viewport3DAnimationTests : WpfGtkTestBase
{
	sealed class Scene
	{
		internal Window Window = null!;
		internal Viewport3D Viewport = null!;
		internal AxisAngleRotation3D Inner = null!, Outer = null!;
		internal Storyboard Storyboard = null!;
	}

	/// <summary>An off-centre quad under a Transform3DGroup/Model3DGroup chain with two named rotations.</summary>
	static Scene Build()
	{
		var scene = new Scene { Inner = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0),
			Outer = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0) };
		var mesh = new MeshGeometry3D
		{
			Positions = Point3DCollection.Parse("0.3 -0.5 0  1.3 -0.5 0  0.3 0.5 0  1.3 0.5 0"),
			TriangleIndices = Int32Collection.Parse("0 1 2  1 3 2"),
		};
		var geometry = new GeometryModel3D(mesh, new EmissiveMaterial(Brushes.Red));
		var group = new Model3DGroup();
		group.Children.Add(geometry);
		var transforms = new Transform3DGroup();
		transforms.Children.Add(new ScaleTransform3D(1, 1, 1));
		transforms.Children.Add(new RotateTransform3D(scene.Outer));
		transforms.Children.Add(new RotateTransform3D(scene.Inner));
		group.Transform = transforms;
		scene.Viewport = Viewport(group);
		scene.Window = new Window { Content = scene.Viewport };
		scene.Window.RegisterName("myRotateInner", scene.Inner);
		scene.Window.RegisterName("myRotateOuter", scene.Outer);
		scene.Storyboard = Rotate(20, 30);
		scene.Window.Resources["RotateStoryboard"] = scene.Storyboard;
		return scene;
	}

	static Storyboard Rotate(double innerSeconds, double outerSeconds)
	{
		var storyboard = new Storyboard();
		storyboard.Children.Add(Spin("myRotateInner", innerSeconds));
		storyboard.Children.Add(Spin("myRotateOuter", outerSeconds));
		return storyboard;
	}

	static ParallelTimeline Spin(string target, double seconds)
	{
		var timeline = new ParallelTimeline { RepeatBehavior = RepeatBehavior.Forever };
		Storyboard.SetTargetName(timeline, target);
		Storyboard.SetTargetProperty(timeline, new PropertyPath("Angle"));
		timeline.Children.Add(new DoubleAnimation(0, 360, new Duration(TimeSpan.FromSeconds(seconds))));
		return timeline;
	}

	static double Angle(AxisAngleRotation3D rotation) => (double)rotation.GetValue(AxisAngleRotation3D.AngleProperty)!;

	static void Tick(double seconds) => AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(seconds));

	[Fact]
	public void FindName_returns_the_instance_that_sits_in_the_model_tree() => Run(() =>
	{
		var scene = Build();
		Assert.Same(scene.Inner, scene.Window.FindName("myRotateInner"));
		var model = (Model3DGroup)((ModelVisual3D)scene.Viewport.Children[0]).Content!;
		var group = (Transform3DGroup)model.Transform;
		Assert.Same(scene.Inner, ((RotateTransform3D)group.Children[2]).Rotation);
	});

	[Fact]
	public void A_repeating_parallel_timeline_runs_wraps_and_stays_active() => Run(() =>
	{
		var scene = Build();
		scene.Storyboard.Begin(scene.Window, true);
		Tick(5);
		Assert.Equal(90.0, Angle(scene.Inner), 1);
		Tick(20);
		Assert.Equal(90.0, Angle(scene.Inner), 1);
		Assert.Equal(ClockState.Active, scene.Storyboard.GetCurrentState(scene.Window));
		scene.Storyboard.Stop(scene.Window);
	});

	[Fact]
	public void The_two_parallel_timelines_advance_independently() => Run(() =>
	{
		var scene = Build();
		scene.Window.BeginStoryboard((Storyboard)scene.Window.Resources["RotateStoryboard"]!);
		Tick(15);
		Assert.Equal(270.0, Angle(scene.Inner), 1);
		Assert.Equal(180.0, Angle(scene.Outer), 1);
		Tick(10);
		Assert.Equal(90.0, Angle(scene.Inner), 1);
		Assert.Equal(300.0, Angle(scene.Outer), 1);
	});

	[Fact]
	public void Every_tick_invalidates_the_viewport_and_changes_the_frame() => Run(() =>
	{
		var scene = Build();
		scene.Window.BeginStoryboard(scene.Storyboard);
		using var before = Paint(scene.Viewport);
		var first = PixelAt(before, 130, 100);
		Assert.True(first.R > 200, $"quad not drawn at angle 0: {first}");
		int count = scene.Viewport.InvalidateCount;
		Tick(1);
		Assert.True(scene.Viewport.InvalidateCount > count, "tick did not reach Invalidate3D");
		count = scene.Viewport.InvalidateCount;
		Tick(4);
		Assert.True(scene.Viewport.InvalidateCount > count);
		using var after = Paint(scene.Viewport);
		// At 90 degrees about Y the quad is edge-on, so the pixel that was red is the backdrop again.
		Assert.True(IsBackdrop(PixelAt(after, 130, 100)), $"{PixelAt(after, 130, 100)}");
	});

	[Fact]
	public void Replacing_the_rotation_object_repaints_as_the_trackball_does() => Run(() =>
	{
		var scene = Build();
		var model = (Model3DGroup)((ModelVisual3D)scene.Viewport.Children[0]).Content!;
		var rotate = (RotateTransform3D)((Transform3DGroup)model.Transform).Children[2];
		int count = scene.Viewport.InvalidateCount;
		rotate.Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 90);
		Assert.True(scene.Viewport.InvalidateCount > count);
		using var surface = Paint(scene.Viewport);
		Assert.True(IsBackdrop(PixelAt(surface, 130, 100)));
	});
}
