using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class ApplicationUnhandledExceptionTests : WpfGtkTestBase
{
	static readonly string[] NoArgs = new string[0];

	[Fact]
	public void Exception_in_BeginInvoke_reaches_Dispatcher_then_Application_and_Handled_continues() => Run(() =>
	{
		var app = new Application();
		var log = new List<string>();
		var dispatcher = Dispatcher.CurrentDispatcher;
		DispatcherUnhandledExceptionEventHandler dh = (_, _) => log.Add("dispatcher");
		dispatcher.UnhandledException += dh;
		app.DispatcherUnhandledException += (s, e) =>
		{
			log.Add("app");
			Assert.Same(app, s);
			e.Handled = true;
		};
		bool continued = false;
		app.RunLoopOverride = _ =>
		{
			dispatcher.BeginInvoke(() => throw new InvalidOperationException("boom"));
			continued = true;
		};

		try { app.RunCore(NoArgs); }
		finally { dispatcher.UnhandledException -= dh; }

		Assert.Equal(new[] { "dispatcher", "app" }, log);
		Assert.True(continued);
	});

	[Fact]
	public void Unhandled_exception_is_rethrown_so_the_process_terminates_as_before() => Run(() =>
	{
		var app = new Application();
		int raised = 0;
		app.DispatcherUnhandledException += (_, _) => raised++;
		app.RunLoopOverride = _ =>
			Assert.Throws<InvalidOperationException>(
				() => Dispatcher.CurrentDispatcher.BeginInvoke(() => throw new InvalidOperationException()));

		app.RunCore(NoArgs);

		Assert.Equal(1, raised);
	});

	[Fact]
	public void Dispatcher_handler_marking_Handled_skips_the_Application_event() => Run(() =>
	{
		var app = new Application();
		var dispatcher = Dispatcher.CurrentDispatcher;
		DispatcherUnhandledExceptionEventHandler dh = (_, e) => e.Handled = true;
		dispatcher.UnhandledException += dh;
		int raised = 0;
		app.DispatcherUnhandledException += (_, _) => raised++;
		app.RunLoopOverride = _ => dispatcher.BeginInvoke(() => throw new Exception());

		try { app.RunCore(NoArgs); }
		finally { dispatcher.UnhandledException -= dh; }

		Assert.Equal(0, raised);
	});

	[Fact]
	public void Application_event_stops_after_Run_returns() => Run(() =>
	{
		var app = new Application { };
		app.RunLoopOverride = _ => { };
		app.RunCore(NoArgs);
		int raised = 0;
		app.DispatcherUnhandledException += (_, _) => { raised++; };

		Assert.Throws<Exception>(() => Dispatcher.CurrentDispatcher.BeginInvoke(() => throw new Exception()));

		Assert.Equal(0, raised);
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
