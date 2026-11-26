using System;
using System.Collections.Generic;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Auto = System.Windows.Automation.Automation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.AutomationClient;

public class AutomationEventTests : UiaTestBase
{
	[Fact]
	public void Invoked_event_is_raised_for_a_click_on_the_element() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok");
		var seen = new List<AutomationEventArgs>();
		AutomationEventHandler handler = (s, e) => seen.Add(e);
		Auto.AddAutomationEventHandler(InvokePattern.InvokedEvent, ok, TreeScope.Element, handler);
		((InvokePattern)ok.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
		Assert.Single(seen);
		Assert.Same(InvokePattern.InvokedEvent, seen[0].EventId);
		Auto.RemoveAutomationEventHandler(InvokePattern.InvokedEvent, ok, handler);
		((InvokePattern)ok.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
		Assert.Single(seen);
	});

	[Fact]
	public void Scope_limits_which_elements_report() => Run(() =>
	{
		OpenForm();
		int onPanelChildren = 0, onPanelElement = 0, onWindowSubtree = 0;
		var panel = Find("Panel");
		Auto.AddAutomationEventHandler(InvokePattern.InvokedEvent, panel, TreeScope.Element,
			(s, e) => onPanelElement++);
		Auto.AddAutomationEventHandler(InvokePattern.InvokedEvent, panel, TreeScope.Children,
			(s, e) => onPanelChildren++);
		Auto.AddAutomationEventHandler(InvokePattern.InvokedEvent, Find("MainWindow"), TreeScope.Subtree,
			(s, e) => onWindowSubtree++);
		((InvokePattern)Find("Ok").GetCurrentPattern(InvokePattern.Pattern)).Invoke();
		Assert.Equal((0, 1, 1), (onPanelElement, onPanelChildren, onWindowSubtree));
		Auto.RemoveAllEventHandlers();
		((InvokePattern)Find("Ok").GetCurrentPattern(InvokePattern.Pattern)).Invoke();
		Assert.Equal((0, 1, 1), (onPanelElement, onPanelChildren, onWindowSubtree));
		Assert.False(AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked));
	});

	[Fact]
	public void Property_changed_reports_old_and_new_values_for_the_requested_properties() => Run(() =>
	{
		var f = OpenForm();
		var agree = Find("Agree");
		var seen = new List<AutomationPropertyChangedEventArgs>();
		AutomationPropertyChangedEventHandler handler = (s, e) => seen.Add(e);
		Auto.AddAutomationPropertyChangedEventHandler(agree, TreeScope.Element, handler,
			TogglePattern.ToggleStateProperty);
		f.Agree.IsChecked = true;
		var change = Assert.Single(seen);
		Assert.Same(TogglePattern.ToggleStateProperty, change.Property);
		Assert.Equal(ToggleState.Off, change.OldValue);
		Assert.Equal(ToggleState.On, change.NewValue);
		Assert.Same(AutomationElement.AutomationPropertyChangedEvent, change.EventId);
		Auto.RemoveAutomationPropertyChangedEventHandler(agree, handler);
		f.Agree.IsChecked = false;
		Assert.Single(seen);
	});

	[Fact]
	public void Property_changed_ignores_properties_that_were_not_requested() => Run(() =>
	{
		var f = OpenForm();
		int count = 0;
		Auto.AddAutomationPropertyChangedEventHandler(Find("Agree"), TreeScope.Element, (s, e) => count++,
			AutomationElement.NameProperty);
		try
		{
			f.Agree.IsChecked = true;
			Assert.Equal(0, count);
		}
		finally { Auto.RemoveAllEventHandlers(); }
	});

	[Fact]
	public void Property_changed_requires_properties() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok");
		Assert.Throws<ArgumentException>(() =>
			Auto.AddAutomationPropertyChangedEventHandler(ok, TreeScope.Element, (s, e) => { }));
		Assert.Throws<ArgumentNullException>(() =>
			Auto.AddAutomationPropertyChangedEventHandler(ok, TreeScope.Element, null!,
				AutomationElement.NameProperty));
	});

	[Fact]
	public void Structure_changed_is_raised_when_a_peer_is_invalidated() => Run(() =>
	{
		var f = OpenForm();
		var panel = Find("Panel");
		var seen = new List<StructureChangedEventArgs>();
		StructureChangedEventHandler handler = (s, e) => seen.Add(e);
		Auto.AddStructureChangedEventHandler(panel, TreeScope.Element, handler);
		UIElementPeer(f.Panel).InvalidatePeer();
		Assert.Single(seen);
		Assert.Equal(StructureChangeType.ChildrenInvalidated, seen[0].StructureChangeType);
		Assert.Equal(panel.GetRuntimeId(), seen[0].GetRuntimeId());
		Auto.RemoveStructureChangedEventHandler(panel, handler);
		UIElementPeer(f.Panel).InvalidatePeer();
		Assert.Single(seen);
	});

	static AutomationPeer UIElementPeer(System.Windows.UIElement e) =>
		UIElementAutomationPeer.CreatePeerForElement(e)!;

	[Fact]
	public void Focus_changed_reports_the_newly_focused_element() => Run(() =>
	{
		var f = OpenForm();
		// Activation (which focuses the window) is up to the window manager and can land late (macOS):
		// let it settle so it is not reported as the first focus change.
		GtkPump.Until(() => f.Window.Widget is Gtk.Window { IsActive: true }, 2000);
		GtkPump.Until(() => !GLib.MainContext.Pending(), 500);
		var seen = new List<AutomationElement>();
		AutomationFocusChangedEventHandler handler = (s, e) => seen.Add((AutomationElement)s);
		Auto.AddAutomationFocusChangedEventHandler(handler);
		Find("Ok").SetFocus();
		Find("Name").SetFocus();
		Auto.RemoveAutomationFocusChangedEventHandler(handler);
		Find("Ok").SetFocus();
		Assert.Equal(new[] { "Ok", "Name" }, seen.ConvertAll(e => e.Current.AutomationId));
	});

	[Fact]
	public void Handlers_that_throw_do_not_break_the_raiser() => Run(() =>
	{
		var f = OpenForm();
		var ok = Find("Ok");
		int after = 0;
		Auto.AddAutomationEventHandler(InvokePattern.InvokedEvent, ok, TreeScope.Element,
			(s, e) => throw new InvalidOperationException("boom"));
		Auto.AddAutomationEventHandler(InvokePattern.InvokedEvent, ok, TreeScope.Element, (s, e) => after++);
		((InvokePattern)ok.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
		Assert.Equal(1, f.Clicks);
		Assert.Equal(1, after);
		Auto.RemoveAllEventHandlers();
	});

	[Fact]
	public void Property_changed_and_structure_events_cannot_use_the_generic_overload() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok");
		Assert.Throws<ArgumentException>(() => Auto.AddAutomationEventHandler(
			AutomationElement.AutomationPropertyChangedEvent, ok, TreeScope.Element, (s, e) => { }));
		Assert.Throws<ArgumentException>(() => Auto.AddAutomationEventHandler(
			AutomationElement.StructureChangedEvent, ok, TreeScope.Element, (s, e) => { }));
	});
}
