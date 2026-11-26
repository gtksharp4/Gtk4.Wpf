using System;
using System.Threading;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>
/// Pure tests for <see cref="Dispatcher"/> and <see cref="DispatcherPriority"/>.
/// No GTK loop required.
/// </summary>
public class DispatcherTests
{
	// ── Dispatcher ───────────────────────────────────────────────────────────────

	[Fact]
	public void CurrentDispatcher_returns_a_Dispatcher()
	{
		var d = Dispatcher.CurrentDispatcher;
		Assert.NotNull(d);
	}

	[Fact]
	public void CurrentDispatcher_stable_on_same_thread()
	{
		var d1 = Dispatcher.CurrentDispatcher;
		var d2 = Dispatcher.CurrentDispatcher;
		Assert.Same(d1, d2);
	}

	[Fact]
	public void Invoke_runs_the_action()
	{
		bool ran = false;
		Dispatcher.CurrentDispatcher.Invoke(() => { ran = true; });
		Assert.True(ran);
	}

	[Fact]
	public void Invoke_T_returns_value()
	{
		var result = Dispatcher.CurrentDispatcher.Invoke(() => 42);
		Assert.Equal(42, result);
	}

	[Fact]
	public void BeginInvoke_runs_the_action()
	{
		bool ran = false;
		Dispatcher.CurrentDispatcher.BeginInvoke(() => { ran = true; });
		Assert.True(ran);
	}

	[Fact]
	public void CheckAccess_true_on_dispatcher_thread()
	{
		// Default IsOnDispatcherThread always returns true.
		Assert.True(Dispatcher.CurrentDispatcher.CheckAccess());
	}

	[Fact]
	public void VerifyAccess_does_not_throw_on_dispatcher_thread()
	{
		var ex = Record.Exception(() => Dispatcher.CurrentDispatcher.VerifyAccess());
		Assert.Null(ex);
	}

	[Fact]
	public void VerifyAccess_throws_when_IsOnDispatcherThread_is_false()
	{
		var saved = Dispatcher.IsOnDispatcherThread;
		try
		{
			Dispatcher.IsOnDispatcherThread = () => false;
			Assert.Throws<InvalidOperationException>(
				() => Dispatcher.CurrentDispatcher.VerifyAccess());
		}
		finally { Dispatcher.IsOnDispatcherThread = saved; }
	}

	[Fact]
	public void Thread_property_is_the_creating_thread()
	{
		Thread? captured = null;
		Thread t = new Thread(() => { captured = Dispatcher.CurrentDispatcher.Thread; });
		t.Start();
		t.Join();
		Assert.Same(t, captured);
	}

	// ── DispatcherPriority ───────────────────────────────────────────────────────

	[Fact]
	public void DispatcherPriority_Normal_has_expected_value()
	{
		Assert.Equal(9, (int)DispatcherPriority.Normal);
	}

	[Fact]
	public void DispatcherPriority_values_are_ordered()
	{
		Assert.True(DispatcherPriority.Background < DispatcherPriority.Normal);
		Assert.True(DispatcherPriority.Normal < DispatcherPriority.Send);
	}
}