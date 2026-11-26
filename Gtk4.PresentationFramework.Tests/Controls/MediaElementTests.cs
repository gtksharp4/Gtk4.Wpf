using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class MediaElementTests : WpfGtkTestBase
{
	[Fact]
	public void Defaults_match_WPF()
	{
		var m = new MediaElement();
		Assert.Null(m.Source);
		Assert.Equal(0.5, m.Volume);
		Assert.Equal(0.0, m.Balance);
		Assert.False(m.IsMuted);
		Assert.False(m.ScrubbingEnabled);
		Assert.Equal(MediaState.Close, m.UnloadedBehavior);
		Assert.Equal(MediaState.Play, m.LoadedBehavior);
		Assert.Equal(Stretch.Uniform, m.Stretch);
		Assert.Equal(StretchDirection.Both, m.StretchDirection);
		Assert.Equal(1.0, m.SpeedRatio);
		Assert.Equal(TimeSpan.Zero, m.Position);
		Assert.Null(m.Clock);
		Assert.Equal(Duration.Automatic, m.NaturalDuration);
		Assert.False(m.HasAudio || m.HasVideo || m.IsBuffering || m.CanPause);
		Assert.Equal(0, m.NaturalVideoWidth + m.NaturalVideoHeight);
		Assert.IsAssignableFrom<System.Windows.Markup.IUriContext>(m);
		Assert.IsAssignableFrom<FrameworkElement>(m);
	}

	[Fact]
	public void Behavior_properties_reject_undefined_states()
	{
		var m = new MediaElement();
		Assert.Throws<ArgumentException>(() => m.LoadedBehavior = (MediaState)9);
		Assert.Throws<ArgumentException>(() => m.UnloadedBehavior = (MediaState)(-1));
	}

	[Fact]
	public void Routed_events_use_the_WPF_handler_types()
	{
		Assert.Equal(typeof(RoutedEventHandler), MediaElement.MediaOpenedEvent.HandlerType);
		Assert.Equal(typeof(RoutedEventHandler), MediaElement.MediaEndedEvent.HandlerType);
		Assert.Equal(typeof(EventHandler<ExceptionRoutedEventArgs>), MediaElement.MediaFailedEvent.HandlerType);
		Assert.Equal(typeof(EventHandler<MediaScriptCommandRoutedEventArgs>),
			MediaElement.ScriptCommandEvent.HandlerType);
		Assert.Equal(RoutingStrategy.Bubble, MediaElement.BufferingStartedEvent.RoutingStrategy);
	}

	[Fact]
	public void Control_methods_throw_NotSupported_unless_Loaded_or_Unloaded_is_Manual()
	{
		var m = new MediaElement();
		Assert.Throws<NotSupportedException>(() => m.Play());
		Assert.Throws<NotSupportedException>(() => m.Pause());
		Assert.Throws<NotSupportedException>(() => m.Stop());
		Assert.Throws<NotSupportedException>(() => m.Close());
		var unloadedManual = new MediaElement
		{
			UnloadedBehavior = MediaState.Manual, LoadedBehavior = MediaState.Play
		};
		Run(() => unloadedManual.Play());
	}

	[Fact]
	public void Control_methods_do_not_throw_while_a_Clock_is_set()
	{
		var m = new MediaElement { LoadedBehavior = MediaState.Manual };
		m.Clock = (MediaClock)new MediaTimeline(new Uri("a.wav", UriKind.Relative)).CreateClock();
		m.Play();
		m.Position = TimeSpan.FromSeconds(1);
	}

	[Fact]
	public void MediaTimeline_allocates_a_MediaClock()
	{
		var uri = new Uri("a.wav", UriKind.Relative);
		var timeline = new MediaTimeline(uri);
		var clock = timeline.CreateClock();
		var media = Assert.IsType<MediaClock>(clock);
		Assert.Same(timeline, media.Timeline);
		Assert.Equal(uri, timeline.Source);
		Assert.Equal(Duration.Automatic, media.NaturalDuration);
		Assert.IsAssignableFrom<System.Windows.Markup.IUriContext>(timeline);
	}

	[Fact]
	public void A_Storyboard_tolerates_a_MediaTimeline_child()
	{
		Run(() =>
		{
			var sb = new Storyboard();
			var timeline = new MediaTimeline(new Uri("a.wav", UriKind.Relative));
			Storyboard.SetTargetName(timeline, "m");
			sb.Children.Add(timeline);
			var host = new Canvas();
			var element = new MediaElement { Name = "m" };
			host.Children.Add(element);
			host.RegisterName("m", element);
			sb.Begin(host);
			sb.Stop(host);
		});
	}

	[Fact]
	public void A_missing_file_raises_MediaFailed_without_throwing()
	{
		Run(() =>
		{
			using var scope = new GtkDispatcherScope();
			var m = new MediaElement();
			ExceptionRoutedEventArgs? failure = null;
			m.MediaFailed += (_, e) => failure = e;
			m.Source = new Uri(Path.Combine(Path.GetTempPath(), "no-such-media-" + Guid.NewGuid() + ".wmv"));
			_ = m.Widget;
			Assert.True(GtkPump.Until(() => failure != null));
			Assert.IsType<FileNotFoundException>(failure!.ErrorException);
			Assert.Same(m, failure.Source);
		});
	}

	[Fact]
	public void A_relative_missing_file_resolves_against_the_app_base_and_fails()
	{
		Run(() =>
		{
			using var scope = new GtkDispatcherScope();
			var m = new MediaElement { Source = new Uri("media/none.wmv", UriKind.Relative) };
			Exception? error = null;
			m.MediaFailed += (_, e) => error = e.ErrorException;
			_ = m.Widget;
			Assert.True(GtkPump.Until(() => error != null));
			Assert.Contains("none.wmv", ((FileNotFoundException)error!).FileName);
		});
	}

	[Fact]
	public void Source_Volume_and_Position_reach_the_real_stream()
	{
		Run(() =>
		{
			using var scope = new GtkDispatcherScope();
			var path = WriteWav();
			try
			{
				var m = new MediaElement { LoadedBehavior = MediaState.Manual, Volume = 0.25 };
				bool opened = false, failed = false;
				m.MediaOpened += (_, _) => opened = true;
				m.MediaFailed += (_, _) => failed = true;
				m.Source = new Uri(path);
				_ = m.Widget;
				GtkPump.Until(() => opened || failed, 3000);
				// Volume and Muted reach the GTK stream whether or not a backend can decode the file.
				Assert.NotNull(m.Stream);
				Assert.Equal(0.25, m.Stream!.Volume, 6);
				m.IsMuted = true;
				Assert.True(m.Stream.Muted);
				m.Volume = 0.75;
				Assert.Equal(0.75, m.Stream.Volume, 6);
				m.IsMuted = false;
				Assert.SkipWhen(failed || !opened, "The GTK runtime has no media backend that decodes WAV.");
				Assert.True(m.NaturalDuration.HasTimeSpan);
				Assert.True(m.NaturalDuration.TimeSpan > TimeSpan.FromMilliseconds(500));
				Assert.True(m.HasAudio);
				Assert.True(m.CanPause);
				m.IsMuted = true;
				m.Volume = 0.75;
				m.Position = TimeSpan.FromMilliseconds(500);
				m.Play();
				m.Pause();
			}
			finally
			{
				File.Delete(path);
			}
		});
	}

	internal static string WriteWav()
	{
		var path = Path.Combine(Path.GetTempPath(), "mediaelement-" + Guid.NewGuid() + ".wav");
		const int rate = 8000;
		var data = new byte[rate * 2];
		using var writer = new BinaryWriter(File.Create(path), Encoding.ASCII);
		writer.Write(Encoding.ASCII.GetBytes("RIFF"));
		writer.Write(36 + data.Length);
		writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
		writer.Write(16);
		writer.Write((short)1);
		writer.Write((short)1);
		writer.Write(rate);
		writer.Write(rate * 2);
		writer.Write((short)2);
		writer.Write((short)16);
		writer.Write(Encoding.ASCII.GetBytes("data"));
		writer.Write(data.Length);
		writer.Write(data);
		return path;
	}
}
