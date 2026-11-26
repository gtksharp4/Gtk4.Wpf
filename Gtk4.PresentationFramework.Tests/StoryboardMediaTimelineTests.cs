using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class StoryboardMediaTimelineTests : WpfGtkTestBase
{
	[Fact]
	public void MediaTimeline_BindsItsClockToTheNamedMediaElement() => Run(() =>
	{
		var media = new MediaElement { Name = "m" };
		var timeline = new MediaTimeline(new Uri("file:///tmp/x.wav"));
		Storyboard.SetTargetName(timeline, "m");
		var sb = new Storyboard();
		sb.Children.Add(timeline);
		var host = new Canvas();
		host.Children.Add(media);
		host.RegisterName("m", media);

		sb.Begin(host);

		Assert.NotNull(media.Clock);
		Assert.IsType<MediaClock>(media.Clock);
	});

	[Fact]
	public void MediaTimeline_TargetingANonMediaElement_Throws() => Run(() =>
	{
		var border = new Border { Name = "b" };
		var timeline = new MediaTimeline(new Uri("file:///tmp/x.wav"));
		Storyboard.SetTargetName(timeline, "b");
		var sb = new Storyboard();
		sb.Children.Add(timeline);
		var host = new Canvas();
		host.Children.Add(border);
		host.RegisterName("b", border);

		Assert.Throws<InvalidOperationException>(() => sb.Begin(host));
	});

	[Fact]
	public void MediaTimeline_WithoutATarget_BindsToTheContainerMediaElement() => Run(() =>
	{
		var media = new MediaElement();
		var timeline = new MediaTimeline(new Uri("file:///tmp/x.wav"));
		var sb = new Storyboard();
		sb.Children.Add(timeline);

		sb.Begin(media);

		Assert.IsType<MediaClock>(media.Clock);
	});
}
