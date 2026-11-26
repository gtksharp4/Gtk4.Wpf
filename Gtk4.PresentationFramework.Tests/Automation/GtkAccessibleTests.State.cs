using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

public partial class GtkAccessibleTests
{
	[Fact]
	public void Toggle_state_change_raises_a_property_changed_event() => Run(() =>
	{
		var check = new CheckBox();
		var peer = System.Windows.Automation.Peers.UIElementAutomationPeer.CreatePeerForElement(check)!;
		var seen = new System.Collections.Generic.List<string>();
		void OnProp(System.Windows.Automation.Peers.AutomationPeer p, AutomationProperty prop, object? o, object? n) =>
			seen.Add($"{prop.Id}:{o}->{n}");
		System.Windows.Automation.Peers.AutomationPeer.AddListener(
			System.Windows.Automation.Peers.AutomationEvents.PropertyChanged);
		System.Windows.Automation.Peers.AutomationPeer.PropertyChangedRaised += OnProp;
		try { check.IsChecked = true; }
		finally
		{
			System.Windows.Automation.Peers.AutomationPeer.PropertyChangedRaised -= OnProp;
			System.Windows.Automation.Peers.AutomationPeer.RemoveListener(
				System.Windows.Automation.Peers.AutomationEvents.PropertyChanged);
		}
		Assert.Equal(new[] { "30086:Off->On" }, seen);
		Assert.NotNull(peer);
	});
	[Fact]
	public void Only_an_explicit_name_reaches_the_gtk_label_property() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
		var button = new Button { Content = "Accept" };
		var widget = button.Widget;
		Assert.False(GtkA11yProbe.HasProperty(widget, Gtk.AccessibleProperty.Label), "content names are GTK's");
		AutomationProperties.SetName(button, "Confirm");
		Assert.True(GtkA11yProbe.PropertyEquals(widget, Gtk.AccessibleProperty.Label, "Confirm"));
		AutomationProperties.SetName(button, "Again");
		Assert.True(GtkA11yProbe.PropertyEquals(widget, Gtk.AccessibleProperty.Label, "Again"));
		AutomationProperties.SetName(button, null);
		Assert.False(GtkA11yProbe.HasProperty(widget, Gtk.AccessibleProperty.Label), "cleared name is reset");
	});

	[Fact]
	public void Gtk_follows_Button_content_and_TextBlock_text_changes() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
		var button = new Button { Content = "Accept" };
		var block = new TextBlock { Text = "Ready" };
		var buttonWidget = (Gtk.Button)button.Widget;
		var blockWidget = (Gtk.Label)block.Widget;
		button.Content = "Reject";
		block.Text = "Done";
		Assert.Equal("Reject", buttonWidget.Label);
		Assert.Equal("Done", blockWidget.Text);
		Assert.False(GtkA11yProbe.HasProperty(buttonWidget, Gtk.AccessibleProperty.Label));
		// GtkLabel sets its own label property and keeps it current; the stale "Ready" must not survive.
		Assert.False(GtkA11yProbe.PropertyEquals(blockWidget, Gtk.AccessibleProperty.Label, "Ready"));
		Assert.Equal("Reject", System.Windows.Automation.Peers.UIElementAutomationPeer
			.CreatePeerForElement(button)!.GetName());
	});

	[Fact]
	public void Cleared_labelling_element_resets_the_label() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
		var box = new TextBox();
		AutomationProperties.SetLabeledBy(box, new TextBlock { Text = "User" });
		Assert.True(GtkA11yProbe.PropertyEquals(box.FocusWidget, Gtk.AccessibleProperty.Label, "User"));
		AutomationProperties.SetLabeledBy(box, null);
		Assert.False(GtkA11yProbe.HasProperty(box.FocusWidget, Gtk.AccessibleProperty.Label));
	});

	[Fact]
	public void Checked_state_follows_IsChecked() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
		var check = new CheckBox { IsThreeState = true, IsChecked = true };
		var widget = check.Widget;
		Assert.True(GtkA11yProbe.StateEquals(widget, Gtk.AccessibleState.Checked, (int)Gtk.AccessibleTristate.True));
		check.IsChecked = false;
		Assert.True(GtkA11yProbe.StateEquals(widget, Gtk.AccessibleState.Checked, (int)Gtk.AccessibleTristate.False));
		check.IsChecked = null;
		Assert.True(GtkA11yProbe.StateEquals(widget, Gtk.AccessibleState.Checked, (int)Gtk.AccessibleTristate.Mixed));
	});

	[Fact]
	public void Toggle_button_pressed_state_follows_IsChecked() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
		var toggle = new System.Windows.Controls.Primitives.ToggleButton { IsChecked = true };
		Assert.True(GtkA11yProbe.StateEquals(toggle.Widget, Gtk.AccessibleState.Pressed,
			(int)Gtk.AccessibleTristate.True));
	});

	[Fact]
	public void Disabled_state_follows_IsEnabled() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
		var button = new Button();
		var widget = button.Widget;
		Assert.True(GtkA11yProbe.StateEquals(widget, Gtk.AccessibleState.Disabled, 0));
		button.IsEnabled = false;
		Assert.True(GtkA11yProbe.StateEquals(widget, Gtk.AccessibleState.Disabled, 1));
	});

	[Fact]
	public void Slider_value_range_is_pushed_and_tracks_changes() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckDoubles, "variadic doubles need the Win64 ABI");
		var slider = new Slider { Minimum = 0, Maximum = 10, Value = 3 };
		var widget = slider.Widget;
		Assert.True(GtkA11yProbe.PropertyEquals(widget, Gtk.AccessibleProperty.ValueMax, 10.0));
		Assert.True(GtkA11yProbe.PropertyEquals(widget, Gtk.AccessibleProperty.ValueNow, 3.0));
		slider.Value = 8;
		Assert.True(GtkA11yProbe.PropertyEquals(widget, Gtk.AccessibleProperty.ValueNow, 8.0));
		Assert.False(GtkA11yProbe.PropertyEquals(widget, Gtk.AccessibleProperty.ValueNow, 3.0));
	});

	[Fact]
	public void TextBox_read_only_and_required_are_pushed() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
		var box = new TextBox { IsReadOnly = true };
		AutomationProperties.SetIsRequiredForForm(box, true);
		var widget = box.Widget;
		Assert.True(GtkA11yProbe.PropertyEquals(widget, Gtk.AccessibleProperty.ReadOnly, true));
		Assert.True(GtkA11yProbe.PropertyEquals(widget, Gtk.AccessibleProperty.Required, true));
		box.IsReadOnly = false;
		Assert.True(GtkA11yProbe.PropertyEquals(widget, Gtk.AccessibleProperty.ReadOnly, false));
	});

	[Fact]
	public void ComboBox_expanded_state_follows_the_drop_down() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
		var combo = new ComboBox();
		combo.Items.Add("x");
		var widget = combo.Widget;
		Assert.True(GtkA11yProbe.StateEquals(widget, Gtk.AccessibleState.Expanded, 0));
		combo.IsDropDownOpen = true;
		Assert.True(GtkA11yProbe.StateEquals(widget, Gtk.AccessibleState.Expanded, 1));
	});

	[Fact]
	public void Selected_state_follows_the_selection() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
		var list = new ListBox();
		list.Items.Add("a");
		list.Items.Add("b");
		var window = new Window { Content = list };
		_ = list.Widget;
		list.SelectedItem = "b";
		Assert.True(GtkA11yProbe.StateEquals(list.ContainerAt(1)!.Widget, Gtk.AccessibleState.Selected, 1));
		Assert.True(GtkA11yProbe.StateEquals(list.ContainerAt(0)!.Widget, Gtk.AccessibleState.Selected, 0));
	});

}
