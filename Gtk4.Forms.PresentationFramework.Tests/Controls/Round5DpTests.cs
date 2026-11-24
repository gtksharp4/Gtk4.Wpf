using System;
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

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no GTK) DP tests for the Round-5 controls.
/// </summary>
public class Round5DpTests
{
	// ── ScrollViewer ─────────────────────────────────────────────────────────

	[Fact]
	public void ScrollViewer_HorizontalScrollBarVisibility_default_is_Disabled()
	{
		var sv = new WpfScrollViewer();
		Assert.Equal(WpfScrollBarVis.Disabled, sv.HorizontalScrollBarVisibility);
	}

	[Fact]
	public void ScrollViewer_VerticalScrollBarVisibility_default_is_Visible()
	{
		var sv = new WpfScrollViewer();
		Assert.Equal(WpfScrollBarVis.Visible, sv.VerticalScrollBarVisibility);
	}

	[Fact]
	public void ScrollViewer_HorizontalScrollBarVisibility_roundtrip()
	{
		var sv = new WpfScrollViewer();
		sv.HorizontalScrollBarVisibility = WpfScrollBarVis.Disabled;
		Assert.Equal(WpfScrollBarVis.Disabled, sv.HorizontalScrollBarVisibility);
	}

	[Fact]
	public void ScrollViewer_VerticalScrollBarVisibility_roundtrip()
	{
		var sv = new WpfScrollViewer();
		sv.VerticalScrollBarVisibility = WpfScrollBarVis.Visible;
		Assert.Equal(WpfScrollBarVis.Visible, sv.VerticalScrollBarVisibility);
	}

	[Fact]
	public void ScrollViewer_Content_roundtrip()
	{
		var sv = new WpfScrollViewer();
		var tb = new WpfTextBlock { Text = "scroll me" };
		sv.Content = tb;
		Assert.Same(tb, sv.Content);
	}

	// ── TabControl / TabItem ──────────────────────────────────────────────────

	[Fact]
	public void TabControl_SelectedIndex_default_is_minus_one()
	{
		var tc = new WpfTabControl();
		Assert.Equal(-1, tc.SelectedIndex);
	}

	[Fact]
	public void TabControl_SelectedItem_default_is_null()
	{
		var tc = new WpfTabControl();
		Assert.Null(tc.SelectedItem);
	}

	[Fact]
	public void TabItem_Header_roundtrip()
	{
		var ti = new WpfTabItem();
		ti.Header = "Page 1";
		Assert.Equal("Page 1", ti.Header);
	}

	[Fact]
	public void TabItem_Content_roundtrip()
	{
		var ti = new WpfTabItem();
		var tb = new WpfTextBlock { Text = "body" };
		ti.Content = tb;
		Assert.Same(tb, ti.Content);
	}

	[Fact]
	public void TabControl_SelectionChanged_fires_when_SelectedIndex_set()
	{
		var tc = new WpfTabControl();
		var ti1 = new WpfTabItem { Header = "A" };
		var ti2 = new WpfTabItem { Header = "B" };
		tc.ItemsSource = new[] { ti1, ti2 };

		bool fired = false;
		tc.SelectionChanged += (s, e) => { fired = true; };
		tc.SelectedItem = ti2;
		Assert.True(fired);
	}

	// ── Expander ─────────────────────────────────────────────────────────────

	[Fact]
	public void Expander_IsExpanded_default_is_false()
	{
		var exp = new WpfExpander();
		Assert.False(exp.IsExpanded);
	}

	[Fact]
	public void Expander_IsExpanded_roundtrip()
	{
		var exp = new WpfExpander();
		exp.IsExpanded = true;
		Assert.True(exp.IsExpanded);
	}

	[Fact]
	public void Expander_Header_roundtrip()
	{
		var exp = new WpfExpander();
		exp.Header = "Details";
		Assert.Equal("Details", exp.Header);
	}

	[Fact]
	public void Expander_Expanded_event_fires_on_true()
	{
		var exp = new WpfExpander();
		bool fired = false;
		exp.Expanded += (s, e) => { fired = true; };
		exp.IsExpanded = true;
		Assert.True(fired);
	}

	[Fact]
	public void Expander_Collapsed_event_fires_on_false()
	{
		var exp = new WpfExpander { IsExpanded = true };
		bool fired = false;
		exp.Collapsed += (s, e) => { fired = true; };
		exp.IsExpanded = false;
		Assert.True(fired);
	}

	[Fact]
	public void Expander_Content_roundtrip()
	{
		var exp = new WpfExpander();
		var tb = new WpfTextBlock { Text = "inner" };
		exp.Content = tb;
		Assert.Same(tb, exp.Content);
	}

	// ── GroupBox ─────────────────────────────────────────────────────────────

	[Fact]
	public void GroupBox_Header_default_is_null()
	{
		var gb = new WpfGroupBox();
		Assert.Null(gb.Header);
	}

