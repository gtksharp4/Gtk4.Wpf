using System;
using System.Windows;
using System.Windows.Automation;
using Auto = System.Windows.Automation.Automation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.AutomationClient;

public class ElementPropertyTests : UiaTestBase
{
	[Fact]
	public void Current_reports_the_peer_state() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok").Current;
		Assert.Equal("Ok", ok.AutomationId);
		Assert.Equal("OK", ok.Name);
		Assert.Equal("Button", ok.ClassName);
		Assert.Equal(ControlType.Button, ok.ControlType);
		Assert.Equal("button", ok.LocalizedControlType);
		Assert.True(ok.IsEnabled);
		Assert.True(ok.IsKeyboardFocusable);
		Assert.False(ok.IsOffscreen);
		Assert.False(ok.IsPassword);
		Assert.Equal("WPF", ok.FrameworkId);
		Assert.Equal(System.Diagnostics.Process.GetCurrentProcess().Id, ok.ProcessId);
		Assert.True(ok.BoundingRectangle.Width > 0);
		Assert.False(Find("Off").Current.IsEnabled);
	});

	[Fact]
	public void GetCurrentPropertyValue_reads_element_and_pattern_properties() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok");
		Assert.Equal("OK", ok.GetCurrentPropertyValue(AutomationElement.NameProperty));
		Assert.Equal(ControlType.Button, ok.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty));
		var slider = Find("Level");
		Assert.Equal(2.0, slider.GetCurrentPropertyValue(RangeValuePattern.ValueProperty));
		Assert.Equal(10.0, slider.GetCurrentPropertyValue(RangeValuePattern.MaximumProperty));
	});

	[Fact]
	public void Unsupported_pattern_property_is_NotSupported_when_defaults_are_ignored() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok");
		Assert.Same(AutomationElement.NotSupported,
			ok.GetCurrentPropertyValue(ValuePattern.ValueProperty, true));
		Assert.Equal(string.Empty, ok.GetCurrentPropertyValue(ValuePattern.ValueProperty, false));
		Assert.Throws<ArgumentNullException>(() => ok.GetCurrentPropertyValue(null!));
	});

	[Fact]
	public void Cached_without_a_CacheRequest_throws() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok");
		Assert.Throws<InvalidOperationException>(() => ok.Cached.Name);
		Assert.Throws<InvalidOperationException>(() => ok.GetCachedPropertyValue(AutomationElement.NameProperty));
	});

	[Fact]
	public void FromHandle_is_not_supported() => Run(() =>
	{
		Assert.Throws<PlatformNotSupportedException>(() => AutomationElement.FromHandle(new IntPtr(1)));
	});

	[Fact]
	public void Elements_of_the_same_peer_are_equal_and_have_a_runtime_id() => Run(() =>
	{
		OpenForm();
		var a = Find("Ok");
		var b = Find("Ok");
		Assert.Equal(a, b);
		Assert.True(a == b);
		Assert.Equal(a.GetHashCode(), b.GetHashCode());
		Assert.NotEqual(a, Find("Off"));
		Assert.True(Auto.Compare(a, b));
		Assert.Equal(a.GetRuntimeId(), b.GetRuntimeId());
		Assert.NotEqual(a.GetRuntimeId(), Find("Off").GetRuntimeId());
	});

	[Fact]
	public void SetFocus_moves_FocusedElement() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok");
		ok.SetFocus();
		Assert.Equal(ok, AutomationElement.FocusedElement);
		Assert.True(ok.Current.HasKeyboardFocus);
		Find("Name").SetFocus();
		Assert.Equal("Name", AutomationElement.FocusedElement.Current.AutomationId);
	});

	[Fact]
	public void SetFocus_on_a_non_focusable_element_throws() => Run(() =>
	{
		OpenForm();
		Assert.Throws<InvalidOperationException>(() => Find("Off").SetFocus());
	});

	[Fact]
	public void FromPoint_finds_the_element_under_a_point_and_clickable_point_is_inside_it() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok");
		Assert.True(ok.TryGetClickablePoint(out Point p));
		var r = ok.Current.BoundingRectangle;
		Assert.True(p.X >= r.X && p.X <= r.X + r.Width && p.Y >= r.Y && p.Y <= r.Y + r.Height);
		Assert.Equal(p, ok.GetClickablePoint());
		Assert.Equal(ok, AutomationElement.FromPoint(p));
	});

	[Fact]
	public void Offscreen_element_has_no_clickable_point() => Run(() =>
	{
		var f = OpenForm();
		var ok = Find("Ok");
		f.Ok.Visibility = Visibility.Collapsed;
		Assert.False(ok.TryGetClickablePoint(out _));
		Assert.Throws<NoClickablePointException>(() => ok.GetClickablePoint());
	});

	[Fact]
	public void A_closed_window_makes_its_elements_unavailable() => Run(() =>
	{
		var f = OpenForm();
		var ok = Find("Ok");
		f.Window.Close();
		Assert.Throws<ElementNotAvailableException>(() => ok.Current.Name);
		Assert.Throws<ElementNotAvailableException>(() => ok.SetFocus());
		Assert.Null(Find("Ok"));
	});

	[Fact]
	public void FromLocalProvider_wraps_a_peer() => Run(() =>
	{
		var f = OpenForm();
		var peer = System.Windows.Automation.Peers.UIElementAutomationPeer.CreatePeerForElement(f.Ok)!;
		Assert.Equal(Find("Ok"), AutomationElement.FromLocalProvider(peer.ProviderFromPeer(peer)));
	});

	[Fact]
	public void Supported_patterns_and_properties_are_listed() => Run(() =>
	{
		OpenForm();
		Assert.Contains(InvokePattern.Pattern, Find("Ok").GetSupportedPatterns());
		Assert.Contains(TogglePattern.Pattern, Find("Agree").GetSupportedPatterns());
		Assert.DoesNotContain(TogglePattern.Pattern, Find("Ok").GetSupportedPatterns());
		Assert.Contains(AutomationElement.NameProperty, Find("Ok").GetSupportedProperties());
	});
}
