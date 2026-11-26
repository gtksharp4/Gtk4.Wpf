using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Window registration, ShutdownMode policy and activation on real GTK windows.</summary>
public class ApplicationWindowsTests : WpfGtkTestBase, System.IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	static readonly string[] NoArgs = new string[0];

	[Fact]
	public void New_window_is_registered_and_the_first_becomes_MainWindow() => Run(() =>
	{
		var app = new Application();
		var w1 = new Window();
		var w2 = new Window();

		Assert.Equal(2, app.Windows.Count);
		Assert.Same(w1, app.Windows[0]);
		Assert.Same(w1, app.MainWindow);
		Assert.NotSame(w2, app.MainWindow);
	});

	[Fact]
	public void Window_leaves_Windows_after_Closed() => Run(() =>
	{
		var app = new Application();
		var w = new Window();
		int countInClosed = -1;
		w.Closed += (_, _) => countInClosed = app.Windows.Count;

		w.Close();

		Assert.Equal(1, countInClosed);
		Assert.Equal(0, app.Windows.Count);
		Assert.Null(app.MainWindow);
	});

	[Fact]
	public void Closing_a_window_while_not_running_never_shuts_down() => Run(() =>
	{
		var app = new RecordingApp();
		var w = new Window();

		w.Close();

		Assert.Equal(0, app.ExitRaised);
	});

	[Fact]
	public void OnLastWindowClose_shuts_down_only_when_the_last_window_closes() => Run(() =>
	{
		var app = new RecordingApp();
		int afterFirst = -1, afterSecond = -1;
		app.RunLoopOverride = a =>
		{
			var w1 = new Window();
			var w2 = new Window(); // created, never shown: still counts
			w1.Close();
			afterFirst = app.ExitRaised;
			w2.Close();
			afterSecond = app.ExitRaised;
		};

		app.RunCore(NoArgs);

		Assert.Equal(0, afterFirst);
		Assert.Equal(1, afterSecond);
	});

	[Fact]
	public void OnMainWindowClose_shuts_down_even_with_other_windows_open() => Run(() =>
	{
		var app = new RecordingApp { ShutdownMode = ShutdownMode.OnMainWindowClose };
		bool otherClosed = false;
		int afterOther = -1, afterMain = -1;
		app.RunLoopOverride = a =>
		{
			var main = new Window();
			var other = new Window();
			other.Closed += (_, _) => otherClosed = true;
			other.Close();
			afterOther = app.ExitRaised;
			var third = new Window();
			third.Closed += (_, _) => otherClosed = true;
			otherClosed = false;
			main.Close();
			afterMain = app.ExitRaised;
		};

		app.RunCore(NoArgs);

		Assert.Equal(0, afterOther);
		Assert.Equal(1, afterMain);
		Assert.True(otherClosed);
	});

	[Fact]
	public void OnExplicitShutdown_ignores_window_closes() => Run(() =>
	{
		var app = new RecordingApp { ShutdownMode = ShutdownMode.OnExplicitShutdown };
		int afterClose = -1, afterShutdown = -1;
		app.RunLoopOverride = a =>
		{
			var w1 = new Window();
			var w2 = new Window();
			w1.Close();
			w2.Close();
			afterClose = app.ExitRaised;
			app.Shutdown();
			afterShutdown = app.ExitRaised;
		};

		app.RunCore(NoArgs);

		Assert.Equal(0, afterClose);
		Assert.Equal(1, afterShutdown);
	});

	[Fact]
	public void ShutdownMode_defaults_to_OnLastWindowClose() => Run(() =>
		Assert.Equal(ShutdownMode.OnLastWindowClose, new Application().ShutdownMode));
}
