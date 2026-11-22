using System;
using System.Windows;
using System.Windows.Media.Animation;
using Gtk4.Forms.PresentationFramework.Tests.Animation;
using Xunit;
using static Gtk4.Forms.PresentationFramework.Tests.Animation.AnimationTestHelpers;

namespace Gtk4.Forms.PresentationFramework.Tests.InputDevices;

/// <summary>Compiling as a FrameworkElement subclass proves the method wins over the trigger class.</summary>
sealed class StoryboardHost : FrameworkElement
{
	public static readonly DependencyProperty AmountProperty = DependencyProperty.Register(
		"HostAmount", typeof(double), typeof(StoryboardHost), new PropertyMetadata(10.0));

	public double Amount => (double)GetValue(AmountProperty)!;

	public void Start(Storyboard s) => BeginStoryboard(s);
	public void Start(Storyboard s, HandoffBehavior h) => BeginStoryboard(s, h);
	public void Start(Storyboard s, HandoffBehavior h, bool c) => BeginStoryboard(s, h, c);

	protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
}

public class BeginStoryboardTests
{
	static Storyboard Make(double to = 100)
	{
		var anim = Linear(0, to);
		var sb = new Storyboard();
		sb.Children.Add(anim);
		Storyboard.SetTargetProperty(anim, new PropertyPath(StoryboardHost.AmountProperty));
		return sb;
	}

	[Fact]
	public void Overloads_animate_to_the_To_value()
	{
		var host = new StoryboardHost();
		host.Start(Make());
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(1));
		Assert.Equal(100.0, host.Amount, 6);

		var h2 = new StoryboardHost();
		h2.Start(Make(40), HandoffBehavior.Compose);
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(1));
		Assert.Equal(40.0, h2.Amount, 6);

		var h3 = new StoryboardHost();
		h3.Start(Make(70), HandoffBehavior.SnapshotAndReplace, true);
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(1));
		Assert.Equal(70.0, h3.Amount, 6);
	}

	[Fact]
	public void Null_storyboard_throws_for_every_overload()
	{
		var host = new StoryboardHost();
		Assert.Equal("storyboard", Assert.Throws<ArgumentNullException>(() => host.BeginStoryboard(null!)).ParamName);
		Assert.Throws<ArgumentNullException>(() => host.BeginStoryboard(null!, HandoffBehavior.Compose));
		Assert.Throws<ArgumentNullException>(() => host.BeginStoryboard(null!, HandoffBehavior.Compose, true));
	}

	[Fact]
	public void Not_controllable_by_default_so_pause_is_a_noop()
	{
		var host = new StoryboardHost();
		var sb = Make();
		host.BeginStoryboard(sb);
		sb.Pause(host);
		Assert.False(sb.GetIsPaused(host));
		AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(1));
		Assert.Equal(100.0, host.Amount, 6);
	}

	[Fact]
	public void Controllable_allows_pause_and_resume()
	{
		var host = new StoryboardHost();
		var sb = Make();
		host.BeginStoryboard(sb, HandoffBehavior.SnapshotAndReplace, true);
		sb.Pause(host);
		Assert.True(sb.GetIsPaused(host));
		sb.Resume(host);
		Assert.False(sb.GetIsPaused(host));
	}

	[Fact]
	public void Trigger_action_class_is_still_the_Triggers_type()
	{
		var t = new System.Windows.BeginStoryboard { Storyboard = Make() };
		Assert.IsAssignableFrom<TriggerAction>(t);
	}
}
