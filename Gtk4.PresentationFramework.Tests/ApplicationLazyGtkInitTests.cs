#pragma warning disable WPF0001 // experimental ThemeMode
using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK is initialised lazily and idempotently (a Window or MessageBox can precede Application.Run). The test
/// host has GTK up already, so the native init is replaced by a counting seam.
/// </summary>
public class ApplicationLazyGtkInitTests : WpfGtkTestBase, IDisposable
{
	readonly Action _original = Application._gtkInit;
	readonly Action<Action> _invoke = System.Windows.Threading.Dispatcher.InvokeAction;
	readonly Action<Action> _begin = System.Windows.Threading.Dispatcher.BeginInvokeAction;
	readonly Action<Action, System.Windows.Threading.DispatcherPriority>? _beginPri =
		System.Windows.Threading.Dispatcher.BeginInvokePriorityAction;
	readonly Func<bool> _isOn = System.Windows.Threading.Dispatcher.IsOnDispatcherThread;
	readonly Func<uint, Func<bool>, uint> _schedule = System.Windows.Threading.DispatcherTimer.ScheduleAction;
	readonly Action<uint> _remove = System.Windows.Threading.DispatcherTimer.RemoveSourceAction;
	readonly (bool, int) _scheduler = System.Windows.Media.Animation.AnimationScheduler.GtkStateForTests;

	public void Dispose()
	{
		// InitGtk turns on the animation scheduler's thread check; later tests tick clocks from the test thread.
		System.Windows.Media.Animation.AnimationScheduler.GtkStateForTests = _scheduler;
		Application._gtkInit = _original;
		Application.ResetCurrentForTests();
		// InitGtk wires process-wide dispatcher bridges: undo that, and mark GTK initialised as it really is.
		DispatcherGlibHost.Detach();
		System.Windows.Threading.Dispatcher.InvokeAction = _invoke;
		System.Windows.Threading.Dispatcher.BeginInvokeAction = _begin;
		System.Windows.Threading.Dispatcher.BeginInvokePriorityAction = _beginPri;
		System.Windows.Threading.Dispatcher.IsOnDispatcherThread = _isOn;
		System.Windows.Threading.DispatcherTimer.ScheduleAction = _schedule;
		System.Windows.Threading.DispatcherTimer.RemoveSourceAction = _remove;
		Application.MarkGtkInitialisedForTests();
	}

	[Fact]
	public void InitGtk_runs_the_native_init_once_however_often_it_is_called() => Run(() =>
	{
		int calls = 0;
		Application._gtkInit = () => calls++;
		Application.ResetGtkInitForTests();
		Application.InitGtk();
		Application.InitGtk();
		Application.EnsureGtkInitialised();
		Assert.Equal(1, calls);
	});

	[Fact]
	public void A_failed_init_is_retried_not_skipped() => Run(() =>
	{
		int calls = 0;
		Application._gtkInit = () =>
		{
			if (calls++ == 0) throw new DllNotFoundException("gtk");
		};
		Application.ResetGtkInitForTests();
		Assert.Throws<DllNotFoundException>(() => Application.InitGtk());
		Application.InitGtk();
		Application.InitGtk();
		Assert.Equal(2, calls);
	});

	[Fact]
	public void ThemeMode_set_before_init_is_applied_when_GTK_comes_up() => Run(() =>
	{
		var settings = Gtk.Settings.Default;
		bool hasScheme = ThemeModeGtk.HasColorScheme(settings);
		bool Dark() => hasScheme
			? settings.GtkInterfaceColorScheme == Gtk.InterfaceColorScheme.Dark
			: settings.GtkApplicationPreferDarkTheme;
		using var queue = new Lifecycle.ManualDispatcherQueue();
		var app = new Application { ThemeMode = ThemeMode.Dark };
		try
		{
			// Simulate GTK state predating the mode (set before any display), then init runs.
			ThemeModeGtk.Apply(ThemeMode.Light);
			Assert.False(Dark());
			Application._gtkInit = () => { };
			Application.ResetGtkInitForTests();
			Application.InitGtk();
			Assert.True(Dark());
		}
		finally { ThemeModeGtk.Apply(ThemeMode.System); }
	});
}
