using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class DispatcherOperationTests
{
	[Fact]
	public void BeginInvoke_Delegate_returns_a_completed_operation_when_run_inline()
	{
		int seen = 0;
		var op = Dispatcher.CurrentDispatcher.BeginInvoke(new Action<int>(x => seen = x), 7);
		Assert.Equal(DispatcherOperationStatus.Completed, op.Status);
		Assert.Equal(7, seen);
		Assert.Null(op.Result);
	}

	[Fact]
	public void Invoke_with_priority_passes_the_argument_to_a_DispatcherOperationCallback()
	{
		var result = Dispatcher.CurrentDispatcher.Invoke(
			DispatcherPriority.Send, (DispatcherOperationCallback)(a => (int)a + 1), 41);
		Assert.Equal(42, result);
	}

	[Fact]
	public void Invoke_priority_delegate_overload_runs_the_delegate()
	{
		bool ran = false;
		Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Send, new Action(() => ran = true));
		Assert.True(ran);
	}

	[Fact]
	public void Invoke_rejects_Inactive_and_Invalid_priorities()
	{
		var d = Dispatcher.CurrentDispatcher;
		Assert.Throws<InvalidEnumArgumentException>(() => d.Invoke(DispatcherPriority.Inactive, new Action(() => { })));
		Assert.Throws<InvalidEnumArgumentException>(() => d.Invoke(DispatcherPriority.Invalid, new Action(() => { })));
		Assert.Throws<ArgumentNullException>(() => d.Invoke(DispatcherPriority.Normal, null!));
	}

	[Fact]
	public void Delegate_exceptions_are_unwrapped_from_TargetInvocationException()
	{
		var pump = new DispatcherPump();
		try
		{
			Exception? seen = null;
			pump.Dispatcher.UnhandledException += (s, e) => { seen = e.Exception; e.Handled = true; };
			pump.Dispatcher.Invoke(DispatcherPriority.Send, new Func<int, int>(x => throw new FormatException("boom")), 5);
			Assert.IsType<FormatException>(seen);
		}
		finally { pump.Stop(); }
	}

	[Fact]
	public void Inactive_operation_stays_pending_until_its_priority_is_raised()
	{
		bool ran = false;
		var op = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Inactive, new Action(() => ran = true));
		Assert.Equal(DispatcherOperationStatus.Pending, op.Status);
		Assert.False(ran);
		op.Priority = DispatcherPriority.Normal;
		Assert.True(ran);
		Assert.Equal(DispatcherOperationStatus.Completed, op.Status);
	}

	[Fact]
	public void Abort_stops_a_pending_operation()
	{
		var d = Dispatcher.CurrentDispatcher;
		bool ran = false, aborted = false;
		var op = d.BeginInvoke(DispatcherPriority.Inactive, new Action(() => ran = true));
		op.Aborted += (s, e) => aborted = true;
		Assert.True(op.Abort());
		Assert.False(op.Abort());
		op.Priority = DispatcherPriority.Normal;
		Assert.False(ran);
		Assert.True(aborted);
		Assert.Equal(DispatcherOperationStatus.Aborted, op.Status);
		Assert.True(op.Task.IsCanceled);
		Assert.Equal(DispatcherOperationStatus.Aborted, op.Wait());
	}

	[Fact]
	public void Queued_operations_run_by_priority_then_fifo_on_the_dispatcher_thread()
	{
		var pump = new DispatcherPump();
		try
		{
			var order = new List<string>();
			int runThread = 0;
			using var gate = new ManualResetEventSlim();
			var first = pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => gate.Wait()));
			pump.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => order.Add("bg")));
			pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => order.Add("n1")));
			pump.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
			{
				order.Add("send");
				runThread = Thread.CurrentThread.ManagedThreadId;
			}));
			pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => order.Add("n2")));
			gate.Set();
			pump.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => order.Add("idle")));
			Assert.Equal(new[] { "send", "n1", "n2", "bg", "idle" }, order);
			Assert.Equal(pump.Thread.ManagedThreadId, runThread);
			Assert.Equal(DispatcherOperationStatus.Completed, first.Status);
		}
		finally { pump.Stop(); }
	}

	[Fact]
	public void Invoke_from_another_thread_waits_and_returns_the_result()
	{
		var pump = new DispatcherPump();
		try
		{
			var result = pump.Dispatcher.Invoke(DispatcherPriority.Send,
				(DispatcherOperationCallback)(a => Thread.CurrentThread.ManagedThreadId), null!);
			Assert.Equal(pump.Thread.ManagedThreadId, result);
			Assert.False(pump.Dispatcher.CheckAccess());
		}
		finally { pump.Stop(); }
	}

	[Fact]
	public void Operation_exposes_result_task_and_completed_event()
	{
		var pump = new DispatcherPump();
		try
		{
			bool completed = false;
			using var gate = new ManualResetEventSlim();
			pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => gate.Wait()));
			var op = pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)(a => "done"), null!);
			op.Completed += (s, e) => completed = true;
			Assert.Equal(DispatcherOperationStatus.Pending, op.Status);
			gate.Set();
			Assert.Equal(DispatcherOperationStatus.Completed, op.Wait());
			Assert.Equal("done", op.Result);
			Assert.True(op.Task.Wait(2000));
			Assert.True(completed);
		}
		finally { pump.Stop(); }
	}

	[Fact]
	public void Wait_with_a_timeout_reports_a_pending_operation()
	{
		var pump = new DispatcherPump();
		try
		{
			using var gate = new ManualResetEventSlim();
			pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => gate.Wait()));
			var op = pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { }));
			Assert.Equal(DispatcherOperationStatus.Pending, op.Wait(TimeSpan.FromMilliseconds(50)));
			gate.Set();
			Assert.Equal(DispatcherOperationStatus.Completed, op.Wait());
		}
		finally { pump.Stop(); }
	}

	[Fact]
	public void InvokeAsync_returns_an_operation_whose_task_yields_the_result()
	{
		var pump = new DispatcherPump();
		try
		{
			var op = pump.Dispatcher.InvokeAsync(() => 5);
			Assert.Equal(5, op.Task.Result);
			Assert.Equal(5, op.Result);
			pump.Dispatcher.InvokeAsync(new Action(() => { })).Wait();
		}
		finally { pump.Stop(); }
	}

	[Fact]
	public void BeginInvoke_Action_keeps_working_and_now_returns_the_operation()
	{
		bool ran = false;
		DispatcherOperation op = Dispatcher.CurrentDispatcher.BeginInvoke(() => { ran = true; });
		Assert.True(ran);
		Assert.Equal(DispatcherOperationStatus.Completed, op.Status);
	}
}
