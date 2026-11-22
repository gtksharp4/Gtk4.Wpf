using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xunit;
using static Gtk4.Forms.PresentationFramework.Tests.Animation.AnimationTestHelpers;

namespace Gtk4.Forms.PresentationFramework.Tests.Animation;

/// <summary>Layer gaps (WPF DependencyObjects that are plain classes here) are skipped; other CLR objects throw.</summary>
public class StoryboardPathResolverGapTests
{
	sealed class Holder : FrameworkElement
	{
		public BezierSegment Segment { get; } = new BezierSegment();
		public Pen Pen { get; } = new Pen();
		public object Plain { get; } = new object();
		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
	}

	[Theory]
	[InlineData("(Holder.Segment).(BezierSegment.Point1)")]
	[InlineData("(Holder.Pen).(Pen.Thickness)")]
	public void A_path_through_a_layer_gap_type_is_an_unanimatable_target(string path)
	{
		var ex = Assert.Throws<UnanimatableTargetException>(
			() => StoryboardPathResolver.Resolve(new Holder(), new PropertyPath(path)));

		Assert.Contains("cannot be read from", ex.Message);
	}

	[Theory]
	[InlineData("(Holder.Segment).(BezierSegment.Point1)")]
	[InlineData("(Holder.Pen).(Pen.Thickness)")]
	public void Begin_skips_an_animation_through_a_gap_without_throwing(string path)
	{
		var holder = new Holder();
		var sb = new Storyboard();
		var anim = Linear(0, 1);
		Storyboard.SetTargetProperty(anim, new PropertyPath(path));
		sb.Children.Add(anim);

		sb.Begin(holder, true);

		Assert.Empty(sb.ActiveClocks);
	}

	[Fact]
	public void A_path_through_a_plain_clr_object_still_throws_InvalidOperationException()
	{
		var ex = Assert.Throws<InvalidOperationException>(
			() => StoryboardPathResolver.Resolve(new Holder(), new PropertyPath("(Holder.Plain).(Pen.Thickness)")));

		Assert.Contains("cannot be read from a Object", ex.Message);
	}
}
