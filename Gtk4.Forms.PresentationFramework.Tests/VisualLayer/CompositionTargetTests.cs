using System;
using System.Collections.Generic;
using System.Windows.Media;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.VisualLayer;

public class CompositionTargetTests : WpfGtkTestBase
{
	[Fact]
	public void Rendering_fires_per_frame_with_monotonic_time_and_stops_on_unsubscribe() => Run(() =>
	{
		var times = new List<TimeSpan>();
		EventHandler handler = (s, e) => times.Add(((RenderingEventArgs)e).RenderingTime);
		CompositionTarget.Rendering += handler;
		try
		{
			Assert.True(GtkPump.Until(() => times.Count >= 3, 4000));
		}
		finally
		{
			CompositionTarget.Rendering -= handler;
		}
		for (int i = 1; i < times.Count; i++) Assert.True(times[i] >= times[i - 1]);
		Assert.True(times[times.Count - 1] > times[0]);

		int after = times.Count;
		GtkPump.For(120);
		Assert.Equal(after, times.Count);
	});

	[Fact]
	public void Timer_runs_while_any_handler_remains_and_handlers_share_each_frame() => Run(() =>
	{
		int a = 0, b = 0;
		EventHandler ha = (s, e) => a++;
		EventHandler hb = (s, e) => b++;
		CompositionTarget.Rendering += ha;
		CompositionTarget.Rendering += hb;
		Assert.True(GtkPump.Until(() => a >= 2 && b >= 2, 4000));
		CompositionTarget.Rendering -= ha;
		int bBefore = b;
		Assert.True(GtkPump.Until(() => b >= bBefore + 2, 4000));
		CompositionTarget.Rendering -= hb;
		int a2 = a;
		GtkPump.For(100);
		Assert.Equal(a2, a);
	});
}
