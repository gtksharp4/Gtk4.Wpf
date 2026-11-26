using System.Linq;
using System.Windows;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Application Activated/Deactivated follow the app, not individual windows (WPF WM_ACTIVATEAPP).</summary>
public class ApplicationActivationTests : WpfGtkTestBase, System.IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	[Fact]
	public void Application_activates_with_the_first_window_and_deactivates_with_the_last() => Run(() =>
	{
		using var queue = new ManualDispatcherQueue();
		var app = new Application();
		var w1 = new Window();
		var w2 = new Window();
		int activated = 0, deactivated = 0, windowActivated = 0;
		app.Activated += (_, _) => activated++;
		app.Deactivated += (_, _) => deactivated++;
		w1.Activated += (_, _) => windowActivated++;

		w1.NotifyActivation(true);
		w2.NotifyActivation(true);
		Assert.Equal(1, activated);
		Assert.Equal(1, windowActivated);

		w1.NotifyActivation(false);
		queue.Pump();
		Assert.Equal(0, deactivated);
		w2.NotifyActivation(false);
		queue.Pump();
		Assert.Equal(1, deactivated);
	});

	[Fact]
	public void Moving_focus_between_windows_raises_neither_event() => Run(() =>
	{
		using var queue = new ManualDispatcherQueue();
		var app = new Application();
		var w1 = new Window();
		var w2 = new Window();
		int activated = 0, deactivated = 0;
		app.Activated += (_, _) => activated++;
		app.Deactivated += (_, _) => deactivated++;

		w1.NotifyActivation(true);
		Assert.Equal(1, activated);

		// Window managers deactivate the old toplevel before activating the next.
		w1.NotifyActivation(false);
		w2.NotifyActivation(true);
		queue.Pump();
		Assert.Equal(1, activated);
		Assert.Equal(0, deactivated);

		w2.NotifyActivation(false);
		queue.Pump();
		Assert.Equal(1, deactivated);
		Assert.Equal(1, activated);
	});

	[Fact]
	public void Window_Deactivated_without_prior_activation_does_not_raise_app_Deactivated() => Run(() =>
	{
		using var queue = new ManualDispatcherQueue();
		var app = new Application();
		var w = new Window();
		int deactivated = 0;
		app.Deactivated += (_, _) => deactivated++;

		w.NotifyActivation(false);
		queue.Pump();

		Assert.Equal(0, deactivated);
	});

	[Fact]
	public void Window_created_after_Run_completed_does_not_drive_app_activation() => Run(() =>
	{
		using var queue = new ManualDispatcherQueue();
		var app = new Application { RunLoopOverride = _ => { } };
		app.RunCore(new string[0]);
		int activated = 0;
		app.Activated += (_, _) => activated++;
		var w = new Window();
		Assert.DoesNotContain(w, app.Windows.Cast<Window>());

		w.NotifyActivation(true);
		Assert.Equal(0, activated);

		w.NotifyActivation(false);
		queue.Pump();
	});
}
