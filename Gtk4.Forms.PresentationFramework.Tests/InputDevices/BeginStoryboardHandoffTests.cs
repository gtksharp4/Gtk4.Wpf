using System;
using System.Windows;
using System.Windows.Media.Animation;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.InputDevices;

/// <summary>FrameworkElement.BeginStoryboard hands a running animation over by Compose or SnapshotAndReplace.</summary>
public class BeginStoryboardHandoffTests
{
	static Storyboard Make(double? from, double to)
	{
		var anim = new DoubleAnimation { From = from, To = to, Duration = TimeSpan.FromSeconds(10) };
		var sb = new Storyboard();
		sb.Children.Add(anim);
		Storyboard.SetTargetProperty(anim, new PropertyPath(StoryboardHost.AmountProperty));
		return sb;
	}

	static void Tick(double seconds) => AnimationScheduler.TickAllForTest(TimeSpan.FromSeconds(seconds));

	// first runs 0 -> 100 over 10 s and is at 50 when second (To 200, no From) is begun; one second later.
	static (StoryboardHost host, Storyboard first, Storyboard second) Handoff(HandoffBehavior behavior)
	{
		var host = new StoryboardHost();
		var first = Make(0, 100);
		host.BeginStoryboard(first, HandoffBehavior.SnapshotAndReplace, true);
		Tick(5);
		Assert.Equal(50.0, host.Amount, 6);

		var second = Make(null, 200);
		host.BeginStoryboard(second, behavior, true);
		Tick(1);
		return (host, first, second);
	}

	[Fact]
	public void Compose_layers_on_the_running_animation()
	{
		var (host, first, second) = Handoff(HandoffBehavior.Compose);
		// origin is the first animation's value (60 at 6 s), 10% of the way to 200.
		Assert.Equal(74.0, host.Amount, 6);
		Assert.NotNull(first.GetCurrentState(host));
		Assert.NotNull(second.GetCurrentState(host));

		// the first finishes at 100 and still contributes; the second is halfway from there to 200.
		Tick(4);
		Assert.Equal(150.0, host.Amount, 6);
		Tick(5);
		Assert.Equal(200.0, host.Amount, 6);
	}

	[Fact]
	public void SnapshotAndReplace_restarts_from_the_snapshot_and_drops_the_first_clock()
	{
		var (host, _, second) = Handoff(HandoffBehavior.SnapshotAndReplace);
		// origin is the snapshot 50 (not 0, 10 or the first animation's later values), 10% of the way to 200.
		Assert.Equal(65.0, host.Amount, 6);
		Assert.NotNull(second.GetCurrentState(host));

		Tick(4);
		Assert.Equal(125.0, host.Amount, 6); // would be 150 if the first still contributed
		Tick(5);
		Assert.Equal(200.0, host.Amount, 6);
	}
}
