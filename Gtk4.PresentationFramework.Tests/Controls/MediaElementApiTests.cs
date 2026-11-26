using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class MediaElementApiTests : WpfGtkTestBase
{
	[Fact]
	public void MediaClock_is_not_sealed_and_BaseUri_is_only_an_explicit_IUriContext()
	{
		Assert.False(typeof(MediaClock).IsSealed);
		Assert.Null(typeof(MediaElement).GetProperty("BaseUri"));
		Assert.Null(typeof(MediaTimeline).GetProperty("BaseUri"));
		var m = new MediaElement();
		var baseUri = new Uri("file:///tmp/");
		((IUriContext)m).BaseUri = baseUri;
		Assert.Equal(baseUri, ((IUriContext)m).BaseUri);
		var t = new MediaTimeline();
		((IUriContext)t).BaseUri = baseUri;
		Assert.Equal(baseUri, ((IUriContext)t).BaseUri);
	}

	[Fact]
	public void A_MediaTimeline_without_a_Source_cannot_create_a_clock()
	{
		Assert.Throws<InvalidOperationException>(() => new MediaTimeline().CreateClock());
	}

	[Fact]
	public void An_undecodable_file_raises_MediaFailed() => Run(() =>
	{
		using var scope = new GtkDispatcherScope();
		var path = Path.Combine(Path.GetTempPath(), "mediaelement-" + Guid.NewGuid() + ".wav");
		File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
		try
		{
			var m = new MediaElement { LoadedBehavior = MediaState.Manual };
			bool opened = false, failed = false;
			m.MediaOpened += (_, _) => opened = true;
			m.MediaFailed += (_, _) => failed = true;
			m.Source = new Uri(path);
			_ = m.Widget;
			GtkPump.Until(() => opened || failed, 3000);
			Assert.SkipWhen(opened, "The GTK runtime has no media backend; garbage was accepted.");
			Assert.True(failed);
		}
		finally
		{
			File.Delete(path);
		}
	});

	[Fact]
	public void MediaClock_NaturalDuration_is_set_after_MediaOpened() => Run(() =>
	{
		using var scope = new GtkDispatcherScope();
		var path = MediaElementTests.WriteWav();
		try
		{
			var m = new MediaElement { LoadedBehavior = MediaState.Manual };
			var clock = (MediaClock)new MediaTimeline(new Uri(path)).CreateClock();
			bool opened = false, failed = false;
			m.MediaOpened += (_, _) => opened = true;
			m.MediaFailed += (_, _) => failed = true;
			m.Clock = clock;
			_ = m.Widget;
			GtkPump.Until(() => opened || failed, 3000);
			Assert.SkipWhen(failed || !opened, "The GTK runtime has no media backend that decodes WAV.");
			Assert.True(clock.NaturalDuration.HasTimeSpan);
			Assert.True(clock.NaturalDuration.TimeSpan > TimeSpan.FromMilliseconds(500));
		}
		finally
		{
			File.Delete(path);
		}
	});
}
