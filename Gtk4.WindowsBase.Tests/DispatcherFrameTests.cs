using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class DispatcherFrameTests
{
	[Fact]
	public void CurrentDispatcher_is_per_thread_and_FromThread_finds_it()
	{
		var mine = Dispatcher.CurrentDispatcher;
		Assert.Same(mine, Dispatcher.FromThread(Thread.CurrentThread));
		Dispatcher? other = null;
		Thread? t = null;
		t = new Thread(() => other = Dispatcher.CurrentDispatcher);
		t.Start();
		t.Join();
		Assert.NotNull(other);
		Assert.NotSame(mine, other);
		Assert.Same(t, other!.Thread);
		Assert.Same(other, Dispatcher.FromThread(t));
	}

	[Fact]
	public void FromThread_is_null_for_a_thread_that_never_asked_for_a_dispatcher()
	{
		var t = new Thread(() => { });
		Assert.Null(Dispatcher.FromThread(t));
		Assert.Null(Dispatcher.FromThread(null!));
	}

	[Fact]
	public void DependencyObject_is_a_DispatcherObject_bound_to_its_creating_thread()
	{
		var pump = new DispatcherPump();
		try
		{
			var obj = (DependencyObject)pump.Dispatcher.Invoke(
				DispatcherPriority.Send, (DispatcherOperationCallback)(a => new DependencyObject()), null!)!;
			DispatcherObject asDispatcherObject = obj;
			Assert.Same(pump.Dispatcher, asDispatcherObject.Dispatcher);
			Assert.False(obj.CheckAccess());
			Assert.Throws<InvalidOperationException>(() => obj.VerifyAccess());
			Assert.Same(Dispatcher.CurrentDispatcher, new DependencyObject().Dispatcher);
		}
		finally { pump.Stop(); }
	}

	[Fact]
	public void PushFrame_runs_a_nested_loop_until_the_frame_stops()
	{
		var pump = new DispatcherPump();
		try
		{
			var order = new List<string>();
			pump.Dispatcher.Invoke(DispatcherPriority.Send, new Action(() =>
			{
				var frame = new DispatcherFrame();
				pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
				{
					order.Add("inner");
					frame.Continue = false;
				}));
				order.Add("before");
				Dispatcher.PushFrame(frame);
				order.Add("after");
			}));
			Assert.Equal(new[] { "before", "inner", "after" }, order);
			Assert.True(pump.Thread.IsAlive);
		}
		finally { pump.Stop(); }
	}

	[Fact]
	public void ExitAllFrames_ends_nested_frames_and_Run_but_not_the_dispatcher()
	{
		var pump = new DispatcherPump();
		pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Dispatcher.ExitAllFrames));
		Assert.True(pump.Thread.Join(5000));
		Assert.Null(pump.RunException);
		Assert.False(pump.Dispatcher.HasShutdownStarted);
	}

	[Fact]
	public void InvokeShutdown_raises_events_in_order_and_ends_Run()
	{
		var pump = new DispatcherPump();
		var order = new List<string>();
		pump.Dispatcher.ShutdownStarted += (s, e) => order.Add("started");
		pump.Dispatcher.ShutdownFinished += (s, e) => order.Add("finished");
		Assert.False(pump.Dispatcher.HasShutdownStarted);
		pump.Dispatcher.InvokeShutdown();
		Assert.True(pump.Thread.Join(5000));
		Assert.Equal(new[] { "started", "finished" }, order);
		Assert.True(pump.Dispatcher.HasShutdownStarted);
		Assert.True(pump.Dispatcher.HasShutdownFinished);
	}

	[Fact]
	public void BeginInvokeShutdown_honours_priority_and_aborts_pending_work()
	{
		var pump = new DispatcherPump();
		bool ranAfter = false;
		using var gate = new ManualResetEventSlim();
		pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => gate.Wait()));
		pump.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
		var late = pump.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => ranAfter = true));
		gate.Set();
		Assert.True(pump.Thread.Join(5000));
		Assert.False(ranAfter);
		Assert.Equal(DispatcherOperationStatus.Aborted, late.Status);
		var afterShutdown = pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { }));
		Assert.Equal(DispatcherOperationStatus.Aborted, afterShutdown.Status);
		Assert.Null(pump.Dispatcher.Invoke(DispatcherPriority.Send, new Action(() => { })));
	}

	[Fact]
	public void Unhandled_exception_escapes_Run_on_a_secondary_dispatcher()
	{
		var pump = new DispatcherPump();
		pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => throw new InvalidTimeZoneException("x")));
		Assert.True(pump.Thread.Join(5000));
		Assert.IsType<InvalidTimeZoneException>(pump.RunException);
	}

	[Fact]
	public void PushFrame_rejects_a_null_frame()
	{
		Assert.Throws<ArgumentNullException>(() => Dispatcher.PushFrame(null!));
	}

	[Fact]
	public void Frame_with_ExitWhenRequested_stops_on_ExitAllFrames_only_when_requested()
	{
		var pump = new DispatcherPump();
		try
		{
			bool? stubborn = null;
			pump.Dispatcher.Invoke(DispatcherPriority.Send, new Action(() =>
			{
				var frame = new DispatcherFrame(false);
				pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
				{
					Dispatcher.ExitAllFrames();
					stubborn = frame.Continue;
					frame.Continue = false;
				}));
				Dispatcher.PushFrame(frame);
			}));
			Assert.True(stubborn);
		}
		finally { pump.Stop(); }
	}
}
