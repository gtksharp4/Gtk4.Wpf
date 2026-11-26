using Xunit;
using WpfExpander      = System.Windows.Controls.Expander;
using WpfGroupBox      = System.Windows.Controls.GroupBox;
using WpfOrientation   = System.Windows.Controls.Orientation;
using WpfProgressBar   = System.Windows.Controls.ProgressBar;
using WpfScrollViewer  = System.Windows.Controls.ScrollViewer;
using WpfScrollBarVis  = System.Windows.Controls.ScrollBarVisibility;
using WpfSeparator     = System.Windows.Controls.Separator;
using WpfSlider        = System.Windows.Controls.Slider;
using WpfTabControl    = System.Windows.Controls.TabControl;
using WpfTabItem       = System.Windows.Controls.TabItem;
using WpfTextBlock     = System.Windows.Controls.TextBlock;
using WpfToggleButton  = System.Windows.Controls.Primitives.ToggleButton;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for the Round-5 controls. All bodies run on the GTK thread
/// via Run(…). Skipped when GTK is unavailable.
/// </summary>
public class Round5GtkSmokeTests : WpfGtkTestBase
{
	// ── ScrollViewer ─────────────────────────────────────────────────────────

	[Fact]
	public void ScrollViewer_Widget_is_GtkScrolledWindow() => Run(() =>
	{
		var sv = new WpfScrollViewer();
		Assert.IsType<Gtk.ScrolledWindow>(sv.Widget);
	});

	[Fact]
	public void ScrollViewer_Content_UIElement_hosted() => Run(() =>
	{
		var sv = new WpfScrollViewer();
		var tb = new WpfTextBlock { Text = "scroll content" };
		sv.Content = tb;
		// Gtk.ScrolledWindow wraps the child in a Gtk.Viewport automatically;
		// verify the content widget is reachable via the viewport's child.
		var sw = (Gtk.ScrolledWindow)sv.Widget;
		var viewport = sw.Child as Gtk.Viewport;
		Assert.NotNull(viewport);
		Assert.Same(tb.Widget, viewport!.Child);
	});

	[Fact]
	public void ScrollViewer_HScrollbar_policy_applied() => Run(() =>
	{
		var sv = new WpfScrollViewer
		{
			HorizontalScrollBarVisibility = WpfScrollBarVis.Disabled,
		};
		var sw = (Gtk.ScrolledWindow)sv.Widget;
		// Disabled clips like WPF: External shows no bar and, unlike Never, does not pin the window to the content width.
		Assert.Equal(Gtk.PolicyType.External, sw.HscrollbarPolicy);
	});

	[Fact]
	public void ScrollViewer_VScrollbar_policy_applied() => Run(() =>
	{
		var sv = new WpfScrollViewer
		{
			VerticalScrollBarVisibility = WpfScrollBarVis.Visible,
		};
		var sw = (Gtk.ScrolledWindow)sv.Widget;
		Assert.Equal(Gtk.PolicyType.Always, sw.VscrollbarPolicy);
	});

	// ── TabControl / TabItem ──────────────────────────────────────────────────

	[Fact]
	public void TabControl_Widget_is_GtkNotebook() => Run(() =>
	{
		var tc = new WpfTabControl();
		Assert.IsType<Gtk.Notebook>(tc.Widget);
	});

	[Fact]
	public void TabControl_two_TabItems_produces_two_pages() => Run(() =>
	{
		var tc = new WpfTabControl();
		tc.ItemsSource = new[]
		{
			new WpfTabItem { Header = "Tab1" },
			new WpfTabItem { Header = "Tab2" },
		};
		var nb = (Gtk.Notebook)tc.Widget;
		Assert.Equal(2, nb.NPages);
	});

	[Fact]
	public void TabItem_Widget_is_GtkBox() => Run(() =>
	{
		var ti = new WpfTabItem { Header = "X" };
		Assert.IsType<Gtk.Box>(ti.Widget);
	});

	// ── Expander ─────────────────────────────────────────────────────────────

	[Fact]
	public void Expander_Widget_is_GtkExpander() => Run(() =>
	{
		var exp = new WpfExpander { Header = "Show more" };
		Assert.IsType<Gtk.Expander>(exp.Widget);
	});

	[Fact]
	public void Expander_IsExpanded_false_on_creation() => Run(() =>
	{
		var exp = new WpfExpander { Header = "h" };
		var gtkExp = (Gtk.Expander)exp.Widget;
		Assert.False(gtkExp.Expanded);
	});

	[Fact]
	public void Expander_IsExpanded_set_reflects_on_GtkExpander() => Run(() =>
	{
		var exp = new WpfExpander { Header = "h" };
		exp.IsExpanded = true;
		var gtkExp = (Gtk.Expander)exp.Widget;
		Assert.True(gtkExp.Expanded);
	});

	[Fact]
	public void Expander_GtkExpand_updates_IsExpanded() => Run(() =>
	{
		var exp = new WpfExpander { Header = "h" };
		var gtkExp = (Gtk.Expander)exp.Widget;
		gtkExp.Expanded = true;
		Assert.True(exp.IsExpanded);
	});

