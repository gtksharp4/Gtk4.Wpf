using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Animation.AnimationTestHelpers;

namespace Gtk4.PresentationFramework.Tests.Media3D;

public partial class KeyFrameAnimationTests
{
	static AxisAngleRotation3D Target() => new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);

	static double Sample(DoubleAnimationUsingKeyFrames animation, double seconds, double origin = 0)
	{
		var target = Target();
		target.Angle = origin;
		var clock = animation.CreateClock();
		target.ApplyAnimationClock(AxisAngleRotation3D.AngleProperty, clock);
		Tick(clock, seconds);
		return target.Angle;
	}

	[Fact]
	public void KeyTime_validates_and_reports_its_type()
	{
		Assert.Equal(KeyTimeType.Percent, KeyTime.FromPercent(0.5).Type);
		Assert.Equal(0.5, KeyTime.FromPercent(0.5).Percent);
		Assert.Throws<ArgumentOutOfRangeException>(() => KeyTime.FromPercent(1.5));
		Assert.Throws<ArgumentOutOfRangeException>(() => KeyTime.FromPercent(-0.1));
		Assert.Throws<ArgumentOutOfRangeException>(() => KeyTime.FromTimeSpan(TimeSpan.FromSeconds(-1)));
		Assert.Equal(KeyTimeType.Uniform, default(KeyTime).Type);
		Assert.Equal(KeyTimeType.Paced, KeyTime.Paced.Type);
		Assert.Throws<InvalidOperationException>(() => _ = KeyTime.Uniform.TimeSpan);
		KeyTime implicitConversion = TimeSpan.FromSeconds(2);
		Assert.Equal(TimeSpan.FromSeconds(2), implicitConversion.TimeSpan);
	}

	[Fact]
	public void KeyTimeConverter_parses_percent_timespan_and_keywords()
	{
		var converter = new KeyTimeConverter();
		Assert.Equal(KeyTime.FromPercent(0.5), converter.ConvertFromInvariantString("50%"));
		Assert.Equal(KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1)), converter.ConvertFromInvariantString("0:0:1"));
		Assert.Equal(KeyTime.Uniform, converter.ConvertFromInvariantString("Uniform"));
		Assert.Equal(KeyTime.Paced, converter.ConvertFromInvariantString("Paced"));
		Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(null!));
		Assert.Equal("50%", converter.ConvertToInvariantString(KeyTime.FromPercent(0.5)));
	}

	[Fact]
	public void KeySpline_validates_control_points_and_computes_progress()
	{
		var linear = new KeySpline();
		Assert.Equal(new Point(0, 0), linear.ControlPoint1);
		Assert.Equal(new Point(1, 1), linear.ControlPoint2);
		Assert.Equal(0.3, linear.GetSplineProgress(0.3), 6);
		Assert.Throws<ArgumentException>(() => linear.ControlPoint1 = new Point(-0.1, 0));
		Assert.Throws<ArgumentException>(() => new KeySpline(0, 0, 1.5, 1));
		var easeIn = new KeySpline(0.5, 0, 1, 1);
		Assert.True(easeIn.GetSplineProgress(0.5) < 0.5);
		Assert.Equal(0.0, easeIn.GetSplineProgress(0));
		Assert.Equal(1.0, easeIn.GetSplineProgress(1));
		var step = new KeySpline(0, 1, 1, 0);
		Assert.Equal(0.5, step.GetSplineProgress(0.5), 6);
	}

	[Fact]
	public void KeySplineConverter_parses_two_points()
	{
		var spline = (KeySpline)new KeySplineConverter().ConvertFromInvariantString("0.1,0.2 0.3,0.4")!;
		Assert.Equal(new Point(0.1, 0.2), spline.ControlPoint1);
		Assert.Equal(new Point(0.3, 0.4), spline.ControlPoint2);
		Assert.Throws<InvalidOperationException>(() => new KeySplineConverter().ConvertFromInvariantString("0.1,0.2"));
	}

	[Fact]
	public void Key_frames_interpolate_discretely_linearly_with_splines_and_easing()
	{
		Assert.Equal(1.0, new DiscreteDoubleKeyFrame(5).InterpolateValue(1, 0.99));
		Assert.Equal(5.0, new DiscreteDoubleKeyFrame(5).InterpolateValue(1, 1));
		Assert.Equal(3.0, new LinearDoubleKeyFrame(5).InterpolateValue(1, 0.5));
		var spline = new SplineDoubleKeyFrame(10, KeyTime.Uniform, new KeySpline(0.5, 0, 1, 1));
		Assert.True(spline.InterpolateValue(0, 0.5) < 5);
		Assert.Equal(10.0, spline.InterpolateValue(0, 1), 6);
		Assert.NotNull(new SplineDoubleKeyFrame().KeySpline);
		var easing = new EasingDoubleKeyFrame(10) { EasingFunction = new SquareEase() };
		Assert.Equal(2.5, easing.InterpolateValue(0, 0.5), 9);
		Assert.Throws<ArgumentOutOfRangeException>(() => new LinearDoubleKeyFrame().InterpolateValue(0, 1.1));
		Assert.Throws<ArgumentOutOfRangeException>(() => new LinearDoubleKeyFrame().InterpolateValue(0, -0.1));
	}

	sealed class SquareEase : IEasingFunction
	{
		public double Ease(double normalizedTime) => normalizedTime * normalizedTime;
	}

	[Fact]
	public void Animation_samples_discrete_linear_and_spline_frames_through_a_clock()
	{
		var animation = new DoubleAnimationUsingKeyFrames();
		animation.KeyFrames.Add(new DiscreteDoubleKeyFrame(10, TimeSpan.FromSeconds(1)));
		animation.KeyFrames.Add(new LinearDoubleKeyFrame(20, TimeSpan.FromSeconds(2)));
		animation.KeyFrames.Add(new SplineDoubleKeyFrame(40, TimeSpan.FromSeconds(3), new KeySpline(0, 0, 1, 1)));
		Assert.Equal(0.0, Sample(animation, 0.5));
		Assert.Equal(10.0, Sample(animation, 1));
		Assert.Equal(15.0, Sample(animation, 1.5));
		Assert.Equal(30.0, Sample(animation, 2.5), 6);
		Assert.Equal(40.0, Sample(animation, 3));
	}

	[Fact]
	public void Duration_is_the_largest_timespan_key_time_else_one_second()
	{
		var timed = new DoubleAnimationUsingKeyFrames();
		timed.KeyFrames.Add(new LinearDoubleKeyFrame(10, TimeSpan.FromSeconds(4)));
		var clock = timed.CreateClock();
		Tick(clock, 1);
		Assert.Equal(0.25, clock.CurrentProgress!.Value, 9);

		var untimed = new DoubleAnimationUsingKeyFrames();
		untimed.KeyFrames.Add(new LinearDoubleKeyFrame(10));
		Assert.Equal(5.0, Sample(untimed, 0.5), 9);
	}

	[Fact]
	public void Percent_and_uniform_key_times_resolve_against_the_duration()
	{
		var animation = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromSeconds(4) };
		animation.KeyFrames.Add(new LinearDoubleKeyFrame(10, KeyTime.FromPercent(0.5)));
		animation.KeyFrames.Add(new LinearDoubleKeyFrame(20, KeyTime.Uniform));
		animation.KeyFrames.Add(new LinearDoubleKeyFrame(30, KeyTime.Uniform));
		Assert.Equal(5.0, Sample(animation, 1), 6);
		Assert.Equal(10.0, Sample(animation, 2), 6);
		Assert.Equal(15.0, Sample(animation, 2.5), 6);
		Assert.Equal(30.0, Sample(animation, 4), 6);
	}

	[Fact]
	public void IsAdditive_adds_the_base_value_and_the_first_frame_starts_from_zero()
	{
		var animation = new DoubleAnimationUsingKeyFrames { IsAdditive = true };
		animation.KeyFrames.Add(new LinearDoubleKeyFrame(10, TimeSpan.FromSeconds(1)));
		Assert.Equal(105.0, Sample(animation, 0.5, origin: 100), 6);
		Assert.Equal(110.0, Sample(animation, 1, origin: 100), 6);
	}

	[Fact]
	public void Key_frame_animation_is_content_aware_and_clones_its_frames()
	{
		var attribute = (ContentPropertyAttribute)Attribute.GetCustomAttribute(
			typeof(DoubleAnimationUsingKeyFrames), typeof(ContentPropertyAttribute))!;
		Assert.Equal("KeyFrames", attribute.Name);
		var animation = new DoubleAnimationUsingKeyFrames();
		((IAddChild)animation).AddChild(new LinearDoubleKeyFrame(3, TimeSpan.FromSeconds(1)));
		Assert.Throws<InvalidOperationException>(() => ((IAddChild)animation).AddChild("x"));
		Assert.Single(((IKeyFrameAnimation)animation).KeyFrames);
		var clone = animation.Clone();
		Assert.NotSame(animation.KeyFrames, clone.KeyFrames);
		Assert.Single(clone.KeyFrames);
		animation.Freeze();
		Assert.True(animation.KeyFrames.IsFrozen);
		Assert.Equal(typeof(double), animation.TargetPropertyType);
	}
}

static class TimelineTestExtensions
{
	internal static TimeSpan ResolveNaturalDuration(this TimelineGroup group)
	{
		var longest = TimeSpan.Zero;
		foreach (var child in group.Children)
			if (child.Duration.HasTimeSpan && child.Duration.TimeSpan > longest) longest = child.Duration.TimeSpan;
		return longest;
	}
}
