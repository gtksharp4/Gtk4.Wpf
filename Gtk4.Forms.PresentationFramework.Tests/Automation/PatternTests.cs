using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Automation;

/// <summary>Patterns called directly on the providers, checked against the real GTK widgets.</summary>
public class PatternTests : WpfGtkTestBase
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
		Assert.Throws<ElementNotEnabledException>(() => Pattern<IInvokeProvider>(button, PatternInterface.Invoke).Invoke());
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
	public void TextBox_SetValue_rejects_read_only_and_disabled() => Run(() =>
	{
		var box = new TextBox { IsReadOnly = true };
		var value = Pattern<IValueProvider>(box, PatternInterface.Value);
		Assert.True(value.IsReadOnly);
		Assert.Throws<InvalidOperationException>(() => value.SetValue("x"));
		box.IsReadOnly = false;
		box.IsEnabled = false;
		Assert.Throws<ElementNotEnabledException>(() => value.SetValue("x"));
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

	[Fact]
	public void ComboBox_selection_value_and_expand_collapse() => Run(() =>
	{
		var combo = new ComboBox();
		combo.Items.Add("one");
		combo.Items.Add("two");
		var peer = UIElementAutomationPeer.CreatePeerForElement(combo)!;
		var items = peer.GetChildren()!;
		Assert.Equal(new[] { "one", "two" }, new[] { items[0].GetName(), items[1].GetName() });

		var item = (ISelectionItemProvider)items[1].GetPattern(PatternInterface.SelectionItem)!;
		item.Select();
		Assert.Equal("two", combo.SelectedItem);
		Assert.Equal(1, combo.SelectedIndex);
		Assert.True(item.IsSelected);
		var selection = (ISelectionProvider)peer.GetPattern(PatternInterface.Selection)!;
		Assert.Single(selection.GetSelection());
		Assert.False(selection.CanSelectMultiple);

		var expand = (IExpandCollapseProvider)peer.GetPattern(PatternInterface.ExpandCollapse)!;
		Assert.Equal(ExpandCollapseState.Collapsed, expand.ExpandCollapseState);
		expand.Expand();
		Assert.True(combo.IsDropDownOpen);
		Assert.Equal(ExpandCollapseState.Expanded, expand.ExpandCollapseState);
		expand.Collapse();
		Assert.False(combo.IsDropDownOpen);

		var value = (IValueProvider)peer.GetPattern(PatternInterface.Value)!;
		Assert.True(value.IsReadOnly);
		Assert.Throws<InvalidOperationException>(() => value.SetValue("x"));
	});

	[Fact]
	public void ListBox_items_select_through_the_provider_and_change_the_widget() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add("a");
		list.Items.Add("b");
		list.Items.Add("c");
		var window = new Window { Content = list };
		_ = list.Widget;
		var peer = UIElementAutomationPeer.CreatePeerForElement(list)!;
		var items = peer.GetChildren()!;
		Assert.Equal(3, items.Count);
		Assert.Same(items[0], peer.GetChildren()![0]);
		Assert.Same(peer, items[0].GetParent());

		var b = (ISelectionItemProvider)items[1].GetPattern(PatternInterface.SelectionItem)!;
		b.Select();
		Assert.Equal("b", list.SelectedItem);
		Assert.True(b.IsSelected);
		Assert.False(((ISelectionItemProvider)items[0].GetPattern(PatternInterface.SelectionItem)!).IsSelected);
		var selected = ((ISelectionProvider)peer.GetPattern(PatternInterface.Selection)!).GetSelection();
		Assert.Single(selected);
		Assert.Same(items[1], ((PeerProvider)selected[0]).Peer);
	});

	[Fact]
	public void ListBox_multiple_selection_adds_and_removes() => Run(() =>
	{
		var list = new ListBox { SelectionMode = SelectionMode.Multiple };
		list.Items.Add("a");
		list.Items.Add("b");
		var window = new Window { Content = list };
		_ = list.Widget;
		var peer = UIElementAutomationPeer.CreatePeerForElement(list)!;
		var items = peer.GetChildren()!;
		var a = (ISelectionItemProvider)items[0].GetPattern(PatternInterface.SelectionItem)!;
		var b = (ISelectionItemProvider)items[1].GetPattern(PatternInterface.SelectionItem)!;
		a.AddToSelection();
		b.AddToSelection();
		Assert.True(a.IsSelected && b.IsSelected);
		Assert.Equal(2, list.SelectedItems.Count);
		a.RemoveFromSelection();
		Assert.False(a.IsSelected);
		Assert.Single(list.SelectedItems);
		Assert.True(((ISelectionProvider)peer.GetPattern(PatternInterface.Selection)!).CanSelectMultiple);
	});

	[Fact]
	public void Window_pattern_reports_and_changes_chrome_state() => Run(() =>
	{
		var window = new Window { Title = "Main", Topmost = true };
		var peer = UIElementAutomationPeer.CreatePeerForElement(window)!;
		Assert.Equal("Main", peer.GetName());
		Assert.Equal(AutomationControlType.Window, peer.GetAutomationControlType());
		var provider = (IWindowProvider)peer.GetPattern(PatternInterface.Window)!;
		Assert.True(provider.IsTopmost);
		Assert.False(provider.IsModal);
		Assert.True(provider.Maximizable);
		Assert.Equal(WindowVisualState.Normal, provider.VisualState);
		provider.SetVisualState(WindowVisualState.Maximized);
		Assert.Equal(WindowState.Maximized, window.WindowState);
		Assert.Equal(WindowVisualState.Maximized, provider.VisualState);
		window.ResizeMode = ResizeMode.NoResize;
		Assert.Throws<InvalidOperationException>(() => provider.SetVisualState(WindowVisualState.Minimized));
	});

	[Fact]
	public void Window_Close_closes_the_window() => Run(() =>
	{
		var window = new Window();
		bool closed = false;
		window.Closed += (s, e) => closed = true;
		window.Show();
		Pattern<IWindowProvider>(window, PatternInterface.Window).Close();
		Assert.True(GtkPump.Until(() => closed));
	});

	[Fact]
	public void Slider_orientation_and_text_peers_report_their_types() => Run(() =>
	{
		var slider = UIElementAutomationPeer.CreatePeerForElement(new Slider { Orientation = Orientation.Vertical })!;
		Assert.Equal(AutomationOrientation.Vertical, slider.GetOrientation());
		var text = UIElementAutomationPeer.CreatePeerForElement(new TextBlock { Text = "Hi" })!;
		Assert.Equal(AutomationControlType.Text, text.GetAutomationControlType());
		Assert.Equal("Hi", text.GetName());
		var label = UIElementAutomationPeer.CreatePeerForElement(new Label { Content = "Name" })!;
		Assert.Equal("Name", label.GetName());
		Assert.Equal("Label", label.GetClassName());
	});
}
