using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>
/// Headless tests for the <see cref="WpfWindow"/> lifecycle ordering, driven through the
/// internal <c>RaiseLifecycleForTest</c> seam (no realized GTK window needed).
/// </summary>
public class WindowLifecycleSeamTests
{
	sealed class OverrideWindow : WpfWindow
	{
		public readonly List<string> Log = new List<string>();

		protected override void OnSourceInitialized(EventArgs e)
		{
			Log.Add("OnSourceInitialized");
			base.OnSourceInitialized(e);
		}

		protected override void OnContentRendered(EventArgs e)
		{
			Log.Add("OnContentRendered");
			base.OnContentRendered(e);
		}
	}

	static List<string> Wire(WpfWindow win, Border child)
	{
		var log = new List<string>();
		win.SourceInitialized += (_, _) => log.Add("SourceInitialized");
		win.Loaded += (_, _) => log.Add("Loaded:window");
		child.Loaded += (_, _) => log.Add("Loaded:child");
		win.ContentRendered += (_, _) => log.Add("ContentRendered");
		win.Unloaded += (_, _) => log.Add("Unloaded:window");
		child.Unloaded += (_, _) => log.Add("Unloaded:child");
		win.Closed += (_, _) => log.Add("Closed");
		return log;
	}

	[Fact]
	public void Lifecycle_order_is_SourceInitialized_Loaded_pre_order_then_ContentRendered()
	{
		using var queue = new ManualDispatcherQueue();
		var child = new Border();
		var win = new WpfWindow { Content = child };
		var log = Wire(win, child);

		win.RaiseLifecycleForTest();
		queue.Pump();

		Assert.Equal(new[] { "SourceInitialized", "Loaded:window", "Loaded:child", "ContentRendered" }, log);
		Assert.True(win.IsLoaded);
		Assert.True(child.IsLoaded);
	}

	[Fact]
	public void SourceInitialized_and_ContentRendered_fire_only_once()
	{
		using var queue = new ManualDispatcherQueue();
		var child = new Border();
		var win = new WpfWindow { Content = child };
		var log = Wire(win, child);

		win.RaiseLifecycleForTest();
		queue.Pump();
		win.RaiseLifecycleForTest();
		queue.Pump();

		Assert.Equal(1, log.FindAll(s => s == "SourceInitialized").Count);
		Assert.Equal(1, log.FindAll(s => s == "ContentRendered").Count);
		Assert.Equal(1, log.FindAll(s => s == "Loaded:window").Count);
	}

	[Fact]
	public void Protected_virtual_overrides_run()
	{
		using var queue = new ManualDispatcherQueue();
		var win = new OverrideWindow { Content = new Border() };
		int src = 0, rendered = 0;
		win.SourceInitialized += (_, _) => src++;
		win.ContentRendered += (_, _) => rendered++;

		win.RaiseLifecycleForTest();
		queue.Pump();

		Assert.Equal(new[] { "OnSourceInitialized", "OnContentRendered" }, win.Log);
		Assert.Equal(1, src);
		Assert.Equal(1, rendered);
	}

	[Fact]
	public void Closed_lifecycle_raises_Unloaded_pre_order()
	{
		using var queue = new ManualDispatcherQueue();
		var child = new Border();
		var win = new WpfWindow { Content = child };
		win.RaiseLifecycleForTest();
		queue.Pump();
		var log = Wire(win, child);

		win.RaiseClosedLifecycleForTest();

		Assert.Equal(new[] { "Unloaded:window", "Unloaded:child" }, log);
		Assert.False(win.IsLoaded);
	}

	[Fact]
	public void ContentRendered_is_not_raised_when_Content_was_never_set()
	{
		// WPF arms ContentRendered from Window.OnContentChanged only.
		using var queue = new ManualDispatcherQueue();
		var win = new WpfWindow();
		int rendered = 0;
		win.ContentRendered += (_, _) => rendered++;

		win.RaiseLifecycleForTest();
		queue.Pump();

		Assert.True(win.IsLoaded);
		Assert.Equal(0, rendered);
	}

	[Fact]
	public void ContentRendered_is_raised_again_after_each_Content_change_on_a_loaded_window()
	{
		using var queue = new ManualDispatcherQueue();
		var win = new WpfWindow { Content = new Border() };
		int rendered = 0;
		win.ContentRendered += (_, _) => rendered++;
		win.RaiseLifecycleForTest();
		queue.Pump();
		Assert.Equal(1, rendered);

		win.Content = new Border();
		Assert.Equal(1, rendered); // deferred, as WPF's PostContentRendered
		queue.Pump();
		Assert.Equal(2, rendered);

		win.Content = null;
		queue.Pump();
		Assert.Equal(3, rendered);
	}

	[Fact]
	public void Content_set_after_load_on_a_window_that_had_none_raises_ContentRendered()
	{
		using var queue = new ManualDispatcherQueue();
		var win = new WpfWindow();
		int rendered = 0;
		win.ContentRendered += (_, _) => rendered++;
		win.RaiseLifecycleForTest();
		queue.Pump();

		win.Content = new Border();
		queue.Pump();

		Assert.Equal(1, rendered);
	}
}
