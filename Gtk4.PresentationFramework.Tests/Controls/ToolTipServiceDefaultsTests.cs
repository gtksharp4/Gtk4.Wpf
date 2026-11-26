using Xunit;
using WpfPlacementMode  = System.Windows.Controls.PlacementMode;
using WpfTextBlock      = System.Windows.Controls.TextBlock;
using WpfToolTipService = System.Windows.Controls.ToolTipService;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Default values + headless no-throw for the ToolTipService timing/placement attached DPs.
/// </summary>
public class ToolTipServiceDefaultsTests
{
	[Fact]
	public void InitialShowDelay_default_1000()
	{
		Assert.Equal(1000, WpfToolTipService.GetInitialShowDelay(new WpfTextBlock()));
	}

	[Fact]
	public void ShowDuration_default_5000()
	{
		Assert.Equal(5000, WpfToolTipService.GetShowDuration(new WpfTextBlock()));
	}

	[Fact]
	public void BetweenShowDelay_default_100()
	{
		Assert.Equal(100, WpfToolTipService.GetBetweenShowDelay(new WpfTextBlock()));
	}

	[Fact]
	public void IsEnabled_default_true()
	{
		Assert.True(WpfToolTipService.GetIsEnabled(new WpfTextBlock()));
	}

	[Fact]
	public void ShowOnDisabled_default_false()
	{
		Assert.False(WpfToolTipService.GetShowOnDisabled(new WpfTextBlock()));
	}

	[Fact]
	public void Placement_default_Mouse()
	{
		Assert.Equal(WpfPlacementMode.Mouse, WpfToolTipService.GetPlacement(new WpfTextBlock()));
	}

	[Fact]
	public void PlacementTarget_default_null()
	{
		Assert.Null(WpfToolTipService.GetPlacementTarget(new WpfTextBlock()));
	}

	[Fact]
	public void HorizontalOffset_default_zero()
	{
		Assert.Equal(0.0, WpfToolTipService.GetHorizontalOffset(new WpfTextBlock()));
	}

	[Fact]
	public void VerticalOffset_default_zero()
	{
		Assert.Equal(0.0, WpfToolTipService.GetVerticalOffset(new WpfTextBlock()));
	}

	[Fact]
	public void All_new_props_before_realize_no_throw_and_persist()
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
		Assert.Equal(200, WpfToolTipService.GetInitialShowDelay(tb));
		Assert.False(WpfToolTipService.GetIsEnabled(tb));
		Assert.Equal(WpfPlacementMode.Bottom, WpfToolTipService.GetPlacement(tb));
		Assert.Same(target, WpfToolTipService.GetPlacementTarget(tb));
		Assert.Equal(5.0, WpfToolTipService.GetVerticalOffset(tb));
	}
}
