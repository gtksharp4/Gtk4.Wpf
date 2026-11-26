using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media3D;

public partial class KeyFrameAnimationTests
{
	sealed class DerivedKeyFrames : DoubleAnimationUsingKeyFrames { }

	static LinearDoubleKeyFrame Frame(double value, KeyTime? time = null) =>
		time == null ? new LinearDoubleKeyFrame(value) : new LinearDoubleKeyFrame(value, time.Value);

	static DoubleAnimationUsingKeyFrames Animation(double seconds, params DoubleKeyFrame[] frames)
	{
		var animation = new DoubleAnimationUsingKeyFrames { Duration = new Duration(TimeSpan.FromSeconds(seconds)) };
		foreach (var frame in frames) animation.KeyFrames.Add(frame);
		return animation;
	}

	[Fact]
	public void Leading_uniform_frames_resolve_from_a_virtual_time_zero()
	{
		var animation = Animation(3, Frame(10), Frame(20), Frame(30));
		Assert.Equal(5.0, Sample(animation, 0.5), 6);
		Assert.Equal(10.0, Sample(animation, 1), 6);
		Assert.Equal(20.0, Sample(animation, 2), 6);
		Assert.Equal(30.0, Sample(animation, 3), 6);
	}

	[Fact]
	public void A_uniform_frame_before_a_timed_frame_takes_half_of_its_time()
	{
		var animation = Animation(3, Frame(10), Frame(20, TimeSpan.FromSeconds(1)), Frame(30));
		Assert.Equal(5.0, Sample(animation, 0.25), 6);
		Assert.Equal(10.0, Sample(animation, 0.5), 6);
		Assert.Equal(20.0, Sample(animation, 1), 6);
		Assert.Equal(30.0, Sample(animation, 3), 6);
	}

	[Fact]
	public void Default_key_time_frames_start_from_the_origin_not_from_time_zero()
	{
		var animation = Animation(1, Frame(10), Frame(20));
		Assert.Equal(5.0, Sample(animation, 0.25), 6);
		Assert.Equal(10.0, Sample(animation, 0.5), 6);
	}

	[Fact]
	public void A_leading_paced_frame_is_pinned_to_time_zero()
	{
		var animation = Animation(1, Frame(10, KeyTime.Paced), Frame(20));
		Assert.Equal(10.1, Sample(animation, 0.01), 6);
		Assert.Equal(15.0, Sample(animation, 0.5), 6);
	}

	[Fact]
	public void Cumulative_repeats_add_the_last_frame_value_per_completed_iteration()
	{
		var animation = Animation(1, Frame(10, TimeSpan.FromSeconds(1)));
		animation.RepeatBehavior = new RepeatBehavior(3);
		animation.IsCumulative = true;
		Assert.Equal(65.0, Sample(animation, 1.5, origin: 100), 6);
	}

	[Fact]
	public void The_animation_is_not_sealed_and_a_subclass_animates()
	{
		Assert.False(typeof(DoubleAnimationUsingKeyFrames).IsSealed);
		var derived = new DerivedKeyFrames { Duration = new Duration(TimeSpan.FromSeconds(1)) };
		derived.KeyFrames.Add(Frame(10, TimeSpan.FromSeconds(1)));
		Assert.Equal(5.0, Sample(derived, 0.5), 6);
	}

	[Fact]
	public void SplineDoubleKeyFrame_has_a_frozen_linear_default_spline()
	{
		var frame = new SplineDoubleKeyFrame();
		Assert.NotNull(frame.KeySpline);
		Assert.Equal(new Point(0, 0), frame.KeySpline.ControlPoint1);
		Assert.Equal(new Point(1, 1), frame.KeySpline.ControlPoint2);
		int changes = 0;
		frame.Changed += (s, e) => changes++;
		_ = frame.KeySpline;
		Assert.Equal(0, changes);
		Assert.Equal(DependencyProperty.UnsetValue, frame.ReadLocalValue(SplineDoubleKeyFrame.KeySplineProperty));
	}

	[Fact]
	public void Key_frame_collections_derive_from_Freezable_and_3D_collections_from_the_wpf_bases()
	{
		Assert.Equal(typeof(Freezable), typeof(DoubleKeyFrameCollection).BaseType);
		Assert.Equal(typeof(Freezable), typeof(Point3DCollection).BaseType);
		Assert.Equal(typeof(Freezable), typeof(Vector3DCollection).BaseType);
		Assert.Equal(typeof(Freezable), typeof(System.Windows.Media.Int32Collection).BaseType);
		Assert.Equal(typeof(Animatable), typeof(Model3DCollection).BaseType);
		Assert.Equal(typeof(Animatable), typeof(MaterialCollection).BaseType);
		Assert.Equal(typeof(Animatable), typeof(Transform3DCollection).BaseType);
	}
}
