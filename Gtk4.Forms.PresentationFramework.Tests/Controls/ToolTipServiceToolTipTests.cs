using System;
using System.Windows;
using Xunit;
using WpfButton = System.Windows.Controls.Button;
using WpfToolTipService = System.Windows.Controls.ToolTipService;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

public class ToolTipServiceToolTipTests : WpfGtkTestBase
{
	[Fact]
	public void ToolTipProperty_is_owned_by_the_service_and_shared_with_FrameworkElement()
	{
		Assert.Equal("ToolTip", WpfToolTipService.ToolTipProperty.Name);
		Assert.Equal(typeof(WpfToolTipService), WpfToolTipService.ToolTipProperty.OwnerType);
		Assert.Equal(typeof(object), WpfToolTipService.ToolTipProperty.PropertyType);
		Assert.Same(WpfToolTipService.ToolTipProperty, FrameworkElement.ToolTipProperty);
		Assert.Same(WpfToolTipService.ToolTipProperty, FrameworkContentElement.ToolTipProperty);
		Assert.Null(WpfToolTipService.ToolTipProperty.DefaultMetadata.DefaultValue);
	}

	[Fact]
	public void SetToolTip_round_trips_and_validates_its_argument() => Run(() =>
	{
		var button = new WpfButton();
		Assert.Null(WpfToolTipService.GetToolTip(button));
		WpfToolTipService.SetToolTip(button, "tip");
		Assert.Equal("tip", WpfToolTipService.GetToolTip(button));
		Assert.Equal("tip", button.ToolTip);
		Assert.Throws<ArgumentNullException>(() => WpfToolTipService.GetToolTip(null!));
		Assert.Throws<ArgumentNullException>(() => WpfToolTipService.SetToolTip(null!, "x"));
	});

	[Fact]
	public void SetToolTip_on_a_button_shows_the_gtk_tooltip() => Run(() =>
	{
		var button = new WpfButton();
		WpfToolTipService.SetToolTip(button, "service tip");
		var widget = button.Widget;
		Assert.Equal("service tip", widget.TooltipText);
		Assert.True(widget.HasTooltip);
	});

	[Fact]
	public void SetToolTip_on_a_realized_element_updates_and_clears_the_gtk_tooltip() => Run(() =>
	{
		var text = new System.Windows.Controls.TextBlock();
		var widget = text.Widget;
		WpfToolTipService.SetToolTip(text, "service tip");
		Assert.Equal("service tip", widget.TooltipText);
		WpfToolTipService.SetToolTip(text, null);
		Assert.False(widget.HasTooltip);
	});

	[Fact]
	public void A_service_tooltip_set_before_the_widget_exists_is_applied_on_creation() => Run(() =>
	{
		var button = new WpfButton();
		WpfToolTipService.SetToolTip(button, "early");
		Assert.Equal("early", button.Widget.TooltipText);
	});

	[Fact]
	public void ToolTipService_IsEnabled_false_still_suppresses_a_service_tooltip() => Run(() =>
	{
		var button = new WpfButton();
		WpfToolTipService.SetIsEnabled(button, false);
		WpfToolTipService.SetToolTip(button, "hidden");
		var widget = button.Widget;
		Assert.False(widget.HasTooltip);
	});
}
