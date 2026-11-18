using Xunit;
using WpfFrameworkElement = System.Windows.FrameworkElement;
using WpfPlacementMode    = System.Windows.Controls.PlacementMode;
using WpfTextBlock        = System.Windows.Controls.TextBlock;
using WpfToolTip          = System.Windows.Controls.ToolTip;
using WpfToolTipService   = System.Windows.Controls.ToolTipService;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure DP tests for <see cref="WpfToolTip"/> and <see cref="WpfToolTipService"/>.
/// No GTK runtime required.
/// </summary>
public class ToolTipDpTests
{
	// ── ToolTip control defaults ──────────────────────────────────────────────

	[Fact]
	public void ToolTip_IsOpen_default_false()
	{
		var tip = new WpfToolTip();
		Assert.False(tip.IsOpen);
	}

	[Fact]
	public void ToolTip_IsOpen_roundtrip()
	{
		var tip = new WpfToolTip();
		tip.IsOpen = true;
		Assert.True(tip.IsOpen);
	}

	[Fact]
	public void ToolTip_Placement_default_MousePoint()
	{
		var tip = new WpfToolTip();
		Assert.Equal(WpfPlacementMode.MousePoint, tip.Placement);
	}

	[Fact]
	public void ToolTip_Placement_roundtrip()
	{
		var tip = new WpfToolTip { Placement = WpfPlacementMode.Bottom };
		Assert.Equal(WpfPlacementMode.Bottom, tip.Placement);
	}

	[Fact]
	public void ToolTip_PlacementTarget_default_null()
	{
		var tip = new WpfToolTip();
		Assert.Null(tip.PlacementTarget);
	}

	[Fact]
	public void ToolTip_PlacementTarget_roundtrip()
	{
		var tip    = new WpfToolTip();
		var target = new WpfTextBlock();
		tip.PlacementTarget = target;
		Assert.Same(target, tip.PlacementTarget);
	}

	[Fact]
	public void ToolTip_StaysOpen_default_true()
	{
		var tip = new WpfToolTip();
		Assert.True(tip.StaysOpen);
	}

	[Fact]
	public void ToolTip_StaysOpen_roundtrip_false()
	{
		var tip = new WpfToolTip { StaysOpen = false };
		Assert.False(tip.StaysOpen);
	}

	[Fact]
	public void ToolTip_Content_roundtrip_string()
	{
		var tip = new WpfToolTip { Content = "hint" };
		Assert.Equal("hint", tip.Content);
	}

	[Fact]
	public void ToolTip_Content_roundtrip_UIElement()
	{
		var inner = new WpfTextBlock { Text = "rich" };
		var tip   = new WpfToolTip { Content = inner };
		Assert.Same(inner, tip.Content);
	}

	// ── FrameworkElement.ToolTip DP ───────────────────────────────────────────

	[Fact]
	public void FrameworkElement_ToolTip_string_headless_no_throw()
	{
		var tb = new WpfTextBlock();
		var ex = Record.Exception(() => tb.ToolTip = "hint");
		Assert.Null(ex);
	}

	[Fact]
	public void FrameworkElement_ToolTip_persists_before_realize()
	{
		var tb = new WpfTextBlock();
		tb.ToolTip = "stored";
		Assert.Equal("stored", tb.ToolTip);
	}

	[Fact]
	public void FrameworkElement_ToolTip_ToolTip_instance_headless_no_throw()
	{
		var tb  = new WpfTextBlock();
		var tip = new WpfToolTip { Content = "rich" };
		var ex  = Record.Exception(() => tb.ToolTip = tip);
		Assert.Null(ex);
	}

	[Fact]
	public void FrameworkElement_ToolTip_ToolTip_instance_persists_before_realize()
	{
		var tb  = new WpfTextBlock();
		var tip = new WpfToolTip { Content = "rich" };
		tb.ToolTip = tip;
		Assert.Same(tip, tb.ToolTip);
	}