	// ── GroupBox ─────────────────────────────────────────────────────────────

	[Fact]
	public void GroupBox_Widget_is_GtkFrame() => Run(() =>
	{
		var gb = new WpfGroupBox { Header = "Options" };
		Assert.IsType<Gtk.Frame>(gb.Widget);
	});

	[Fact]
	public void GroupBox_Header_string_applied_to_GtkFrame() => Run(() =>
	{
		var gb = new WpfGroupBox { Header = "My Group" };
		var frame = (Gtk.Frame)gb.Widget;
		Assert.Equal("My Group", frame.Label);
	});

	[Fact]
	public void GroupBox_Content_UIElement_set_as_frame_child() => Run(() =>
	{
		var gb = new WpfGroupBox { Header = "G" };
		var tb = new WpfTextBlock { Text = "inner" };
		gb.Content = tb;
		var frame = (Gtk.Frame)gb.Widget;
		Assert.Same(tb.Widget, frame.Child);
	});

	// ── Slider ───────────────────────────────────────────────────────────────

	[Fact]
	public void Slider_Widget_is_GtkScale() => Run(() =>
	{
		var s = new WpfSlider { Maximum = 100 };
		Assert.IsType<Gtk.Scale>(s.Widget);
	});

	[Fact]
	public void Slider_Value_set_reflects_on_GtkScale() => Run(() =>
	{
		var s = new WpfSlider { Maximum = 100, Value = 30.0 };
		var scale = (Gtk.Scale)s.Widget;
		Assert.Equal(30.0, scale.Value);
	});

	[Fact]
	public void Slider_GtkScale_change_updates_Value() => Run(() =>
	{
		var s = new WpfSlider { Maximum = 100 };
		var scale = (Gtk.Scale)s.Widget;
		scale.Value = 60.0;
		Assert.Equal(60.0, s.Value);
	});

	[Fact]
	public void Slider_Vertical_orientation_produces_vertical_GtkScale() => Run(() =>
	{
		var s = new WpfSlider { Orientation = WpfOrientation.Vertical };
		var scale = (Gtk.Scale)s.Widget;
		Assert.Equal(Gtk.Orientation.Vertical, scale.Orientation);
	});

	// ── ProgressBar ──────────────────────────────────────────────────────────

	[Fact]
	public void ProgressBar_Widget_is_GtkProgressBar() => Run(() =>
	{
		var pb = new WpfProgressBar();
		Assert.IsType<Gtk.ProgressBar>(pb.Widget);
	});

	[Fact]
	public void ProgressBar_Value_50_sets_fraction_half() => Run(() =>
	{
		var pb = new WpfProgressBar { Value = 50.0 };
		var bar = (Gtk.ProgressBar)pb.Widget;
		Assert.Equal(0.5, bar.Fraction, precision: 5);
	});

	[Fact]
	public void ProgressBar_Value_zero_sets_fraction_zero() => Run(() =>
	{
		var pb = new WpfProgressBar { Value = 0.0 };
		var bar = (Gtk.ProgressBar)pb.Widget;
		Assert.Equal(0.0, bar.Fraction);
	});

	// ── Separator ────────────────────────────────────────────────────────────

	[Fact]
	public void Separator_Widget_is_GtkSeparator() => Run(() =>
	{
		var sep = new WpfSeparator();
		Assert.IsType<Gtk.Separator>(sep.Widget);
	});

	[Fact]
	public void Separator_Vertical_produces_vertical_GtkSeparator() => Run(() =>
	{
		var sep = new WpfSeparator { Orientation = WpfOrientation.Vertical };
		var gtkSep = (Gtk.Separator)sep.Widget;
		Assert.Equal(Gtk.Orientation.Vertical, gtkSep.Orientation);
	});

	// ── ToggleButton ─────────────────────────────────────────────────────────

	[Fact]
	public void ToggleButton_Widget_is_GtkToggleButton() => Run(() =>
	{
		var tb = new WpfToggleButton();
		Assert.IsType<Gtk.ToggleButton>(tb.Widget);
	});

	[Fact]
	public void ToggleButton_IsChecked_true_activates_GtkToggleButton() => Run(() =>
	{
		var tb = new WpfToggleButton { IsChecked = true };
		var gtkTb = (Gtk.ToggleButton)tb.Widget;
		Assert.True(gtkTb.Active);
	});

	[Fact]
	public void ToggleButton_GtkToggle_updates_IsChecked() => Run(() =>
	{
		var tb = new WpfToggleButton();
		var gtkTb = (Gtk.ToggleButton)tb.Widget;
		gtkTb.Active = true;
		Assert.Equal(true, tb.IsChecked);
	});

	[Fact]
	public void ToggleButton_Checked_fires_on_GtkToggle() => Run(() =>
	{
		var tb = new WpfToggleButton();
		_ = tb.Widget; // materialise
		bool fired = false;
		tb.Checked += (s, e) => { fired = true; };
		var gtkTb = (Gtk.ToggleButton)tb.Widget;
		gtkTb.Active = true;
		Assert.True(fired);
	});
}