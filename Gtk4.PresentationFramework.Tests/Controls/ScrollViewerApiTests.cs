using Xunit;
using WpfScrollViewer = System.Windows.Controls.ScrollViewer;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no GTK) tests for <see cref="WpfScrollViewer"/> offset API helpers
/// and headless safety.
/// </summary>
public class ScrollViewerApiTests
{
	// ── ClampOffset pure helper ──────────────────────────────────────────────

	[Fact]
	public void ClampOffset_within_range_returns_value()
		=> Assert.Equal(5.0, WpfScrollViewer.ClampOffset(5.0, 10.0), 10);

	[Fact]
	public void ClampOffset_below_zero_returns_zero()
		=> Assert.Equal(0.0, WpfScrollViewer.ClampOffset(-3.0, 10.0), 10);

	[Fact]
	public void ClampOffset_above_scrollable_returns_scrollable()
		=> Assert.Equal(10.0, WpfScrollViewer.ClampOffset(20.0, 10.0), 10);

	[Fact]
	public void ClampOffset_zero_scrollable_always_returns_zero()
		=> Assert.Equal(0.0, WpfScrollViewer.ClampOffset(5.0, 0.0), 10);

	// ── EffectiveStep helper ─────────────────────────────────────────────────

	[Fact]
	public void EffectiveStep_positive_stepIncrement_returns_it()
		=> Assert.Equal(8.0, WpfScrollViewer.EffectiveStep(8.0), 10);

	[Fact]
	public void EffectiveStep_zero_stepIncrement_returns_fallback()
		=> Assert.Equal(16.0, WpfScrollViewer.EffectiveStep(0.0), 10);

	[Fact]
	public void EffectiveStep_negative_stepIncrement_returns_fallback()
		=> Assert.Equal(16.0, WpfScrollViewer.EffectiveStep(-1.0), 10);

	// ── Headless metrics are 0 ───────────────────────────────────────────────

	[Fact]
	public void HorizontalOffset_headless_is_zero()
		=> Assert.Equal(0.0, new WpfScrollViewer().HorizontalOffset, 10);

	[Fact]
	public void VerticalOffset_headless_is_zero()
		=> Assert.Equal(0.0, new WpfScrollViewer().VerticalOffset, 10);

	[Fact]
	public void ExtentWidth_headless_is_zero()
		=> Assert.Equal(0.0, new WpfScrollViewer().ExtentWidth, 10);

	[Fact]
	public void ExtentHeight_headless_is_zero()
		=> Assert.Equal(0.0, new WpfScrollViewer().ExtentHeight, 10);

	[Fact]
	public void ViewportWidth_headless_is_zero()
		=> Assert.Equal(0.0, new WpfScrollViewer().ViewportWidth, 10);

	[Fact]
	public void ViewportHeight_headless_is_zero()
		=> Assert.Equal(0.0, new WpfScrollViewer().ViewportHeight, 10);

	[Fact]
	public void ScrollableWidth_headless_is_zero()
		=> Assert.Equal(0.0, new WpfScrollViewer().ScrollableWidth, 10);

	[Fact]
	public void ScrollableHeight_headless_is_zero()
		=> Assert.Equal(0.0, new WpfScrollViewer().ScrollableHeight, 10);

	// ── Headless scroll methods don't throw ─────────────────────────────────

	[Fact]
	public void ScrollToVerticalOffset_headless_does_not_throw()
	{
		var sv = new WpfScrollViewer();
		var ex = Record.Exception(() => sv.ScrollToVerticalOffset(50.0));
		Assert.Null(ex);
	}

	[Fact]
	public void ScrollToHorizontalOffset_headless_does_not_throw()
	{
		var sv = new WpfScrollViewer();
		var ex = Record.Exception(() => sv.ScrollToHorizontalOffset(50.0));
		Assert.Null(ex);
	}

	[Fact]
	public void LineUp_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().LineUp()));

	[Fact]
	public void LineDown_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().LineDown()));

	[Fact]
	public void LineLeft_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().LineLeft()));

	[Fact]
	public void LineRight_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().LineRight()));

	[Fact]
	public void PageUp_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().PageUp()));

	[Fact]
	public void PageDown_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().PageDown()));

	[Fact]
	public void PageLeft_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().PageLeft()));

	[Fact]
	public void PageRight_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().PageRight()));

	[Fact]
	public void ScrollToTop_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().ScrollToTop()));

	[Fact]
	public void ScrollToBottom_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().ScrollToBottom()));

	[Fact]
	public void ScrollToHome_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().ScrollToHome()));

	[Fact]
	public void ScrollToEnd_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().ScrollToEnd()));

	[Fact]
	public void ScrollToLeftEnd_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().ScrollToLeftEnd()));

	[Fact]
	public void ScrollToRightEnd_headless_does_not_throw()
		=> Assert.Null(Record.Exception(() => new WpfScrollViewer().ScrollToRightEnd()));
}