	[Fact]
	public void GroupBox_Header_roundtrip()
	{
		var gb = new WpfGroupBox();
		gb.Header = "Settings";
		Assert.Equal("Settings", gb.Header);
	}

	[Fact]
	public void GroupBox_Content_roundtrip()
	{
		var gb = new WpfGroupBox();
		var tb = new WpfTextBlock { Text = "content" };
		gb.Content = tb;
		Assert.Same(tb, gb.Content);
	}

	// ── Slider ───────────────────────────────────────────────────────────────

	[Fact]
	public void Slider_Minimum_default_is_zero()
	{
		var s = new WpfSlider();
		Assert.Equal(0.0, s.Minimum);
	}

	[Fact]
	public void Slider_Maximum_default_is_10()
	{
		var s = new WpfSlider();
		Assert.Equal(10.0, s.Maximum);
	}

	[Fact]
	public void Slider_Value_default_is_zero()
	{
		var s = new WpfSlider();
		Assert.Equal(0.0, s.Value);
	}

	[Fact]
	public void Slider_Value_roundtrip()
	{
		var s = new WpfSlider { Maximum = 100 };
		s.Value = 42.0;
		Assert.Equal(42.0, s.Value);
	}

	[Fact]
	public void Slider_Value_clamped_to_Maximum()
	{
		var s = new WpfSlider { Maximum = 50.0 };
		s.Value = 200.0;
		Assert.Equal(50.0, s.Value);
	}

	[Fact]
	public void Slider_Value_clamped_to_Minimum()
	{
		var s = new WpfSlider { Minimum = 10.0 };
		s.Value = 0.0;
		Assert.Equal(10.0, s.Value);
	}

	[Fact]
	public void Slider_ValueChanged_fires_on_set()
	{
		var s = new WpfSlider();
		bool fired = false;
		s.ValueChanged += (sender, e) => { fired = true; };
		s.Value = 50.0;
		Assert.True(fired);
	}

	[Fact]
	public void Slider_Orientation_default_is_Horizontal()
	{
		var s = new WpfSlider();
		Assert.Equal(WpfOrientation.Horizontal, s.Orientation);
	}

	// ── ProgressBar ──────────────────────────────────────────────────────────

	[Fact]
	public void ProgressBar_Minimum_default_is_zero()
	{
		var pb = new WpfProgressBar();
		Assert.Equal(0.0, pb.Minimum);
	}

	[Fact]
	public void ProgressBar_Maximum_default_is_100()
	{
		var pb = new WpfProgressBar();
		Assert.Equal(100.0, pb.Maximum);
	}

	[Fact]
	public void ProgressBar_Value_default_is_zero()
	{
		var pb = new WpfProgressBar();
		Assert.Equal(0.0, pb.Value);
	}

	[Fact]
	public void ProgressBar_Value_roundtrip()
	{
		var pb = new WpfProgressBar();
		pb.Value = 75.0;
		Assert.Equal(75.0, pb.Value);
	}

	[Fact]
	public void ProgressBar_IsIndeterminate_default_is_false()
	{
		var pb = new WpfProgressBar();
		Assert.False(pb.IsIndeterminate);
	}

	[Fact]
	public void ProgressBar_IsIndeterminate_roundtrip()
	{
		var pb = new WpfProgressBar();
		pb.IsIndeterminate = true;
		Assert.True(pb.IsIndeterminate);
	}

	// ── Separator ────────────────────────────────────────────────────────────

	[Fact]
	public void Separator_Orientation_default_is_Horizontal()
	{
		var sep = new WpfSeparator();
		Assert.Equal(WpfOrientation.Horizontal, sep.Orientation);
	}

	[Fact]
	public void Separator_Orientation_roundtrip()
	{
		var sep = new WpfSeparator();
		sep.Orientation = WpfOrientation.Vertical;
		Assert.Equal(WpfOrientation.Vertical, sep.Orientation);
	}

	// ── ToggleButton ─────────────────────────────────────────────────────────

	[Fact]
	public void ToggleButton_IsChecked_default_is_false()
	{
		var tb = new WpfToggleButton();
		Assert.Equal(false, tb.IsChecked);
	}

	[Fact]
	public void ToggleButton_IsChecked_roundtrip()
	{
		var tb = new WpfToggleButton();
		tb.IsChecked = true;
		Assert.Equal(true, tb.IsChecked);
	}

	[Fact]
	public void ToggleButton_Checked_fires_when_IsChecked_set_true()
	{
		var tb = new WpfToggleButton();
		bool fired = false;
		tb.Checked += (s, e) => { fired = true; };
		tb.IsChecked = true;
		Assert.True(fired);
	}

	[Fact]
	public void ToggleButton_Unchecked_fires_when_IsChecked_set_false()
	{
		var tb = new WpfToggleButton { IsChecked = true };
		bool fired = false;
		tb.Unchecked += (s, e) => { fired = true; };
		tb.IsChecked = false;
		Assert.True(fired);
	}
}