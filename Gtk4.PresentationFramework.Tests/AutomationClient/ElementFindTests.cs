using System;
using System.Windows.Automation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.AutomationClient;

public class ElementFindTests : UiaTestBase
{
	[Fact]
	public void RootElement_children_are_the_application_windows() => Run(() =>
	{
		var f = OpenForm();
		var windows = AutomationElement.RootElement.FindAll(TreeScope.Children, Condition.TrueCondition);
		Assert.Single(windows);
		Assert.Equal("Main", windows[0].Current.Name);
		Assert.Equal(ControlType.Window, windows[0].Current.ControlType);
		Assert.Equal(ControlType.Pane, AutomationElement.RootElement.Current.ControlType);
		Assert.Equal(f.Window.Title, windows[0].Current.Name);
	});

	[Fact]
	public void FindFirst_by_AutomationId_as_in_a_WPF_UIA_test() => Run(() =>
	{
		OpenForm();
		var ok = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
			new PropertyCondition(AutomationElement.AutomationIdProperty, "Ok"));
		Assert.NotNull(ok);
		Assert.Equal("OK", ok.Current.Name);
		Assert.Equal(ControlType.Button, ok.Current.ControlType);
	});

	[Fact]
	public void FindFirst_without_a_match_returns_null_and_FindAll_an_empty_collection() => Run(() =>
	{
		OpenForm();
		var none = new PropertyCondition(AutomationElement.AutomationIdProperty, "Missing");
		Assert.Null(AutomationElement.RootElement.FindFirst(TreeScope.Subtree, none));
		Assert.Empty(AutomationElement.RootElement.FindAll(TreeScope.Subtree, none));
	});

	[Fact]
	public void Scopes_select_the_element_children_and_descendants() => Run(() =>
	{
		OpenForm();
		var panel = Find("Panel");
		var all = Condition.TrueCondition;
		Assert.Single(panel.FindAll(TreeScope.Element, all));
		Assert.Equal(7, panel.FindAll(TreeScope.Children, all).Count);
		Assert.True(panel.FindAll(TreeScope.Descendants, all).Count > 7);
		Assert.Equal(panel.FindAll(TreeScope.Descendants, all).Count + 1, panel.FindAll(TreeScope.Subtree, all).Count);
		Assert.Equal(panel, panel.FindFirst(TreeScope.Subtree, all));
	});

	[Fact]
	public void Parent_and_Ancestors_scopes_are_rejected() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok");
		Assert.Throws<ArgumentException>(() => ok.FindFirst(TreeScope.Parent, Condition.TrueCondition));
		Assert.Throws<ArgumentException>(() => ok.FindAll(TreeScope.Ancestors, Condition.TrueCondition));
		Assert.Throws<ArgumentNullException>(() => ok.FindAll(TreeScope.Children, null!));
	});

	[Fact]
	public void Results_come_in_document_order() => Run(() =>
	{
		OpenForm();
		var buttons = Find("Panel").FindAll(TreeScope.Children,
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));
		Assert.Equal(new[] { "Ok", "Off" }, new[] { buttons[0].Current.AutomationId, buttons[1].Current.AutomationId });
	});

	[Fact]
	public void IgnoreCase_makes_string_conditions_case_insensitive() => Run(() =>
	{
		OpenForm();
		var exact = new PropertyCondition(AutomationElement.NameProperty, "ok");
		var loose = new PropertyCondition(AutomationElement.NameProperty, "ok", PropertyConditionFlags.IgnoreCase);
		Assert.Null(Find("Panel").FindFirst(TreeScope.Descendants, exact));
		Assert.Equal("Ok", Find("Panel").FindFirst(TreeScope.Descendants, loose).Current.AutomationId);
		Assert.Equal(PropertyConditionFlags.IgnoreCase, loose.Flags);
		Assert.Equal("ok", loose.Value);
		Assert.Same(AutomationElement.NameProperty, loose.Property);
	});

	[Fact]
	public void And_Or_Not_True_False_combine() => Run(() =>
	{
		OpenForm();
		var panel = Find("Panel");
		var button = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button);
		var enabled = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
		Assert.Single(panel.FindAll(TreeScope.Children, new AndCondition(button, enabled)));
		Assert.Single(panel.FindAll(TreeScope.Children, new AndCondition(button, new NotCondition(enabled))));
		var either = new OrCondition(new PropertyCondition(AutomationElement.AutomationIdProperty, "Ok"),
			new PropertyCondition(AutomationElement.AutomationIdProperty, "Agree"));
		Assert.Equal(2, panel.FindAll(TreeScope.Children, either).Count);
		Assert.Equal(2, either.GetConditions().Length);
		Assert.Empty(panel.FindAll(TreeScope.Subtree, Condition.FalseCondition));
		Assert.Same(enabled, new NotCondition(enabled).Condition);
	});

	[Fact]
	public void Composite_conditions_validate_their_arguments() => Run(() =>
	{
		var p = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
		Assert.Throws<ArgumentException>(() => new AndCondition(p));
		Assert.Throws<ArgumentException>(() => new OrCondition(p));
		Assert.Throws<ArgumentNullException>(() => new NotCondition(null!));
		Assert.Throws<ArgumentNullException>(() => new PropertyCondition(null!, true));
		Assert.Throws<ArgumentException>(() => new PropertyCondition(AutomationElement.IsEnabledProperty, "yes"));
		Assert.Throws<ArgumentException>(() =>
			new PropertyCondition(AutomationElement.IsEnabledProperty, true, PropertyConditionFlags.IgnoreCase));
	});

	[Fact]
	public void Property_conditions_work_on_pattern_properties() => Run(() =>
	{
		var f = OpenForm();
		f.Agree.IsChecked = true;
		var on = new PropertyCondition(TogglePattern.ToggleStateProperty, ToggleState.On);
		var hit = Find("Panel").FindFirst(TreeScope.Descendants, on);
		Assert.Equal("Agree", hit.Current.AutomationId);
	});
}
