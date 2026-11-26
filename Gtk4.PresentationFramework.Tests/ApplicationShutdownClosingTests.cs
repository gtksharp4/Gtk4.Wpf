using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Shutdown raises Closing on each window but ignores its Cancel (WPF InternalClose with ignoreCancel).</summary>
public class ApplicationShutdownClosingTests : WpfGtkTestBase, System.IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	static readonly string[] NoArgs = new string[0];

	[Fact]
	public void Shutdown_closes_remaining_windows_despite_a_Closing_veto() => Run(() =>
	{
		var app = new RecordingApp();
		var win = new Window();
		int closing = 0;
		win.Closing += (_, e) => { closing++; e.Cancel = true; };
		bool closed = false;
		win.Closed += (_, _) => closed = true;
		app.RunLoopOverride = a => a.Shutdown();

		app.RunCore(NoArgs);

		Assert.Equal(1, closing);
		Assert.True(closed);
		Assert.Equal(0, app.Windows.Count);
	});

	[Fact]
	public void Shutdown_called_from_a_Closing_handler_does_not_reenter_Closing() => Run(() =>
	{
		var app = new RecordingApp();
		var win = new Window();
		int closing = 0, closedCount = 0;
		win.Closing += (_, _) => { closing++; app.Shutdown(); };
		win.Closed += (_, _) => closedCount++;
		app.RunLoopOverride = _ => win.Close();

		app.RunCore(NoArgs);

		Assert.Equal(1, closing);
		Assert.Equal(1, closedCount);
		Assert.Equal(0, app.Windows.Count);
	});
}
