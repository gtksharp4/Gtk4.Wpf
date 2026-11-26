using System;
using System.Collections.Generic;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Auto = System.Windows.Automation.Automation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.AutomationClient;

public class ClientParityTests : UiaTestBase
{
	static List<AutomationEventArgs> Watch(AutomationElement scope, AutomationEvent id)
	{
		var seen = new List<AutomationEventArgs>();
		Auto.AddAutomationEventHandler(id, scope, TreeScope.Subtree, (s, e) => seen.Add(e));
		return seen;
	}

	[Fact]
	public void Clicking_a_CheckBox_raises_no_Invoked_event() => Run(() =>
	{
		var f = OpenForm();
		var seen = Watch(Find("MainWindow"), InvokePattern.InvokedEvent);
		f.Agree.AutomationClick();
		Assert.Empty(seen);
		f.Ok.AutomationClick();
		Assert.Single(seen);
	});

	[Fact]
	public void Clicking_a_RepeatButton_raises_Invoked_once() => Run(() =>
	{
		var f = OpenForm();
		var repeat = new RepeatButton { Content = "Hold" };
		f.Panel.Children.Add(repeat);
		AutomationProperties.SetAutomationId(repeat, "Hold");
		GtkPump.Until(() => repeat.Widget.Width > 0);
		var seen = Watch(Find("MainWindow"), InvokePattern.InvokedEvent);
		repeat.AutomationClick();
		Assert.Single(seen);
	});

	[Fact]
	public void Unsupported_pattern_properties_report_the_WPF_schema_defaults() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok");
		Assert.Equal(true, ok.GetCurrentPropertyValue(ValuePattern.IsReadOnlyProperty));
		Assert.Equal(true, ok.GetCurrentPropertyValue(RangeValuePattern.IsReadOnlyProperty));
		Assert.Equal(ToggleState.Indeterminate, ok.GetCurrentPropertyValue(TogglePattern.ToggleStateProperty));
		Assert.Equal(ExpandCollapseState.LeafNode,
			ok.GetCurrentPropertyValue(ExpandCollapsePattern.ExpandCollapseStateProperty));
		var selection = Assert.IsType<AutomationElement[]>(
			ok.GetCurrentPropertyValue(SelectionPattern.SelectionProperty));
		Assert.Empty(selection);
		Assert.Same(AutomationElement.NotSupported,
			ok.GetCurrentPropertyValue(ValuePattern.IsReadOnlyProperty, true));
	});

	[Fact]
	public void Cached_unsupported_property_reports_the_schema_default() => Run(() =>
	{
		OpenForm();
		var request = new CacheRequest();
		request.Add(ValuePattern.IsReadOnlyProperty);
		var ok = Find("Ok").GetUpdatedCache(request);
		Assert.Equal(true, ok.GetCachedPropertyValue(ValuePattern.IsReadOnlyProperty));
	});

	[Fact]
	public void PropertyCondition_accepts_null_for_reference_types_and_NotSupported_for_any() => Run(() =>
	{
		OpenForm();
		var nullName = new PropertyCondition(AutomationElement.NameProperty, null);
		var nullIgnoreCase = new PropertyCondition(AutomationElement.NameProperty, null,
			PropertyConditionFlags.IgnoreCase);
		Assert.Null(nullName.Value);
		Assert.Null(nullIgnoreCase.Value);
		Assert.Throws<ArgumentException>(() => new PropertyCondition(AutomationElement.IsEnabledProperty, null));
		var unsupported = new PropertyCondition(ValuePattern.ValueProperty, AutomationElement.NotSupported);
		Assert.Same(AutomationElement.NotSupported, unsupported.Value);
		var ok = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
			new AndCondition(unsupported, new PropertyCondition(AutomationElement.AutomationIdProperty, "Ok")));
		Assert.NotNull(ok);
		Assert.Null(AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
			new AndCondition(unsupported, new PropertyCondition(AutomationElement.AutomationIdProperty, "Name"))));
	});

	[Fact]
	public void AsyncContentLoaded_reaches_the_client_with_its_own_args() => Run(() =>
	{
		var f = OpenForm();
		var seen = Watch(Find("MainWindow"), AutomationElement.AsyncContentLoadedEvent);
		UIElementAutomationPeer.CreatePeerForElement(f.Ok)!.RaiseAsyncContentLoadedEvent(
			new AsyncContentLoadedEventArgs(AsyncContentLoadedState.Progress, 50));
		var args = Assert.IsType<AsyncContentLoadedEventArgs>(Assert.Single(seen));
		Assert.Equal(AsyncContentLoadedState.Progress, args.AsyncContentLoadedState);
		Assert.Equal(50, args.PercentComplete);
	});

	[Fact]
	public void GetSupportedProperties_throws_for_a_closed_window() => Run(() =>
	{
		var f = OpenForm();
		var ok = Find("Ok");
		f.Window.Close();
		Assert.Throws<ElementNotAvailableException>(() => ok.GetSupportedProperties());
	});
}
