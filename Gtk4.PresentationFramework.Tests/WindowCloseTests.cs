using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for <see cref="System.Windows.Window.Closed"/> — the event
/// <see cref="System.Windows.Application"/> uses to exit when the main window closes.
/// </summary>
public class WindowCloseTests : WpfGtkTestBase
{
	[Fact]
	public void Close_raises_Closed() => Run(() =>
	{
		var win = new WpfWindow { Title = "CloseTest" };
		_ = win.Widget; // materialise the backing GTK window

		int raised = 0;
		win.Closed += (_, _) => raised++;

		win.Close();

		Assert.Equal(1, raised);
	});

	[Fact]
	public void Close_with_result_raises_Closed_and_sets_result() => Run(() =>
	{
		var win = new WpfWindow { Title = "CloseResult" };
		_ = win.Widget;

		bool raised = false;
		win.Closed += (_, _) => raised = true;

		win.Close(true);

		Assert.True(raised);
		Assert.True(win.DialogResult);
	});

	[Fact]
	public void Closed_raised_once_on_double_close() => Run(() =>
	{
		var win = new WpfWindow { Title = "DoubleClose" };
		_ = win.Widget;

		int raised = 0;
		win.Closed += (_, _) => raised++;

		win.Close();
		win.Close();

		Assert.Equal(1, raised);
	});
}