using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>Real-GTK tests: BeginInvoke priorities reach the GLib main loop.</summary>
public class DispatcherPriorityGtkTests : WpfGtkTestBase
{
	[Fact]
	public void Burst_runs_in_WPF_priority_order() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var order = new List<DispatcherPriority>();
		var d = Dispatcher.CurrentDispatcher;
		var posted = new[]
		{
			DispatcherPriority.SystemIdle, DispatcherPriority.Background, DispatcherPriority.Loaded,
			DispatcherPriority.Normal, DispatcherPriority.Input, DispatcherPriority.Send,
			DispatcherPriority.Render, DispatcherPriority.DataBind, DispatcherPriority.ContextIdle,
			DispatcherPriority.ApplicationIdle,
		};
		foreach (var p in posted)
		{
			var captured = p;
			d.BeginInvoke(() => order.Add(captured), captured);
		}

		Assert.True(GtkPump.Until(() => order.Count == posted.Length));
		var expected = new List<DispatcherPriority>(posted);
		expected.Sort((a, b) => b.CompareTo(a));
		Assert.Equal(expected, order);
	});

	[Fact]
	public void Loaded_runs_while_a_default_priority_source_is_busy() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		bool ran = false;
		// A busy source at G_PRIORITY_DEFAULT: anything posted at an idle priority starves behind it.
		uint busy = GLib.Idle.Add(GLib.Priority.Default, () => true);
		try
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(() => ran = true, DispatcherPriority.Loaded);
			Assert.True(GtkPump.Until(() => ran, 2000));
		}
		finally { GLib.Source.Remove(busy); }
	});

	[Fact]
	public void Idle_priority_work_yields_to_a_busy_default_source() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		bool ran = false;
		int spins = 0;
		uint busy = GLib.Idle.Add(GLib.Priority.Default, () => { spins++; return spins < 50; });
		Dispatcher.CurrentDispatcher.BeginInvoke(() => ran = ran || spins >= 50, DispatcherPriority.Background);
		Assert.True(GtkPump.Until(() => ran, 2000));
		Assert.True(spins >= 50);
	});
}
