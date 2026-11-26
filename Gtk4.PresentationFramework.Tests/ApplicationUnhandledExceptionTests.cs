using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

// The app event forwards to the shared fixture-thread Dispatcher, so every handler is removed in Dispose.
public class ApplicationUnhandledExceptionTests : WpfGtkTestBase, IDisposable
{
	static readonly string[] NoArgs = new string[0];

	readonly List<(Dispatcher, DispatcherUnhandledExceptionEventHandler)> _subscribed =
		new List<(Dispatcher, DispatcherUnhandledExceptionEventHandler)>();

	void Subscribe(Application app, DispatcherUnhandledExceptionEventHandler handler)
	{
		app.DispatcherUnhandledException += handler;
		_subscribed.Add((app.Dispatcher, handler));
	}

	public void Dispose()
	{
		Application.ResetCurrentForTests();
		foreach (var (dispatcher, handler) in _subscribed)
			dispatcher.UnhandledException -= handler;
		_subscribed.Clear();
	}

	[Fact]
	public void Exception_in_BeginInvoke_reaches_handlers_in_subscription_order_and_Handled_continues() => Run(() =>
	{
		var app = new Application();
		var log = new List<string>();
		var dispatcher = Dispatcher.CurrentDispatcher;
		DispatcherUnhandledExceptionEventHandler dh = (_, _) => log.Add("dispatcher");
		dispatcher.UnhandledException += dh;
		_subscribed.Add((dispatcher, dh));
		Subscribe(app, (s, e) =>
		{
			log.Add("app");
			Assert.Same(app.Dispatcher, s);
			e.Handled = true;
		});
		bool continued = false;
		app.RunLoopOverride = _ =>
		{
			dispatcher.BeginInvoke(() => throw new InvalidOperationException("boom"));
			continued = true;
		};

		app.RunCore(NoArgs);

		Assert.Equal(new[] { "dispatcher", "app" }, log);
		Assert.True(continued);
	});

	[Fact]
	public void Unhandled_exception_is_rethrown_so_the_process_terminates_as_before() => Run(() =>
	{
		var app = new Application();
		int raised = 0;
		Subscribe(app, (_, _) => raised++);
		app.RunLoopOverride = _ =>
			Assert.Throws<InvalidOperationException>(
				() => Dispatcher.CurrentDispatcher.BeginInvoke(() => throw new InvalidOperationException()));

		app.RunCore(NoArgs);

		Assert.Equal(1, raised);
	});

	[Fact]
	public void Application_handler_still_runs_after_a_Dispatcher_handler_marks_Handled() => Run(() =>
	{
		var app = new Application();
		var dispatcher = Dispatcher.CurrentDispatcher;
		DispatcherUnhandledExceptionEventHandler dh = (_, e) => e.Handled = true;
		dispatcher.UnhandledException += dh;
		_subscribed.Add((dispatcher, dh));
		int raised = 0;
		Subscribe(app, (_, _) => raised++);
		app.RunLoopOverride = _ => dispatcher.BeginInvoke(() => throw new Exception());

		app.RunCore(NoArgs);

		Assert.Equal(1, raised);
	});

	[Fact]
	public void Application_handler_subscribed_outside_Run_receives_the_exception() => Run(() =>
	{
		var app = new Application();
		Exception? seen = null;
		Subscribe(app, (_, e) => { seen = e.Exception; e.Handled = true; });
		var boom = new InvalidOperationException("queued");
		using var queue = new Lifecycle.ManualDispatcherQueue();
		Dispatcher.CurrentDispatcher.BeginInvoke(() => throw boom);
		Assert.Null(seen);
		queue.Pump();
		Assert.Same(boom, seen);
	});

	[Fact]
	public void Glib_hook_routes_handled_exceptions_and_keeps_running() => Run(() =>
	{
		var dispatcher = Dispatcher.CurrentDispatcher;
		DispatcherUnhandledExceptionEventHandler dh = (_, e) => e.Handled = true;
		dispatcher.UnhandledException += dh;
		try
		{
			var args = new GLib.UnhandledExceptionArgs(new Exception("g"), false);
			Application.OnGlibUnhandled(args);
			Assert.False(args.ExitApplication);
		}
		finally { dispatcher.UnhandledException -= dh; }
	});

	[Fact]
	public void Glib_hook_unwraps_the_TargetInvocationException_of_the_callback_marshaller() => Run(() =>
	{
		var dispatcher = Dispatcher.CurrentDispatcher;
		Exception? recorded = null;
		DispatcherUnhandledExceptionEventHandler dh = (_, e) => { recorded = e.Exception; e.Handled = true; };
		dispatcher.UnhandledException += dh;
		try
		{
			var wrapped = new System.Reflection.TargetInvocationException(new ArgumentOutOfRangeException());
			var args = new GLib.UnhandledExceptionArgs(wrapped, false);
			Application.OnGlibUnhandled(args);
			Assert.IsType<ArgumentOutOfRangeException>(recorded);
			Assert.False(args.ExitApplication);
		}
		finally { dispatcher.UnhandledException -= dh; }
	});

	[Fact]
	public void Glib_hook_terminates_when_unhandled_and_skips_already_reported() => Run(() =>
	{
		var unhandled = new GLib.UnhandledExceptionArgs(new Exception("g"), false);
		Application.OnGlibUnhandled(unhandled);
		Assert.True(unhandled.ExitApplication);

		var reported = new Exception("r");
		reported.Data[Dispatcher.ReportedKey] = true;
		int raised = 0;
		DispatcherUnhandledExceptionEventHandler dh = (_, _) => raised++;
		Dispatcher.CurrentDispatcher.UnhandledException += dh;
		try { Application.OnGlibUnhandled(new GLib.UnhandledExceptionArgs(reported, false)); }
		finally { Dispatcher.CurrentDispatcher.UnhandledException -= dh; }
		Assert.Equal(0, raised);
	});

	[Fact]
	public void Raw_glib_idle_exception_is_routed_through_ExceptionManager() => Run(() =>
	{
		var dispatcher = Dispatcher.CurrentDispatcher;
		Exception? seen = null;
		DispatcherUnhandledExceptionEventHandler dh = (_, e) => { seen = e.Exception; e.Handled = true; };
		dispatcher.UnhandledException += dh;
		Application.EnsureGlibExceptionHook();
		try
		{
			var boom = new Exception("idle");
			GLib.Idle.Add(() => { throw boom; });
			for (int i = 0; i < 50 && seen == null; i++)
				Gtk.Application.RunIteration(false);
			Assert.Same(boom, seen);
		}
		finally { dispatcher.UnhandledException -= dh; }
	});
}
