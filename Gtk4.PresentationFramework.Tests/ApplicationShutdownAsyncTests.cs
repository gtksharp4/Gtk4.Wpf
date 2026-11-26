using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Shutdown is posted to the dispatcher (WPF); Run is single-shot; a finished app stops tracking windows.</summary>
public class ApplicationShutdownAsyncTests : WpfGtkTestBase, System.IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	static readonly string[] NoArgs = new string[0];

	[Fact]
	public void Shutdown_while_running_is_deferred_until_the_dispatcher_drains() => Run(() =>
	{
		using var queue = new ManualDispatcherQueue();
		var app = new RecordingApp();
		int exitBefore = -1, windowsBefore = -1;
		app.RunLoopOverride = a =>
		{
			var w = new Window();
			a.Shutdown(5);
			a.Shutdown(9); // ignored: the first request wins
			exitBefore = app.ExitRaised;
			windowsBefore = a.Windows.Count;
			queue.Pump();
		};

		int code = app.RunCore(NoArgs);

		Assert.Equal(0, exitBefore);
		Assert.Equal(1, windowsBefore);
		Assert.Equal(5, code);
		Assert.Equal(1, app.ExitRaised);
		Assert.Equal(5, app.ExitCodeSeen);
		Assert.Equal(0, app.Windows.Count);
	});

	[Fact]
	public void Posted_shutdown_still_completes_when_the_loop_returns_without_draining() => Run(() =>
	{
		using var queue = new ManualDispatcherQueue();
		var app = new RecordingApp { RunLoopOverride = a => a.Shutdown(4) };

		Assert.Equal(4, app.RunCore(NoArgs));
		Assert.Equal(1, app.ExitRaised);
	});

	[Fact]
	public void Shutdown_from_another_thread_throws() => Run(() =>
	{
		var app = new Application();

		var ex = Task.Run(() => Record.Exception(() => app.Shutdown())).GetAwaiter().GetResult();

		Assert.IsType<InvalidOperationException>(ex);
	});

	[Fact]
	public void Run_cannot_be_called_twice() => Run(() =>
	{
		var app = new RecordingApp { RunLoopOverride = _ => { } };
		app.RunCore(NoArgs);

		Assert.Throws<InvalidOperationException>(() => app.RunCore(NoArgs));
	});

	[Fact]
	public void Run_window_keeps_an_existing_MainWindow_and_adds_the_window() => Run(() =>
	{
		var app = new Application();
		var first = new Window();
		var other = new Window();

		app.PrepareMainWindow(other);

		Assert.Same(first, app.MainWindow);
		Assert.Contains(other, app.Windows.OfType<Window>());
	});

	[Fact]
	public void Run_window_adds_a_window_created_before_the_application() => Run(() =>
	{
		Application.ResetCurrentForTests();
		var early = new Window();
		var app = new Application();
		Assert.Equal(0, app.Windows.Count);

		app.PrepareMainWindow(early);

		Assert.Same(early, app.MainWindow);
		Assert.Equal(1, app.Windows.Count);
	});

	[Fact]
	public void Windows_created_after_Run_completes_are_not_tracked() => Run(() =>
	{
		var app = new RecordingApp { RunLoopOverride = _ => { } };
		app.RunCore(NoArgs);

		var late = new Window();

		Assert.Equal(0, app.Windows.Count);
		Assert.NotSame(late, app.MainWindow);
	});
}
