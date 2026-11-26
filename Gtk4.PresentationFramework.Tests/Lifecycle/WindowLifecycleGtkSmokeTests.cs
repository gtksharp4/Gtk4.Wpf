using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>
/// Real-GTK smoke tests: <see cref="WpfWindow.Show"/> drives SourceInitialized → Loaded →
/// ContentRendered from the native realize/map/frame-clock signals, live allocation feeds
/// ActualWidth/SizeChanged, and Close raises Unloaded after Closed.
/// </summary>
public class WindowLifecycleGtkSmokeTests : WpfGtkTestBase
{
	static (WpfWindow win, Button child, List<string> log) ShowWindow(string title)
	{
		var child = new Button { Content = "Hello" };
		var win = new WpfWindow { Title = title, Width = 320, Height = 200, Content = child };
		var log = new List<string>();
		win.SourceInitialized += (_, _) => log.Add("SourceInitialized");
		win.Activated += (_, _) => log.Add("Activated");
		win.Loaded += (_, _) => log.Add("Loaded:window");
		child.Loaded += (_, _) => log.Add("Loaded:child");
		win.ContentRendered += (_, _) => log.Add("ContentRendered");
		win.Show();
		return (win, child, log);
	}

	[Fact]
	public void Show_raises_SourceInitialized_Loaded_then_ContentRendered_once() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (win, _, log) = ShowWindow("LifecycleOrder");
		try
		{
			GtkState.Require(() => log.Contains("ContentRendered"),
				() => "ContentRendered (log: " + string.Join(",", log) + ")", win);
			GtkPump.For(100);

			log.RemoveAll(s => s == "Activated"); // WM-dependent; ordering checked separately
			Assert.Equal(new[] { "SourceInitialized", "Loaded:window", "Loaded:child", "ContentRendered" }, log);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Activated_when_raised_follows_SourceInitialized() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (win, _, log) = ShowWindow("LifecycleActivated");
		try
		{
			GtkPump.Until(() => log.Contains("ContentRendered") && log.Contains("Activated"));
			int act = log.IndexOf("Activated");
			if (act >= 0)
				Assert.True(act > log.IndexOf("SourceInitialized"), string.Join(",", log));
			Assert.Equal(0, log.IndexOf("SourceInitialized"));
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Native_window_resize_raises_SizeChanged() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (win, _, _) = ShowWindow("LifecycleResize");
		try
		{
			GtkState.Require(() => win.ActualWidth > 0, "window ActualWidth > 0", win);
			GtkPump.For(100);
			var events = new List<SizeChangedEventArgs>();
			win.SizeChanged += (_, e) => events.Add(e);
			double before = win.ActualWidth;

			var gtkWin = (Gtk.Window)win.Widget;
			gtkWin.SetSizeRequest(480, 260);
			gtkWin.SetDefaultSize(480, 260);

			Assert.True(GtkPump.Until(() => events.Count > 0), "no SizeChanged after resize");
			Assert.True(events[0].WidthChanged);
			Assert.Equal(before, events[0].PreviousSize.Width);
			Assert.Equal(GtkAllocationSync.BorderBoxSize(win.Widget).Width, win.ActualWidth);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Close_raises_Unloaded_after_Closed() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (win, child, log) = ShowWindow("LifecycleClose");
		GtkState.Require(() => win.IsLoaded && child.IsLoaded, "window and child Loaded", win, child);
		log.Clear();
		win.Closed += (_, _) => log.Add("Closed");
		win.Unloaded += (_, _) => log.Add("Unloaded:window");
		child.Unloaded += (_, _) => log.Add("Unloaded:child");

		win.Close();
		GtkPump.For(50);

		Assert.Equal(new[] { "Closed", "Unloaded:window", "Unloaded:child" }, log);
		Assert.False(win.IsLoaded);
		Assert.False(child.IsLoaded);
	});

	[Fact]
	public void Hiding_child_widget_raises_neither_Unloaded_nor_Loaded() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (win, child, _) = ShowWindow("LifecycleHide");
		try
		{
			GtkState.Require(() => child.IsLoaded, "child Loaded", win, child);
			int unloaded = 0, loaded = 0;
			child.Unloaded += (_, _) => unloaded++;
			child.Loaded += (_, _) => loaded++;

			child.Widget.Visible = false;
			GtkPump.For(100);
			child.Widget.Visible = true;
			GtkPump.For(100);

			Assert.Equal(0, unloaded);
			Assert.Equal(0, loaded);
			if (!child.IsLoaded)
				Assert.Fail("child no longer loaded after hide/show. " + GtkState.Describe(win, child));
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Content_swapped_after_load_gets_Loaded_and_old_gets_Unloaded() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (win, child, _) = ShowWindow("LifecycleSwap");
		try
		{
			GtkState.Require(() => child.IsLoaded, "child Loaded", win, child);
			var next = new Border();
			int oldUnloaded = 0;
			child.Unloaded += (_, _) => oldUnloaded++;

			win.Content = next;
			Assert.False(next.IsLoaded); // deferred at DispatcherPriority.Loaded

			GtkState.Require(() => next.IsLoaded && oldUnloaded == 1,
				() => $"new content Loaded and old Unloaded (oldUnloaded={oldUnloaded})", win, next);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Window_is_Initialized_before_Loaded() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow { Title = "LifecycleInit" };
		var log = new List<string>();
		win.Initialized += (_, _) => log.Add("Initialized");
		win.Loaded += (_, _) => log.Add("Loaded");
		win.Show();
		try
		{
			GtkState.Require(() => win.IsLoaded, "window Loaded", win);
			Assert.Equal(new[] { "Initialized", "Loaded" }, log);
			Assert.True(win.IsInitialized);
		}
		finally { win.Close(); }
	});
}
