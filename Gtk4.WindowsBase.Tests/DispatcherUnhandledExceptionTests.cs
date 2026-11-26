using System;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>Pure tests for <see cref="Dispatcher.UnhandledException"/>.</summary>
public class DispatcherUnhandledExceptionTests
{
	[Fact]
	public void No_handler_returns_false_and_BeginInvoke_rethrows()
	{
		var d = Dispatcher.CurrentDispatcher;
		var boom = new InvalidOperationException("boom");

		Assert.False(d.RaiseUnhandledException(new Exception()));
		var thrown = Assert.Throws<InvalidOperationException>(() => d.BeginInvoke(() => throw boom));
		Assert.Same(boom, thrown);
	}

	[Fact]
	public void Handler_receives_dispatcher_and_exception()
	{
		var d = Dispatcher.CurrentDispatcher;
		var ex = new Exception("x");
		DispatcherUnhandledExceptionEventArgs? seen = null;
		object? sender = null;
		DispatcherUnhandledExceptionEventHandler h = (s, e) => { sender = s; seen = e; };
		d.UnhandledException += h;
		try { d.RaiseUnhandledException(ex); }
		finally { d.UnhandledException -= h; }

		Assert.Same(d, sender);
		Assert.Same(d, seen!.Dispatcher);
		Assert.Same(ex, seen.Exception);
		Assert.False(seen.Handled);
	}

	[Fact]
	public void Handled_handler_makes_raise_true_and_BeginInvoke_swallow()
	{
		var d = Dispatcher.CurrentDispatcher;
		DispatcherUnhandledExceptionEventHandler h = (_, e) => e.Handled = true;
		d.UnhandledException += h;
		try
		{
			Assert.True(d.RaiseUnhandledException(new Exception()));
			d.BeginInvoke(() => throw new Exception("swallowed"));
		}
		finally { d.UnhandledException -= h; }
	}

	[Fact]
	public void Throwing_handler_is_not_swallowed()
	{
		var d = Dispatcher.CurrentDispatcher;
		DispatcherUnhandledExceptionEventHandler h = (_, _) => throw new ArgumentException("handler");
		d.UnhandledException += h;
		try
		{
			Assert.Throws<ArgumentException>(() => d.RaiseUnhandledException(new Exception()));
		}
		finally { d.UnhandledException -= h; }
	}

	[Fact]
	public void Unhandled_exception_is_tagged_so_the_glib_hook_skips_it()
	{
		var d = Dispatcher.CurrentDispatcher;
		var ex = new Exception();
		d.RaiseUnhandledException(ex);
		Assert.True(ex.Data.Contains(Dispatcher.ReportedKey));
	}

	[Fact]
	public void Timer_tick_exception_goes_through_the_dispatcher()
	{
		var d = Dispatcher.CurrentDispatcher;
		Exception? seen = null;
		DispatcherUnhandledExceptionEventHandler h = (_, e) => { seen = e.Exception; e.Handled = true; };
		d.UnhandledException += h;
		var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };
		var boom = new Exception("tick");
		timer.Tick += (_, _) => { timer.Stop(); throw boom; };
		try { timer.Start(); }
		finally { d.UnhandledException -= h; timer.Stop(); }

		Assert.Same(boom, seen);
	}
}
