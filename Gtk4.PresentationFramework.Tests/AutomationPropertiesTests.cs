using System;
using System.Windows;
using System.Windows.Automation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure (no GTK) tests for <see cref="AutomationProperties"/> attached-DP get/set
/// round-trips and default values.
/// </summary>
public class AutomationPropertiesTests : AutomationTestBase
{
	// ── Name ──────────────────────────────────────────────────────────────────────

	[Fact]
	public void Name_default_is_null_on_UIElement()
	{
		var e = new StubElement();
		Assert.Null(AutomationProperties.GetName(e));
	}

	[Fact]
	public void Name_roundtrip_on_UIElement()
	{
		var e = new StubElement();
		AutomationProperties.SetName(e, "My Label");
		Assert.Equal("My Label", AutomationProperties.GetName(e));
	}

	[Fact]
	public void Name_roundtrip_on_plain_DependencyObject()
	{
		var d = new PlainDO();
		AutomationProperties.SetName(d, "Plain");
		Assert.Equal("Plain", AutomationProperties.GetName(d));
	}

	[Fact]
	public void Name_null_argument_throws()
	{
		Assert.Throws<ArgumentNullException>(() => AutomationProperties.GetName(null!));
		Assert.Throws<ArgumentNullException>(() => AutomationProperties.SetName(null!, "x"));
	}

	// ── HelpText ──────────────────────────────────────────────────────────────────

	[Fact]
	public void HelpText_default_is_null()
	{
		var e = new StubElement();
		Assert.Null(AutomationProperties.GetHelpText(e));
	}

	[Fact]
	public void HelpText_roundtrip_on_UIElement()
	{
		var e = new StubElement();
		AutomationProperties.SetHelpText(e, "Enter your name");
		Assert.Equal("Enter your name", AutomationProperties.GetHelpText(e));
	}

	[Fact]
	public void HelpText_roundtrip_on_plain_DependencyObject()
	{
		var d = new PlainDO();
		AutomationProperties.SetHelpText(d, "help");
		Assert.Equal("help", AutomationProperties.GetHelpText(d));
	}

	// ── AutomationId ─────────────────────────────────────────────────────────────

	[Fact]
	public void AutomationId_default_is_null()
	{
		var e = new StubElement();
		Assert.Null(AutomationProperties.GetAutomationId(e));
	}

	[Fact]
	public void AutomationId_roundtrip()
	{
		var e = new StubElement();
		AutomationProperties.SetAutomationId(e, "btn-submit");
		Assert.Equal("btn-submit", AutomationProperties.GetAutomationId(e));
	}

	[Fact]
	public void AutomationId_roundtrip_on_plain_DependencyObject()
	{
		var d = new PlainDO();
		AutomationProperties.SetAutomationId(d, "id42");
		Assert.Equal("id42", AutomationProperties.GetAutomationId(d));
	}

	// ── LabeledBy ────────────────────────────────────────────────────────────────

	[Fact]
	public void LabeledBy_default_is_null()
	{
		var e = new StubElement();
		Assert.Null(AutomationProperties.GetLabeledBy(e));
	}

	[Fact]
	public void LabeledBy_roundtrip()
	{
		var input = new StubElement();
		var label = new StubElement();
		AutomationProperties.SetLabeledBy(input, label);
		Assert.Same(label, AutomationProperties.GetLabeledBy(input));
	}

	// ── IsRequiredForForm ─────────────────────────────────────────────────────────

	[Fact]
	public void IsRequiredForForm_default_is_false()
	{
		var e = new StubElement();
		Assert.False(AutomationProperties.GetIsRequiredForForm(e));
	}

	[Fact]
	public void IsRequiredForForm_roundtrip_true()
	{
		var e = new StubElement();
		AutomationProperties.SetIsRequiredForForm(e, true);
		Assert.True(AutomationProperties.GetIsRequiredForForm(e));
	}

	[Fact]
	public void IsRequiredForForm_roundtrip_on_plain_DependencyObject()
	{
		var d = new PlainDO();
		AutomationProperties.SetIsRequiredForForm(d, true);
		Assert.True(AutomationProperties.GetIsRequiredForForm(d));
	}

	// ── LiveSetting ───────────────────────────────────────────────────────────────

	[Fact]
	public void LiveSetting_default_is_Off()
	{
		var e = new StubElement();
		Assert.Equal(AutomationLiveSetting.Off, AutomationProperties.GetLiveSetting(e));
	}

	[Fact]
	public void LiveSetting_roundtrip_Polite()
	{
		var e = new StubElement();
		AutomationProperties.SetLiveSetting(e, AutomationLiveSetting.Polite);
		Assert.Equal(AutomationLiveSetting.Polite, AutomationProperties.GetLiveSetting(e));
	}

	[Fact]
	public void LiveSetting_roundtrip_Assertive()
	{
		var e = new StubElement();
		AutomationProperties.SetLiveSetting(e, AutomationLiveSetting.Assertive);
		Assert.Equal(AutomationLiveSetting.Assertive, AutomationProperties.GetLiveSetting(e));
	}

	[Fact]
	public void LiveSetting_roundtrip_on_plain_DependencyObject()
	{
		var d = new PlainDO();
		AutomationProperties.SetLiveSetting(d, AutomationLiveSetting.Assertive);
		Assert.Equal(AutomationLiveSetting.Assertive, AutomationProperties.GetLiveSetting(d));
	}

	// ── Set before realize does not throw ─────────────────────────────────────────

	[Fact]
	public void Setting_before_widget_realize_does_not_throw()
	{
		var e = new StubElement();
		// None of these touch the widget — no CreateWidget call, no throw.
		var ex = Record.Exception(() =>
		{
			AutomationProperties.SetName(e, "X");
			AutomationProperties.SetHelpText(e, "Y");
			AutomationProperties.SetAutomationId(e, "Z");
			AutomationProperties.SetIsRequiredForForm(e, true);
			AutomationProperties.SetLiveSetting(e, AutomationLiveSetting.Polite);
		});
		Assert.Null(ex);
	}
}
