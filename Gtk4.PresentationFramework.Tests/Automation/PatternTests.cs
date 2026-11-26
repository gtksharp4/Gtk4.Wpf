using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

/// <summary>Patterns called directly on the providers, checked against the real GTK widgets.</summary>
public partial class PatternTests : WpfGtkTestBase
{
	static T Pattern<T>(UIElement element, PatternInterface kind) where T : class =>
		(T)UIElementAutomationPeer.CreatePeerForElement(element)!.GetPattern(kind)!;

	[Fact]
	public void Invoke_clicks_the_button() => Run(() =>
	{
		var button = new Button();
		int clicks = 0;
		button.Click += (s, e) => clicks++;
		Pattern<IInvokeProvider>(button, PatternInterface.Invoke).Invoke();
		Assert.True(GtkPump.Until(() => clicks == 1));
		Assert.Equal(1, clicks);
	});

	[Fact]
	public void Invoke_on_a_disabled_button_throws() => Run(() =>
	{
		var button = new Button { IsEnabled = false };
		var invoke = Pattern<IInvokeProvider>(button, PatternInterface.Invoke);
		Assert.Throws<ElementNotEnabledException>(invoke.Invoke);
	});

	[Fact]
	public void Button_supports_no_toggle_or_value_pattern() => Run(() =>
	{
		var peer = UIElementAutomationPeer.CreatePeerForElement(new Button())!;
		Assert.Null(peer.GetPattern(PatternInterface.Toggle));
		Assert.Null(peer.GetPattern(PatternInterface.Value));
	});

	[Fact]
	public void Toggle_flips_the_checkbox_and_the_gtk_widget() => Run(() =>
	{
		var check = new CheckBox { IsChecked = false };
		var toggle = Pattern<IToggleProvider>(check, PatternInterface.Toggle);
		Assert.Equal(ToggleState.Off, toggle.ToggleState);
		toggle.Toggle();
		Assert.True(check.IsChecked);
		Assert.Equal(ToggleState.On, toggle.ToggleState);
		Assert.True(((Gtk.CheckButton)check.Widget).Active);
		toggle.Toggle();
		Assert.False(((Gtk.CheckButton)check.Widget).Active);
	});

	[Fact]
	public void Toggle_reports_indeterminate_and_throws_when_disabled() => Run(() =>
	{
		var check = new CheckBox { IsThreeState = true, IsChecked = null };
		var toggle = Pattern<IToggleProvider>(check, PatternInterface.Toggle);
		Assert.Equal(ToggleState.Indeterminate, toggle.ToggleState);
		check.IsEnabled = false;
		Assert.Throws<ElementNotEnabledException>(toggle.Toggle);
	});

	[Fact]
	public void Radio_Select_checks_it_and_unchecks_its_group() => Run(() =>
	{
		var a = new RadioButton { GroupName = "g" };
		var b = new RadioButton { GroupName = "g" };
		var panel = new StackPanel();
		panel.Children.Add(a);
		panel.Children.Add(b);
		var item = Pattern<ISelectionItemProvider>(b, PatternInterface.SelectionItem);
		Assert.False(item.IsSelected);
		item.Select();
		Assert.True(b.IsChecked);
		Assert.True(item.IsSelected);
		Assert.NotEqual(true, a.IsChecked);
		Assert.Throws<InvalidOperationException>(item.RemoveFromSelection);
	});

	[Fact]
	public void TextBox_SetValue_sets_the_text_and_the_gtk_entry() => Run(() =>
	{
		var box = new TextBox();
		var value = Pattern<IValueProvider>(box, PatternInterface.Value);
		_ = box.Widget;
		value.SetValue("ada");
		Assert.Equal("ada", value.Value);
		Assert.Equal("ada", box.Text);
		Assert.Equal("ada", GtkEntryText(box));
		Assert.False(value.IsReadOnly);
		Assert.Throws<ArgumentNullException>(() => value.SetValue(null!));
	});

	static string GtkEntryText(TextBox box) => ((Gtk.Entry)box.Widget).Text;

	[Fact]
	public void TextBox_SetValue_rejects_disabled_then_read_only_then_null() => Run(() =>
	{
		var box = new TextBox { IsReadOnly = true, IsEnabled = false };
		var value = Pattern<IValueProvider>(box, PatternInterface.Value);
		Assert.True(value.IsReadOnly);
		Assert.Throws<ElementNotEnabledException>(() => value.SetValue(null!));
		box.IsEnabled = true;
		Assert.Throws<ElementNotEnabledException>(() => value.SetValue("x"));
		Assert.Equal("", box.Text);
		box.IsReadOnly = false;
		Assert.Throws<ArgumentNullException>(() => value.SetValue(null!));
	});

	[Fact]
	public void ElementNotEnabledException_is_an_InvalidOperationException() => Run(() =>
	{
		Assert.IsAssignableFrom<InvalidOperationException>(new ElementNotEnabledException());
		Assert.IsAssignableFrom<SystemException>(new ElementNotAvailableException());
	});

	[Fact]
	public void Radio_button_supports_SelectionItem_but_not_Toggle() => Run(() =>
	{
		var peer = UIElementAutomationPeer.CreatePeerForElement(new RadioButton())!;
		Assert.NotNull(peer.GetPattern(PatternInterface.SelectionItem));
		Assert.Null(peer.GetPattern(PatternInterface.Toggle));
		Assert.Null(peer.GetPattern(PatternInterface.Value));
	});

	[Fact]
	public void PasswordBox_sets_but_never_reveals_the_password() => Run(() =>
	{
		var box = new PasswordBox();
		var peer = UIElementAutomationPeer.CreatePeerForElement(box)!;
		var value = (IValueProvider)peer.GetPattern(PatternInterface.Value)!;
		value.SetValue("secret");
		Assert.Equal("secret", box.Password);
		Assert.True(peer.IsPassword());
		Assert.Throws<InvalidOperationException>(() => value.Value);
	});

	[Fact]
	public void Slider_RangeValue_reads_and_sets_the_value() => Run(() =>
	{
		var slider = new Slider { Minimum = 0, Maximum = 10, Value = 2 };
		var range = Pattern<IRangeValueProvider>(slider, PatternInterface.RangeValue);
		Assert.Equal(2, range.Value);
		Assert.Equal(10, range.Maximum);
		range.SetValue(7);
		Assert.Equal(7, slider.Value);
		Assert.Equal(7, ((Gtk.Scale)slider.Widget).Adjustment.Value);
		Assert.Throws<ArgumentOutOfRangeException>(() => range.SetValue(11));
		Assert.Throws<ArgumentOutOfRangeException>(() => range.SetValue(-1));
	});
}
