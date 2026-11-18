using System.ComponentModel;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="WpfWindow.Closing"/> — the cancelable close protocol.
/// </summary>
/// <remarks>
/// All tests that need a real GTK widget use <see cref="WpfGtkTestBase.Run"/> so they
/// execute on the GTK thread. Pure tests (no GTK widget materialised) also run via
/// <see cref="WpfGtkTestBase.Run"/> for consistency; they would pass on any thread.
/// </remarks>
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

		bool result = win.RequestClose();

		Assert.False(result);
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

		bool result = win.RequestClose();

		Assert.True(result);
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

	// ── Closed not raised when vetoed ─────────────────────────────────────────

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

		bool proceeded = win.RequestClose();

		// GTK OnCloseRequest would return e.RetVal = true (stop default) when this is false.
		Assert.False(proceeded);
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

	// ── Double-close guard ────────────────────────────────────────────────────

	[Fact]
	public void Closing_not_raised_on_second_Close_after_first_succeeded() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		int closingCount = 0;
		win.Closing += (_, _) => closingCount++;

		win.Close();
		win.Close(); // second call: already closed

		Assert.Equal(1, closingCount);
	});

	// ── Re-entrant Close inside Closing handler ───────────────────────────────

	[Fact]
	public void Closing_handler_calling_Close_does_not_raise_twice() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		int closedCount = 0;
		win.Closed += (_, _) => closedCount++;

		// A re-entrant Close() from inside the Closing handler must be a no-op.
		win.Closing += (_, _) => win.Close();

		win.Close();

		Assert.Equal(1, closedCount);
	});

	// ── Headless: Close() before Show() does not throw ────────────────────────

	[Fact]
	public void Close_before_Show_does_not_throw() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget; // materialise but do not Show

		var ex = Record.Exception(() => win.Close());

		Assert.Null(ex);
	});

	// ── Veto allows subsequent successful close ───────────────────────────────

	[Fact]
	public void Veto_then_allow_closes_successfully() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		bool veto = true;
		win.Closing += (_, e) => e.Cancel = veto;

		int closed = 0;
		win.Closed += (_, _) => closed++;

		win.Close(); // vetoed
		Assert.Equal(0, closed);

		veto = false;
		win.Close(); // allowed
		Assert.Equal(1, closed);
	});

	// ── Close(bool?) with veto restores DialogResult ──────────────────────────

	[Fact]
	public void Close_with_result_vetoed_restores_DialogResult() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		win.Closing += (_, e) => e.Cancel = true;

		win.Close(true); // vetoed — DialogResult must stay null

		Assert.Null(win.DialogResult);
	});
}
