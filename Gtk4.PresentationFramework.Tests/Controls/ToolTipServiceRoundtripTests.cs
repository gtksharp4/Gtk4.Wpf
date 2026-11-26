using Xunit;
using WpfPlacementMode  = System.Windows.Controls.PlacementMode;
using WpfTextBlock      = System.Windows.Controls.TextBlock;
using WpfToolTipService = System.Windows.Controls.ToolTipService;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>Round-trip tests for the ToolTipService timing/placement attached DPs.</summary>
public class ToolTipServiceRoundtripTests
{
	[Fact]
	public void InitialShowDelay_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetInitialShowDelay(tb, 400);
		Assert.Equal(400, WpfToolTipService.GetInitialShowDelay(tb));
	}

	[Fact]
	public void ShowDuration_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetShowDuration(tb, 3000);
		Assert.Equal(3000, WpfToolTipService.GetShowDuration(tb));
	}

	[Fact]
	public void BetweenShowDelay_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetBetweenShowDelay(tb, 50);
		Assert.Equal(50, WpfToolTipService.GetBetweenShowDelay(tb));
	}

	[Fact]
	public void IsEnabled_roundtrip_false()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetIsEnabled(tb, false);
		Assert.False(WpfToolTipService.GetIsEnabled(tb));
	}

	[Fact]
	public void IsEnabled_roundtrip_true_after_false()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetIsEnabled(tb, false);
		WpfToolTipService.SetIsEnabled(tb, true);
		Assert.True(WpfToolTipService.GetIsEnabled(tb));
	}

	[Fact]
	public void ShowOnDisabled_roundtrip_true()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetShowOnDisabled(tb, true);
		Assert.True(WpfToolTipService.GetShowOnDisabled(tb));
	}

	[Fact]
	public void Placement_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetPlacement(tb, WpfPlacementMode.Bottom);
		Assert.Equal(WpfPlacementMode.Bottom, WpfToolTipService.GetPlacement(tb));
	}

	[Fact]
	public void PlacementTarget_roundtrip()
	{
		var tb     = new WpfTextBlock();
		var target = new WpfTextBlock();
		WpfToolTipService.SetPlacementTarget(tb, target);
		Assert.Same(target, WpfToolTipService.GetPlacementTarget(tb));
	}

	[Fact]
	public void HorizontalOffset_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetHorizontalOffset(tb, 12.5);
		Assert.Equal(12.5, WpfToolTipService.GetHorizontalOffset(tb));
	}

	[Fact]
	public void VerticalOffset_roundtrip()
	{
		var tb = new WpfTextBlock();
		WpfToolTipService.SetVerticalOffset(tb, -4.0);
		Assert.Equal(-4.0, WpfToolTipService.GetVerticalOffset(tb));
	}
}
