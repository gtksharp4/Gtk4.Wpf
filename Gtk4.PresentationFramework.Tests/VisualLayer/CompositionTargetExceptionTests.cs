using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Threading;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.VisualLayer;

/// <summary>Handlers run one by one; a handled exception does not skip the handlers after it.</summary>
public class CompositionTargetExceptionTests : WpfGtkTestBase
{
	[Fact]
	public void A_handled_exception_does_not_skip_the_later_handlers_in_the_same_frame() => Run(() =>
	{
		var order = new List<string>();
		var seen = new List<Exception>();
		DispatcherUnhandledExceptionEventHandler onUnhandled = (s, e) => { seen.Add(e.Exception); e.Handled = true; };
		EventHandler bad = (s, e) => { order.Add("bad"); throw new InvalidOperationException("boom"); };
		EventHandler good = (s, e) => order.Add("good");
		var dispatcher = Dispatcher.CurrentDispatcher;
		dispatcher.UnhandledException += onUnhandled;
		CompositionTarget.Rendering += bad;
		CompositionTarget.Rendering += good;
		try { Assert.True(GtkPump.Until(() => order.Count >= 4, 4000)); }
		finally
		{
			CompositionTarget.Rendering -= bad;
			CompositionTarget.Rendering -= good;
			dispatcher.UnhandledException -= onUnhandled;
		}
		Assert.Equal(new[] { "bad", "good", "bad", "good" }, order.GetRange(0, 4));
		Assert.All(seen, ex => Assert.Equal("boom", ex.Message));
		Assert.NotEmpty(seen);
	});

	[Fact]
	public void The_frame_loop_keeps_ticking_after_a_handled_exception_and_stops_when_unsubscribed() => Run(() =>
	{
		int ticks = 0;
		DispatcherUnhandledExceptionEventHandler onUnhandled = (s, e) => e.Handled = true;
		EventHandler bad = (s, e) => { ticks++; throw new InvalidOperationException("boom"); };
		Dispatcher.CurrentDispatcher.UnhandledException += onUnhandled;
		CompositionTarget.Rendering += bad;
		try { Assert.True(GtkPump.Until(() => ticks >= 3, 4000)); }
		finally
		{
			CompositionTarget.Rendering -= bad;
			Dispatcher.CurrentDispatcher.UnhandledException -= onUnhandled;
		}
		int after = ticks;
		GtkPump.For(120);
		Assert.Equal(after, ticks);
	});
}
