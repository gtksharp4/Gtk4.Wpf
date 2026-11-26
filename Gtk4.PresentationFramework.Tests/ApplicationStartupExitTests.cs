using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class ApplicationStartupExitTests : WpfGtkTestBase, System.IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	static readonly string[] NoArgs = new string[0];

	[Fact]
	public void OnStartup_runs_before_Startup_handlers_with_args() => Run(() =>
	{
		var app = new RecordingApp { RunLoopOverride = _ => { } };
		app.RunCore(new[] { "a", "b" });

		Assert.Equal(new[] { "override", "event:a,b" }, app.Log);
	});

	[Fact]
	public void Skipping_base_OnStartup_suppresses_Startup() => Run(() =>
	{
		var app = new RecordingApp { SkipBase = true, RunLoopOverride = _ => { } };
		app.RunCore(NoArgs);

		Assert.Equal(new[] { "override" }, app.Log);
	});

	[Fact]
	public void StartupUri_window_is_created_and_shown_after_Startup() => Run(() =>
	{
		var app = new RecordingApp { StartupUri = new Uri("StartupProbeWindow.xaml", UriKind.Relative) };
		bool mainNullDuringStartup = false;
		app.Startup += (_, _) => mainNullDuringStartup = app.MainWindow == null;
		Window? main = null;
		Visibility vis = Visibility.Collapsed;
		app.RunLoopOverride = a => { main = a.MainWindow; vis = main!.Visibility; };

		app.RunCore(NoArgs);

		Assert.True(mainNullDuringStartup);
		Assert.IsType<StartupProbeWindow>(main);
		Assert.Equal(Visibility.Visible, vis);
	});

	[Fact]
	public void Run_does_not_show_a_code_assigned_MainWindow() => Run(() =>
	{
		var app = new RecordingApp();
		var win = new Window();
		Visibility vis = Visibility.Visible;
		app.RunLoopOverride = _ => vis = win.Visibility;
		app.MainWindow = win;

		app.RunCore(NoArgs);

		Assert.NotEqual(Visibility.Visible, vis);
	});

	[Fact]
	public void Run_with_window_shows_it() => Run(() =>
	{
		var app = new RecordingApp();
		var win = new Window();
		Visibility vis = Visibility.Collapsed;
		app.RunLoopOverride = _ => vis = win.Visibility;

		app.RunCore(NoArgs, win);

		Assert.Equal(Visibility.Visible, vis);
	});

	[Fact]
	public void Shutdown_raises_Exit_once_with_the_code() => Run(() =>
	{
		var app = new RecordingApp { RunLoopOverride = a => a.Shutdown(3) };

		int result = app.RunCore(NoArgs);

		Assert.Equal(3, result);
		Assert.Equal(1, app.ExitRaised);
		Assert.Equal(3, app.ExitCodeSeen);
	});

	[Fact]
	public void Exit_handler_can_change_the_exit_code() => Run(() =>
	{
		var app = new RecordingApp { RunLoopOverride = a => a.Shutdown(3) };
		app.Exit += (_, e) => e.ApplicationExitCode = 5;

		Assert.Equal(5, app.RunCore(NoArgs));
	});

	[Fact]
	public void Second_Shutdown_is_a_no_op() => Run(() =>
	{
		var app = new RecordingApp { RunLoopOverride = a => { a.Shutdown(3); a.Shutdown(9); } };

		Assert.Equal(3, app.RunCore(NoArgs));
		Assert.Equal(1, app.ExitRaised);
	});

	[Fact]
	public void Shutdown_from_Exit_handler_does_not_reenter() => Run(() =>
	{
		var app = new RecordingApp { RunLoopOverride = a => a.Shutdown(1) };
		app.Exit += (s, _) => ((Application)s).Shutdown(7);

		Assert.Equal(1, app.RunCore(NoArgs));
		Assert.Equal(1, app.ExitRaised);
	});

	[Fact]
	public void Shutdown_inside_Startup_is_asynchronous_and_ends_the_loop() => Run(() =>
	{
		using var queue = new ManualDispatcherQueue();
		bool looped = false;
		var app = new RecordingApp { RunLoopOverride = _ => { looped = true; queue.Pump(); } };
		app.Startup += (_, _) => app.Shutdown(4);

		Assert.Equal(4, app.RunCore(NoArgs));
		Assert.True(looped);
		Assert.Equal(1, app.ExitRaised);
	});

	[Fact]
	public void Shutdown_inside_Startup_still_creates_then_closes_the_StartupUri_window() => Run(() =>
	{
		using var queue = new ManualDispatcherQueue();
		var app = new RecordingApp { StartupUri = new Uri("StartupProbeWindow.xaml", UriKind.Relative) };
		app.Startup += (_, _) => app.Shutdown(4);
		Window? during = null;
		app.RunLoopOverride = a => { during = a.MainWindow; queue.Pump(); };

		Assert.Equal(4, app.RunCore(NoArgs));
		Assert.IsType<StartupProbeWindow>(during);
		Assert.Empty(app.Windows);
		Assert.Equal(1, app.ExitRaised);
	});

	[Fact]
	public void Shutdown_before_Run_only_records_state_and_Run_honours_it() => Run(() =>
	{
		bool looped = false;
		var app = new RecordingApp { RunLoopOverride = _ => looped = true };

		app.Shutdown(2);
		Assert.Equal(0, app.ExitRaised);

		Assert.Equal(2, app.RunCore(NoArgs));
		Assert.False(looped);
		Assert.Equal(1, app.ExitRaised);
	});

	[Fact]
	public void Loop_returning_without_Shutdown_still_raises_Exit_once() => Run(() =>
	{
		var app = new RecordingApp { RunLoopOverride = _ => { } };

		Assert.Equal(0, app.RunCore(NoArgs));
		Assert.Equal(1, app.ExitRaised);
	});

	[Fact]
	public void FindResource_throws_for_a_missing_key_and_TryFindResource_returns_null() => Run(() =>
	{
		var app = new Application();

		Assert.Throws<InvalidOperationException>(() => app.FindResource("nope"));
		Assert.Null(app.TryFindResource("nope"));
		Assert.Throws<ArgumentNullException>(() => app.FindResource(null!));
	});

	[Fact]
	public void FindResource_resolves_Resources_then_theme_styles() => Run(() =>
	{
		var app = new Application();
		app.Resources["k"] = "v";

		Assert.Equal("v", app.FindResource("k"));
		Assert.IsType<Style>(app.FindResource(typeof(Button)));
	});

	[Fact]
	public void Properties_round_trips_and_is_synchronized() => Run(() =>
	{
		var app = new Application();
		app.Properties["x"] = 42;

		Assert.Equal(42, app.Properties["x"]);
		Assert.True(((System.Collections.ICollection)app.Properties).IsSynchronized);
	});
}
