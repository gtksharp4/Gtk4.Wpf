using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Double-close / re-entrancy guards for <see cref="WpfWindow.Closing"/>, the
/// veto-then-allow retry, headless safety, DialogResult restoration on veto, and the
/// GTK close-request return-value mapping.
/// </summary>
public class WindowClosingGuardTests : WpfGtkTestBase
{
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

	[Fact]
	public void Close_before_Show_does_not_throw() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget; // materialise but do not Show

		var ex = Record.Exception(() => win.Close());

		Assert.Null(ex);
	});

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

	[Fact]
	public void Close_with_result_vetoed_restores_DialogResult() => Run(() =>
	{
		var win = new WpfWindow();
		_ = win.Widget;

		win.Closing += (_, e) => e.Cancel = true;

		win.Close(true); // vetoed — DialogResult must stay null

		Assert.Null(win.DialogResult);
	});

	// ── GTK close-request return-value mapping (pure) ─────────────────────────

	[Theory]
	[InlineData(true, false)]   // proceeded → RetVal false → allow GTK destroy
	[InlineData(false, true)]   // vetoed → RetVal true → suppress GTK destroy
	public void MapCloseRetVal_inverts_proceeded(bool proceeded, bool expectedRetVal)
	{
		Assert.Equal(expectedRetVal, WpfWindow.MapCloseRetVal(proceeded));
	}
}
