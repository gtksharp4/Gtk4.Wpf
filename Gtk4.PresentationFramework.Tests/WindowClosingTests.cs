using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="WpfWindow.Closing"/> — veto, allow, ordering, and the GTK
/// <c>CloseRequest</c> seam. (Re-entrancy/guard cases live in
/// <see cref="WindowClosingGuardTests"/>.)
/// </summary>
public class WindowClosingTests : WpfGtkTestBase
{
	// ── Cancel veto ──────────────────────────────────────────────────────────

	[Fact]
	public void Close_vetoed_does_not_raise_Closed() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		win.Closing += (_, e) => e.Cancel = true;

		int closed = 0;
		win.Closed += (_, _) => closed++;

		win.Close();

		Assert.Equal(0, closed);
	});

	[Fact]
	public void RequestClose_returns_false_when_vetoed() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		win.Closing += (_, e) => e.Cancel = true;

		Assert.False(win.RequestClose());
	});

	// ── Allow path ───────────────────────────────────────────────────────────

	[Fact]
	public void Close_proceeds_and_raises_Closed_when_not_vetoed() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		win.Closing += (_, _) => { }; // handler that does not cancel

		int closed = 0;
		win.Closed += (_, _) => closed++;

		win.Close();

		Assert.Equal(1, closed);
	});

	[Fact]
	public void RequestClose_returns_true_when_allowed() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		Assert.True(win.RequestClose());
	});

	// ── Order: Closing before Closed ──────────────────────────────────────────

	[Fact]
	public void Closing_raised_before_Closed() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		int order = 0;
		int closingOrder = -1;
		int closedOrder = -1;

		win.Closing += (_, _) => closingOrder = order++;
		win.Closed += (_, _) => closedOrder = order++;

		win.Close();

		Assert.Equal(0, closingOrder);
		Assert.Equal(1, closedOrder);
	});

	[Fact]
	public void Closed_not_raised_when_Closing_canceled() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		bool closedRaised = false;
		win.Closing += (_, e) => e.Cancel = true;
		win.Closed += (_, _) => closedRaised = true;

		win.Close();

		Assert.False(closedRaised);
	});

	// ── GTK CloseRequest path (via internal seam) ─────────────────────────────

	[Fact]
	public void RequestClose_vetoed_returns_false_simulating_gtk_close_request() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		win.Closing += (_, e) => e.Cancel = true;

		Assert.False(win.RequestClose());
	});

	[Fact]
	public void RequestClose_allowed_does_not_raise_Closed_twice_via_gtk_path() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		int closed = 0;
		win.Closed += (_, _) => closed++;

		// Simulate two CloseRequest signals in a row (should not double-fire Closed).
		win.RequestClose();
		win.RequestClose();

		Assert.Equal(1, closed);
	});
}
