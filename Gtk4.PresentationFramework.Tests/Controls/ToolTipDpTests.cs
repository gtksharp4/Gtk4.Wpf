using Xunit;
using WpfFrameworkElement = System.Windows.FrameworkElement;
using WpfPlacementMode    = System.Windows.Controls.PlacementMode;
using WpfTextBlock        = System.Windows.Controls.TextBlock;
using WpfToolTip          = System.Windows.Controls.ToolTip;
using WpfToolTipService   = System.Windows.Controls.ToolTipService;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure DP tests for the <see cref="WpfToolTip"/> control, the
/// <see cref="WpfFrameworkElement"/>.ToolTip DP, and the ToolTipService passthrough.
/// (ToolTipService timing/placement DPs live in the ToolTipService*Tests files.)
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
}
