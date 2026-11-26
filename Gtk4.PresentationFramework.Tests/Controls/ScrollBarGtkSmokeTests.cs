using Xunit;
using WpfScrollBar  = System.Windows.Controls.Primitives.ScrollBar;
using WpfOrientation = System.Windows.Controls.Orientation;
using WpfScrollEventArgs = System.Windows.Controls.Primitives.ScrollEventArgs;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for <see cref="WpfScrollBar"/>.
/// Each body runs on the GTK thread via Run(…); skipped when GTK is unavailable.
/// </summary>
public class ScrollBarGtkSmokeTests : WpfGtkTestBase
{
	// ── Widget type ──────────────────────────────────────────────────────────

	[Fact]
	public void ScrollBar_Widget_is_GtkScrollbar() => Run(() =>
	{
		var sb = new WpfScrollBar();
		Assert.IsType<Gtk.Scrollbar>(sb.Widget);
	});

	[Fact]
	public void ScrollBar_Horizontal_Widget_is_GtkScrollbar() => Run(() =>
	{
		var sb = new WpfScrollBar { Orientation = WpfOrientation.Horizontal };
		Assert.IsType<Gtk.Scrollbar>(sb.Widget);
	});

	// ── Adjustment mirrors DP ────────────────────────────────────────────────

	[Fact]
	public void ScrollBar_Value_set_updates_adjustment() => Run(() =>
	{
		var sb = new WpfScrollBar { Maximum = 100.0 };
		var gtkSb = (Gtk.Scrollbar)sb.Widget;
		sb.Value = 60.0;
		Assert.Equal(60.0, gtkSb.Adjustment.Value, 6);
	});

	[Fact]
	public void ScrollBar_Minimum_set_updates_adjustment() => Run(() =>
	{
		var sb = new WpfScrollBar { Maximum = 100.0 };
		_ = sb.Widget;
		sb.Minimum = 5.0;
		var gtkSb = (Gtk.Scrollbar)sb.Widget;
		Assert.Equal(5.0, gtkSb.Adjustment.Lower, 6);
	});

	[Fact]
	public void ScrollBar_Maximum_set_updates_adjustment_upper() => Run(() =>
	{
		var sb = new WpfScrollBar { ViewportSize = 0.0 };
		_ = sb.Widget;
		sb.Maximum = 50.0;
		var gtkSb = (Gtk.Scrollbar)sb.Widget;
		// GTK upper = Maximum + ViewportSize
		Assert.Equal(50.0, gtkSb.Adjustment.Upper, 6);
	});

	[Fact]
	public void ScrollBar_ViewportSize_reflected_in_adjustment_pageSize() => Run(() =>
	{
		var sb = new WpfScrollBar { Maximum = 100.0, ViewportSize = 20.0 };
		var gtkSb = (Gtk.Scrollbar)sb.Widget;
		Assert.Equal(20.0, gtkSb.Adjustment.PageSize, 6);
	});

	[Fact]
	public void ScrollBar_SmallChange_reflected_in_stepIncrement() => Run(() =>
	{
		var sb = new WpfScrollBar { Maximum = 100.0, SmallChange = 5.0 };
		var gtkSb = (Gtk.Scrollbar)sb.Widget;
		Assert.Equal(5.0, gtkSb.Adjustment.StepIncrement, 6);
	});

	[Fact]
	public void ScrollBar_LargeChange_reflected_in_pageIncrement() => Run(() =>
	{
		var sb = new WpfScrollBar { Maximum = 100.0, LargeChange = 25.0 };
		var gtkSb = (Gtk.Scrollbar)sb.Widget;
		Assert.Equal(25.0, gtkSb.Adjustment.PageIncrement, 6);
	});

	// ── Adjustment → managed sync ────────────────────────────────────────────

	[Fact]
	public void ScrollBar_adjustment_change_raises_ValueChanged() => Run(() =>
	{
		var sb = new WpfScrollBar { Maximum = 100.0 };
		var gtkSb = (Gtk.Scrollbar)sb.Widget;
		int count = 0;
		sb.ValueChanged += (_, _) => count++;
		gtkSb.Adjustment.Value = 40.0;
		Assert.Equal(1, count);
	});

	[Fact]
	public void ScrollBar_adjustment_change_updates_Value() => Run(() =>
	{
		var sb = new WpfScrollBar { Maximum = 100.0 };
		var gtkSb = (Gtk.Scrollbar)sb.Widget;
		gtkSb.Adjustment.Value = 70.0;
		Assert.Equal(70.0, sb.Value, 6);
	});

	[Fact]
	public void ScrollBar_adjustment_change_raises_Scroll() => Run(() =>
	{
		var sb = new WpfScrollBar { Maximum = 100.0 };
		var gtkSb = (Gtk.Scrollbar)sb.Widget;
		WpfScrollEventArgs? fired = null;
		sb.Scroll += (_, e) => fired = e;
		gtkSb.Adjustment.Value = 55.0;
		Assert.NotNull(fired);
		Assert.Equal(55.0, fired!.NewValue, 6);
	});

	// ── Headless → realize applies state ────────────────────────────────────

	[Fact]
	public void ScrollBar_preWidget_state_applied_on_realize() => Run(() =>
	{
		var sb = new WpfScrollBar
		{
			Maximum = 100.0,
			Value = 30.0,
			ViewportSize = 10.0,
			SmallChange = 2.0,
			LargeChange = 15.0,
		};
		// Widget not yet created; now realize it.
		var gtkSb = (Gtk.Scrollbar)sb.Widget;
		Assert.Equal(30.0, gtkSb.Adjustment.Value, 6);
		Assert.Equal(0.0, gtkSb.Adjustment.Lower, 6);
		Assert.Equal(110.0, gtkSb.Adjustment.Upper, 6); // 100 + 10
		Assert.Equal(10.0, gtkSb.Adjustment.PageSize, 6);
		Assert.Equal(2.0, gtkSb.Adjustment.StepIncrement, 6);
		Assert.Equal(15.0, gtkSb.Adjustment.PageIncrement, 6);
	});
}
