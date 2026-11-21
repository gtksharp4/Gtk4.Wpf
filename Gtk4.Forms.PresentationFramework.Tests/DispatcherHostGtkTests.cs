using System;
using System.Windows;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>The GTK thread's dispatcher runs nested GLib frames and exposes itself on every element.</summary>
public class DispatcherHostGtkTests : WpfGtkTestBase
{
	[Fact]
	public void Run_on_the_host_dispatcher_is_a_nested_glib_loop_ended_by_ExitAllFrames() => Run(() =>
	{
		DispatcherGlibHost.Attach();
		try
		{
			bool inside = false;
			GLib.Idle.Add((int)GLib.Priority.Default, () =>
			{
				inside = true;
				Dispatcher.ExitAllFrames();
				return false;
			});
			Dispatcher.Run();
			Assert.True(inside);
		}
		finally { DispatcherGlibHost.Detach(); }
	});

	[Fact]
	public void PushFrame_returns_when_the_frame_stops() => Run(() =>
	{
		DispatcherGlibHost.Attach();
		try
		{
			var frame = new DispatcherFrame();
			GLib.Idle.Add((int)GLib.Priority.Default, () => { frame.Continue = false; return false; });
			Dispatcher.PushFrame(frame);
			Assert.False(frame.Continue);
		}
		finally { DispatcherGlibHost.Detach(); }
	});

	[Fact]
	public void A_stopped_frame_does_not_block_a_later_push() => Run(() =>
	{
		DispatcherGlibHost.Attach();
		try
		{
			var frame = new DispatcherFrame { Continue = false };
			Dispatcher.PushFrame(frame);
		}
		finally { DispatcherGlibHost.Detach(); }
	});

	[Fact]
	public void Elements_expose_the_dispatcher_of_the_gtk_thread() => Run(() =>
	{
		var button = new System.Windows.Controls.Button();
		Assert.Same(Dispatcher.CurrentDispatcher, button.Dispatcher);
		Assert.True(button.CheckAccess());
		button.VerifyAccess();
	});

	[Fact]
	public void Worker_thread_invoke_runs_on_the_gtk_thread_and_unwraps_exceptions() => Run(() =>
	{
		var ui = Dispatcher.CurrentDispatcher;
		DispatcherGlibHost.Attach();
		var oldInvoke = Dispatcher.InvokeAction;
		var oldPost = Dispatcher.BeginInvokePriorityAction;
		var oldOn = Dispatcher.IsOnDispatcherThread;
		try
		{
			int uiThread = Environment.CurrentManagedThreadId;
			Dispatcher.IsOnDispatcherThread = () => Environment.CurrentManagedThreadId == uiThread;
			Dispatcher.BeginInvokePriorityAction = (a, p) => GLib.Idle.Add((int)GLib.Priority.Default, () => { a(); return false; });
			object? result = null;
			var worker = new System.Threading.Thread(() =>
				result = ui.Invoke(DispatcherPriority.Send, (DispatcherOperationCallback)(a => Environment.CurrentManagedThreadId), null!));
			worker.Start();
			var deadline = DateTime.UtcNow.AddSeconds(5);
			while (worker.IsAlive && DateTime.UtcNow < deadline)
				Gtk.Application.RunIteration(false);
			worker.Join();
			Assert.Equal(uiThread, result);
		}
		finally
		{
			Dispatcher.IsOnDispatcherThread = oldOn;
			Dispatcher.BeginInvokePriorityAction = oldPost;
			Dispatcher.InvokeAction = oldInvoke;
			DispatcherGlibHost.Detach();
		}
	});
}
