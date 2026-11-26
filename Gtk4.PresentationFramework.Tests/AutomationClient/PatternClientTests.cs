using System;
using System.Windows;
using System.Windows.Automation;
using Condition = System.Windows.Automation.Condition;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.AutomationClient;

public class PatternClientTests : UiaTestBase
{
	static T Pattern<T>(AutomationElement e, AutomationPattern p) where T : class => (T)e.GetCurrentPattern(p);

	[Fact]
	public void Invoke_as_in_a_WPF_UIA_test() => Run(() =>
	{
		var f = OpenForm();
		var ok = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
			new PropertyCondition(AutomationElement.AutomationIdProperty, "Ok"));
		((InvokePattern)ok.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
		Assert.Equal(1, f.Clicks);
	});

	[Fact]
	public void Invoke_on_a_disabled_button_throws_ElementNotEnabledException() => Run(() =>
	{
		OpenForm();
		var invoke = Pattern<InvokePattern>(Find("Off"), InvokePattern.Pattern);
		Assert.Throws<ElementNotEnabledException>(invoke.Invoke);
	});

	[Fact]
	public void An_unsupported_pattern_throws_InvalidOperationException_and_Try_returns_false() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok");
		Assert.Throws<InvalidOperationException>(() => ok.GetCurrentPattern(TogglePattern.Pattern));
		Assert.False(ok.TryGetCurrentPattern(TogglePattern.Pattern, out var none));
		Assert.Null(none);
		Assert.True(ok.TryGetCurrentPattern(InvokePattern.Pattern, out var invoke));
		Assert.IsType<InvokePattern>(invoke);
		Assert.Throws<ArgumentNullException>(() => ok.GetCurrentPattern(null!));
	});

	[Fact]
	public void Value_sets_the_text_and_reports_read_only() => Run(() =>
	{
		var f = OpenForm();
		var value = Pattern<ValuePattern>(Find("Name"), ValuePattern.Pattern);
		value.SetValue("ada");
		Assert.Equal("ada", f.Name.Text);
		Assert.Equal("ada", value.Current.Value);
		Assert.False(value.Current.IsReadOnly);
		f.Name.IsReadOnly = true;
		Assert.True(value.Current.IsReadOnly);
		Assert.Throws<ElementNotEnabledException>(() => value.SetValue("x"));
		Assert.Throws<ArgumentNullException>(() => value.SetValue(null!));
	});

	[Fact]
	public void Toggle_flips_the_checkbox() => Run(() =>
	{
		var f = OpenForm();
		var toggle = Pattern<TogglePattern>(Find("Agree"), TogglePattern.Pattern);
		Assert.Equal(ToggleState.Off, toggle.Current.ToggleState);
		toggle.Toggle();
		Assert.True(f.Agree.IsChecked);
		Assert.Equal(ToggleState.On, toggle.Current.ToggleState);
	});

	[Fact]
	public void Selection_and_SelectionItem_drive_a_list() => Run(() =>
	{
		var f = OpenForm();
		var list = Find("Items");
		var selection = Pattern<SelectionPattern>(list, SelectionPattern.Pattern);
		Assert.False(selection.Current.CanSelectMultiple);
		Assert.Empty(selection.Current.GetSelection());
		var items = list.FindAll(TreeScope.Children, Condition.TrueCondition);
		Assert.Equal(3, items.Count);
		var b = Pattern<SelectionItemPattern>(items[1], SelectionItemPattern.Pattern);
		b.Select();
		Assert.Equal("b", f.List.SelectedItem);
		Assert.True(b.Current.IsSelected);
		Assert.Equal(list, b.Current.SelectionContainer);
		Assert.Equal(items[1], Assert.Single(selection.Current.GetSelection()));
	});

	[Fact]
	public void RangeValue_sets_the_slider_value() => Run(() =>
	{
		var f = OpenForm();
		var range = Pattern<RangeValuePattern>(Find("Level"), RangeValuePattern.Pattern);
		Assert.Equal(10, range.Current.Maximum);
		Assert.Equal(0, range.Current.Minimum);
		range.SetValue(7);
		Assert.Equal(7, f.Slider.Value);
		Assert.Equal(7, range.Current.Value);
		Assert.Throws<ArgumentOutOfRangeException>(() => range.SetValue(11));
	});

	[Fact]
	public void ExpandCollapse_reads_the_combo_box_state() => Run(() =>
	{
		var f = OpenForm();
		var expand = Pattern<ExpandCollapsePattern>(Find("Choice"), ExpandCollapsePattern.Pattern);
		Assert.Equal(ExpandCollapseState.Collapsed, expand.Current.ExpandCollapseState);
		expand.Expand();
		Assert.True(f.Combo.IsDropDownOpen);
		Assert.Equal(ExpandCollapseState.Expanded, expand.Current.ExpandCollapseState);
		expand.Collapse();
		Assert.False(f.Combo.IsDropDownOpen);
	});

	[Fact]
	public void Window_pattern_reports_and_changes_the_visual_state_and_closes() => Run(() =>
	{
		var f = OpenForm();
		var window = Pattern<WindowPattern>(Find("MainWindow"), WindowPattern.Pattern);
		Assert.Equal(WindowVisualState.Normal, window.Current.WindowVisualState);
		Assert.True(window.Current.CanMaximize);
		Assert.False(window.Current.IsModal);
		window.SetWindowVisualState(WindowVisualState.Maximized);
		Assert.Equal(WindowState.Maximized, f.Window.WindowState);
		Assert.True(window.WaitForInputIdle(100));
		bool closed = false;
		f.Window.Closed += (s, e) => closed = true;
		window.Close();
		Assert.True(GtkPump.Until(() => closed));
	});

	sealed class ScrollHost : FrameworkElement
	{
		internal readonly ScrollState State = new ScrollState();
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
		protected override System.Windows.Automation.Peers.AutomationPeer? OnCreateAutomationPeer() =>
			new ScrollPeer(this);
	}

	sealed class ScrollState
	{
		public double H = 10, V = 20;
	}

	sealed class ScrollPeer : System.Windows.Automation.Peers.FrameworkElementAutomationPeer,
		System.Windows.Automation.Provider.IScrollProvider
	{
		readonly ScrollHost _host;
		public ScrollPeer(ScrollHost host) : base(host) => _host = host;

		public override object? GetPattern(System.Windows.Automation.Peers.PatternInterface p) =>
			p == System.Windows.Automation.Peers.PatternInterface.Scroll ? this : base.GetPattern(p);

		public double HorizontalScrollPercent => _host.State.H;
		public double HorizontalViewSize => 50;
		public bool HorizontallyScrollable => true;
		public double VerticalScrollPercent => _host.State.V;
		public double VerticalViewSize => 50;
		public bool VerticallyScrollable => true;

		public void Scroll(ScrollAmount h, ScrollAmount v)
		{
			if (h == ScrollAmount.LargeIncrement) _host.State.H += 10;
			if (v == ScrollAmount.SmallIncrement) _host.State.V += 1;
		}

		public void SetScrollPercent(double h, double v)
		{
			if (h != ScrollPatternIdentifiers.NoScroll) _host.State.H = h;
			if (v != ScrollPatternIdentifiers.NoScroll) _host.State.V = v;
		}
	}

	[Fact]
	public void Scroll_pattern_wraps_a_custom_peer_provider() => Run(() =>
	{
		var f = OpenForm();
		var host = new ScrollHost();
		AutomationProperties.SetAutomationId(host, "Host");
		f.Panel.Children.Add(host);
		var scroll = Pattern<ScrollPattern>(Find("Host"), ScrollPattern.Pattern);
		Assert.Equal(10, scroll.Current.HorizontalScrollPercent);
		Assert.True(scroll.Current.VerticallyScrollable);
		scroll.ScrollHorizontal(ScrollAmount.LargeIncrement);
		scroll.ScrollVertical(ScrollAmount.SmallIncrement);
		Assert.Equal(20, scroll.Current.HorizontalScrollPercent);
		Assert.Equal(21, scroll.Current.VerticalScrollPercent);
		scroll.SetScrollPercent(ScrollPattern.NoScroll, 5);
		Assert.Equal(5, scroll.Current.VerticalScrollPercent);
		Assert.Equal(20, scroll.Current.HorizontalScrollPercent);
		Assert.Equal(-1, ScrollPattern.NoScroll);
	});

	[Fact]
	public void Pattern_identifiers_match_the_provider_identifiers() => Run(() =>
	{
		Assert.Same(InvokePatternIdentifiers.Pattern, InvokePattern.Pattern);
		Assert.Same(TogglePatternIdentifiers.ToggleStateProperty, TogglePattern.ToggleStateProperty);
		Assert.Same(WindowPatternIdentifiers.WindowClosedEvent, WindowPattern.WindowClosedEvent);
		Assert.Same(SelectionItemPatternIdentifiers.ElementSelectedEvent, SelectionItemPattern.ElementSelectedEvent);
	});
}