	// ── ToolTipService regression ─────────────────────────────────────────────

	[Fact]
	public void ToolTipService_GetSet_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetToolTip(tb, "service");
		Assert.Equal("service", WpfToolTipService.GetToolTip(tb));
	}

	[Fact]
	public void ToolTipService_set_reads_via_FrameworkElement_GetToolTip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetToolTip(tb, "shared");
		Assert.Equal("shared", WpfFrameworkElement.GetToolTip(tb));
	}

	[Fact]
	public void ToolTipService_set_ToolTip_instance_roundtrip()
	{
		var tb  = new WpfTextBlock();
		var tip = new WpfToolTip { Content = "tt" };
		WpfToolTipService.SetToolTip(tb, tip);
		Assert.Same(tip, WpfToolTipService.GetToolTip(tb));
	}

	// ── ToolTipService new attached DPs — defaults ────────────────────────────

	[Fact]
	public void ToolTipService_InitialShowDelay_default_1000()
	{
		var tb = new WpfTextBlock();
		Assert.Equal(1000, WpfToolTipService.GetInitialShowDelay(tb));
	}

	[Fact]
	public void ToolTipService_ShowDuration_default_5000()
	{
		var tb = new WpfTextBlock();
		Assert.Equal(5000, WpfToolTipService.GetShowDuration(tb));
	}

	[Fact]
	public void ToolTipService_BetweenShowDelay_default_100()
	{
		var tb = new WpfTextBlock();
		Assert.Equal(100, WpfToolTipService.GetBetweenShowDelay(tb));
	}

	[Fact]
	public void ToolTipService_IsEnabled_default_true()
	{
		var tb = new WpfTextBlock();
		Assert.True(WpfToolTipService.GetIsEnabled(tb));
	}

	[Fact]
	public void ToolTipService_ShowOnDisabled_default_false()
	{
		var tb = new WpfTextBlock();
		Assert.False(WpfToolTipService.GetShowOnDisabled(tb));
	}

	[Fact]
	public void ToolTipService_Placement_default_Mouse()
	{
		var tb = new WpfTextBlock();
		Assert.Equal(WpfPlacementMode.Mouse, WpfToolTipService.GetPlacement(tb));
	}

	[Fact]
	public void ToolTipService_PlacementTarget_default_null()
	{
		var tb = new WpfTextBlock();
		Assert.Null(WpfToolTipService.GetPlacementTarget(tb));
	}

	[Fact]
	public void ToolTipService_HorizontalOffset_default_zero()
	{
		var tb = new WpfTextBlock();
		Assert.Equal(0.0, WpfToolTipService.GetHorizontalOffset(tb));
	}

	[Fact]
	public void ToolTipService_VerticalOffset_default_zero()
	{
		var tb = new WpfTextBlock();
		Assert.Equal(0.0, WpfToolTipService.GetVerticalOffset(tb));
	}

	// ── ToolTipService new attached DPs — round-trips ────────────────────────

	[Fact]
	public void ToolTipService_InitialShowDelay_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetInitialShowDelay(tb, 400);
		Assert.Equal(400, WpfToolTipService.GetInitialShowDelay(tb));
	}

	[Fact]
	public void ToolTipService_ShowDuration_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetShowDuration(tb, 3000);
		Assert.Equal(3000, WpfToolTipService.GetShowDuration(tb));
	}

	[Fact]
	public void ToolTipService_BetweenShowDelay_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetBetweenShowDelay(tb, 50);
		Assert.Equal(50, WpfToolTipService.GetBetweenShowDelay(tb));
	}

	[Fact]
	public void ToolTipService_IsEnabled_roundtrip_false()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetIsEnabled(tb, false);
		Assert.False(WpfToolTipService.GetIsEnabled(tb));
	}

	[Fact]
	public void ToolTipService_IsEnabled_roundtrip_true_after_false()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetIsEnabled(tb, false);
		WpfToolTipService.SetIsEnabled(tb, true);
		Assert.True(WpfToolTipService.GetIsEnabled(tb));
	}

	[Fact]
	public void ToolTipService_ShowOnDisabled_roundtrip_true()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetShowOnDisabled(tb, true);
		Assert.True(WpfToolTipService.GetShowOnDisabled(tb));
	}

	[Fact]
	public void ToolTipService_Placement_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetPlacement(tb, WpfPlacementMode.Bottom);
		Assert.Equal(WpfPlacementMode.Bottom, WpfToolTipService.GetPlacement(tb));
	}

	[Fact]
	public void ToolTipService_PlacementTarget_roundtrip()
	{
		var tb     = new WpfTextBlock();
		var target = new WpfTextBlock();
		WpfToolTipService.SetPlacementTarget(tb, target);
		Assert.Same(target, WpfToolTipService.GetPlacementTarget(tb));
	}

	[Fact]
	public void ToolTipService_HorizontalOffset_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetHorizontalOffset(tb, 12.5);
		Assert.Equal(12.5, WpfToolTipService.GetHorizontalOffset(tb));
	}

	[Fact]
	public void ToolTipService_VerticalOffset_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetVerticalOffset(tb, -4.0);
		Assert.Equal(-4.0, WpfToolTipService.GetVerticalOffset(tb));
	}

	// ── ToolTipService — headless no-throw (all new props before realize) ─────

	[Fact]
	public void ToolTipService_all_new_props_before_realize_no_throw()
	{
		var tb     = new WpfTextBlock();
		var target = new WpfTextBlock();
		var ex = Record.Exception(() =>
		{
			WpfToolTipService.SetInitialShowDelay(tb, 200);
			WpfToolTipService.SetShowDuration(tb, 2000);
			WpfToolTipService.SetBetweenShowDelay(tb, 50);
			WpfToolTipService.SetIsEnabled(tb, false);
			WpfToolTipService.SetShowOnDisabled(tb, true);
			WpfToolTipService.SetPlacement(tb, WpfPlacementMode.Bottom);
			WpfToolTipService.SetPlacementTarget(tb, target);
			WpfToolTipService.SetHorizontalOffset(tb, 10.0);
			WpfToolTipService.SetVerticalOffset(tb, 5.0);
		});
		Assert.Null(ex);
	}

	[Fact]
	public void ToolTipService_all_new_props_persist_before_realize()
	{
		var tb     = new WpfTextBlock();
		var target = new WpfTextBlock();
		WpfToolTipService.SetInitialShowDelay(tb, 200);
		WpfToolTipService.SetShowDuration(tb, 2000);
		WpfToolTipService.SetBetweenShowDelay(tb, 50);
		WpfToolTipService.SetIsEnabled(tb, false);
		WpfToolTipService.SetShowOnDisabled(tb, true);
		WpfToolTipService.SetPlacement(tb, WpfPlacementMode.Bottom);
		WpfToolTipService.SetPlacementTarget(tb, target);
		WpfToolTipService.SetHorizontalOffset(tb, 10.0);
		WpfToolTipService.SetVerticalOffset(tb, 5.0);

		Assert.Equal(200, WpfToolTipService.GetInitialShowDelay(tb));
		Assert.Equal(2000, WpfToolTipService.GetShowDuration(tb));
		Assert.Equal(50, WpfToolTipService.GetBetweenShowDelay(tb));
		Assert.False(WpfToolTipService.GetIsEnabled(tb));
		Assert.True(WpfToolTipService.GetShowOnDisabled(tb));
		Assert.Equal(WpfPlacementMode.Bottom, WpfToolTipService.GetPlacement(tb));
		Assert.Same(target, WpfToolTipService.GetPlacementTarget(tb));
		Assert.Equal(10.0, WpfToolTipService.GetHorizontalOffset(tb));
		Assert.Equal(5.0, WpfToolTipService.GetVerticalOffset(tb));
	}
}
