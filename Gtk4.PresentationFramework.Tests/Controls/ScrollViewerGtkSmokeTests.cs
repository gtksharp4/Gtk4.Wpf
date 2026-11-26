using Xunit;
using System.Windows.Controls;
using WpfScrollViewer = System.Windows.Controls.ScrollViewer;
using WpfTextBlock    = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for <see cref="WpfScrollViewer"/> offset API — metrics and scroll commands.
/// All bodies run on the GTK thread via Run(…); skipped when GTK is unavailable.
/// </summary>
public class ScrollViewerGtkSmokeTests : WpfGtkTestBase
{
	// ── Widget type ──────────────────────────────────────────────────────────

	[Fact]
	public void ScrollViewer_Widget_is_GtkScrolledWindow() => Run(() =>
	{
		var sv = new WpfScrollViewer();
		Assert.IsType<Gtk.ScrolledWindow>(sv.Widget);
	});

	// ── Shared helper ────────────────────────────────────────────────────────

	/// <summary>
	/// Realizes a ScrollViewer and manually configures adjustments to give it
	/// known geometry without needing a display server.
	/// </summary>
	internal static (WpfScrollViewer sv, Gtk.Adjustment vadj, Gtk.Adjustment hadj)
		MakeScrollViewer(double upper = 500.0, double pageSize = 100.0, double step = 20.0)
	{
		var sv = new WpfScrollViewer { Content = new WpfTextBlock { Text = "content" } };
		_ = sv.Widget; // realize
		var gtkSw = (Gtk.ScrolledWindow)sv.Widget;
		var vadj  = gtkSw.Vadjustment!;
		var hadj  = gtkSw.Hadjustment!;
		// Configure: (value, lower, upper, stepIncrement, pageIncrement, pageSize)
		vadj.Configure(0, 0, upper, step, pageSize, pageSize);
		hadj.Configure(0, 0, upper, step, pageSize, pageSize);
		return (sv, vadj, hadj);
	}

	// ── Read-only metrics ────────────────────────────────────────────────────

	[Fact]
	public void ScrollViewer_VerticalOffset_reads_adjustment_value() => Run(() =>
	{
		var (sv, vadj, _) = MakeScrollViewer();
		vadj.Value = 30.0;
		Assert.Equal(30.0, sv.VerticalOffset, 6);
	});

	[Fact]
	public void ScrollViewer_HorizontalOffset_reads_adjustment_value() => Run(() =>
	{
		var (sv, _, hadj) = MakeScrollViewer();
		hadj.Value = 15.0;
		Assert.Equal(15.0, sv.HorizontalOffset, 6);
	});

	[Fact]
	public void ScrollViewer_ExtentHeight_reads_adjustment_upper() => Run(() =>
	{
		var (sv, _, _) = MakeScrollViewer(upper: 800.0, pageSize: 200.0);
		Assert.Equal(800.0, sv.ExtentHeight, 6);
	});

	[Fact]
	public void ScrollViewer_ViewportHeight_reads_adjustment_pageSize() => Run(() =>
	{
		var (sv, _, _) = MakeScrollViewer(upper: 800.0, pageSize: 200.0);
		Assert.Equal(200.0, sv.ViewportHeight, 6);
	});

	[Fact]
	public void ScrollViewer_ScrollableHeight_is_upper_minus_pageSize() => Run(() =>
	{
		var (sv, _, _) = MakeScrollViewer(upper: 500.0, pageSize: 100.0);
		Assert.Equal(400.0, sv.ScrollableHeight, 6);
	});

	// ── ScrollToVerticalOffset ───────────────────────────────────────────────

	[Fact]
	public void ScrollToVerticalOffset_sets_adjustment_value() => Run(() =>
	{
		var (sv, vadj, _) = MakeScrollViewer();
		sv.ScrollToVerticalOffset(50.0);
		Assert.Equal(50.0, vadj.Value, 6);
	});

	[Fact]
	public void ScrollToVerticalOffset_clamps_above_scrollable() => Run(() =>
	{
		var (sv, vadj, _) = MakeScrollViewer(upper: 500.0, pageSize: 100.0);
		sv.ScrollToVerticalOffset(9999.0);
		Assert.Equal(400.0, vadj.Value, 6);
	});

	[Fact]
	public void ScrollToVerticalOffset_clamps_below_zero() => Run(() =>
	{
		var (sv, vadj, _) = MakeScrollViewer();
		sv.ScrollToVerticalOffset(-10.0);
		Assert.Equal(0.0, vadj.Value, 6);
	});

	[Fact]
	public void ScrollToVerticalOffset_VerticalOffset_reads_back() => Run(() =>
	{
		var (sv, _, _) = MakeScrollViewer();
		sv.ScrollToVerticalOffset(70.0);
		Assert.Equal(70.0, sv.VerticalOffset, 6);
	});

	[Fact]
	public void ScrollToHorizontalOffset_sets_adjustment_value() => Run(() =>
	{
		var (sv, _, hadj) = MakeScrollViewer();
		sv.ScrollToHorizontalOffset(40.0);
		Assert.Equal(40.0, hadj.Value, 6);
	});

	// ── ScrollToTop / ScrollToBottom ─────────────────────────────────────────

	[Fact]
	public void ScrollToTop_sets_offset_to_zero() => Run(() =>
	{
		var (sv, vadj, _) = MakeScrollViewer();
		vadj.Value = 200.0;
		sv.ScrollToTop();
		Assert.Equal(0.0, sv.VerticalOffset, 6);
	});

	[Fact]
	public void ScrollToBottom_sets_offset_to_scrollableHeight() => Run(() =>
	{
		var (sv, _, _) = MakeScrollViewer(upper: 500.0, pageSize: 100.0);
		sv.ScrollToBottom();
		Assert.Equal(400.0, sv.VerticalOffset, 6);
	});

	// ── LineDown / LineUp ────────────────────────────────────────────────────

	[Fact]
	public void LineDown_advances_by_step() => Run(() =>
	{
		var (sv, _, _) = MakeScrollViewer(step: 20.0);
		sv.ScrollToVerticalOffset(100.0);
		sv.LineDown();
		Assert.Equal(120.0, sv.VerticalOffset, 6);
	});

	[Fact]
	public void LineUp_retreats_by_step() => Run(() =>
	{
		var (sv, _, _) = MakeScrollViewer(step: 20.0);
		sv.ScrollToVerticalOffset(100.0);
		sv.LineUp();
		Assert.Equal(80.0, sv.VerticalOffset, 6);
	});

	// ── PageDown / PageUp ────────────────────────────────────────────────────

	[Fact]
	public void PageDown_advances_by_pageSize() => Run(() =>
	{
		var (sv, _, _) = MakeScrollViewer(upper: 500.0, pageSize: 100.0);
		sv.PageDown();
		Assert.Equal(100.0, sv.VerticalOffset, 6);
	});

	[Fact]
	public void PageUp_from_scrolled_retreats_by_pageSize() => Run(() =>
	{
		var (sv, _, _) = MakeScrollViewer(upper: 500.0, pageSize: 100.0);
		sv.ScrollToVerticalOffset(200.0);
		sv.PageUp();
		Assert.Equal(100.0, sv.VerticalOffset, 6);
	});
}
