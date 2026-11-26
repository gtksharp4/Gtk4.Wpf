using System;
using System.Windows.Automation;
using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Driver;

public class ByTests
{
	[Fact]
	public void AutomationId_builds_a_property_condition_and_describes_itself()
	{
		var by = By.AutomationId("Ok");
		var condition = Assert.IsType<PropertyCondition>(by.Condition);
		Assert.Equal(AutomationElement.AutomationIdProperty, condition.Property);
		Assert.Equal("Ok", condition.Value);
		Assert.Equal("AutomationId == \"Ok\"", by.Description);
	}

	[Fact]
	public void Name_can_ignore_case()
	{
		var condition = Assert.IsType<PropertyCondition>(By.Name("ok", ignoreCase: true).Condition);
		Assert.Equal(PropertyConditionFlags.IgnoreCase, condition.Flags);
	}

	[Fact]
	public void ControlType_and_ClassName_target_their_properties()
	{
		Assert.Equal(AutomationElement.ControlTypeProperty,
			Assert.IsType<PropertyCondition>(By.ControlType(ControlType.Button).Condition).Property);
		Assert.Equal(AutomationElement.ClassNameProperty,
			Assert.IsType<PropertyCondition>(By.ClassName("Button").Condition).Property);
	}

	[Fact]
	public void And_combines_conditions_and_descriptions()
	{
		var by = By.ControlType(ControlType.Button).And(By.Name("OK"));
		Assert.IsType<AndCondition>(by.Condition);
		Assert.Equal("ControlType == ControlType.Button AND Name == \"OK\"", by.Description);
	}

	[Fact]
	public void AnyOf_builds_an_or_condition()
	{
		var by = By.AnyOf(By.Name("a"), By.Name("b"));
		Assert.IsType<OrCondition>(by.Condition);
		Assert.Equal("(Name == \"a\" OR Name == \"b\")", by.Description);
	}

	[Fact]
	public void A_single_locator_is_not_wrapped()
	{
		var one = By.Name("a");
		Assert.Same(one.Condition, By.AllOf(one).Condition);
	}

	[Fact]
	public void No_locators_throw()
	{
		Assert.Throws<ArgumentException>(() => By.AllOf());
		Assert.Throws<ArgumentException>(() => By.AnyOf(new By[] { null }));
	}

	[Fact]
	public void A_locator_converts_to_its_condition()
	{
		System.Windows.Automation.Condition condition = By.Name("x");
		Assert.IsType<PropertyCondition>(condition);
	}
}
