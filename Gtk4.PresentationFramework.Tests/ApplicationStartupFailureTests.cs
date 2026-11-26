using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Startup exceptions reach the dispatcher event; StartupUri loads after any Startup window.</summary>
public class ApplicationStartupFailureTests : WpfGtkTestBase, IDisposable
{
	static readonly string[] NoArgs = new string[0];

	readonly List<(Dispatcher, DispatcherUnhandledExceptionEventHandler)> _handlers =
		new List<(Dispatcher, DispatcherUnhandledExceptionEventHandler)>();

	void Subscribe(Application app, DispatcherUnhandledExceptionEventHandler handler)
	{
		app.DispatcherUnhandledException += handler;
		_handlers.Add((app.Dispatcher, handler));
	}

	public void Dispose()
	{
		Application.ResetCurrentForTests();
		foreach (var (dispatcher, handler) in _handlers)
			dispatcher.UnhandledException -= handler;
	}

	[Fact]
	public void Handled_startup_exception_keeps_the_app_running_until_Shutdown() => Run(() =>
	{
		var app = new RecordingApp { RunLoopOverride = a => a.Shutdown(3) };
		app.Startup += (_, _) => throw new InvalidOperationException("startup");
		Exception? seen = null;
		Subscribe(app, (_, e) => { seen = e.Exception; e.Handled = true; });

		int code = app.RunCore(NoArgs);

		Assert.IsType<InvalidOperationException>(seen);
		Assert.Equal(3, code);
		Assert.Equal(1, app.ExitRaised);
	});

	[Fact]
	public void Unhandled_startup_exception_propagates_out_of_RunCore() => Run(() =>
	{
		var app = new RecordingApp { RunLoopOverride = _ => { } };
		app.Startup += (_, _) => throw new InvalidOperationException("startup");

		Assert.Throws<InvalidOperationException>(() => app.RunCore(NoArgs));
	});

	[Fact]
	public void Exception_from_showing_the_Run_window_is_routed_to_the_event() => Run(() =>
	{
		var app = new RecordingApp { RunLoopOverride = a => a.Shutdown(2) };
		var window = new ThrowingShowWindow();
		int raised = 0;
		Subscribe(app, (_, e) => { raised++; e.Handled = true; });

		Assert.Equal(2, app.RunCore(NoArgs, window));
		Assert.Equal(1, raised);
	});

	[Fact]
	public void StartupUri_window_is_still_created_when_Startup_created_a_splash_window() => Run(() =>
	{
		var app = new RecordingApp { StartupUri = new Uri("StartupProbeWindow.xaml", UriKind.Relative) };
		Window? splash = null;
		app.Startup += (_, _) => splash = new Window();
		Window? probe = null, main = null;
		var probeVisibility = Visibility.Collapsed;
		app.RunLoopOverride = a =>
		{
			probe = a.Windows.OfType<StartupProbeWindow>().SingleOrDefault();
			probeVisibility = probe?.Visibility ?? Visibility.Collapsed;
			main = a.MainWindow;
		};

		app.RunCore(NoArgs);

		Assert.NotNull(probe);
		Assert.Equal(Visibility.Visible, probeVisibility);
		Assert.Same(splash, main);
	});

	sealed class ThrowingShowWindow : Window
	{
		internal override void OnVisibilityChangedCore(Visibility visibility)
		{
			if (visibility == Visibility.Visible) throw new InvalidOperationException("show");
			base.OnVisibilityChangedCore(visibility);
		}
	}
}
