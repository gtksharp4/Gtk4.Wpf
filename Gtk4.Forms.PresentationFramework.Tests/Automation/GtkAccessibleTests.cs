using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Automation;

/// <summary>The GTK accessible role, name and state of widgets backed by peers, read from real GTK.</summary>
public class GtkAccessibleTests : WpfGtkTestBase
{
	static void AssertRole(UIElement element, Gtk.AccessibleRole role)
	{
		var widget = element.Widget;
		Assert.Equal(role, widget.AccessibleRole);
		Assert.True(GtkA11yProbe.HasRole(widget, role), "GTK reports role " + role);
	}

	[Fact]
	public void Controls_get_the_role_of_their_control_type() => Run(() =>
	{
		AssertRole(new Button(), Gtk.AccessibleRole.Button);
		AssertRole(new CheckBox(), Gtk.AccessibleRole.Checkbox);
		AssertRole(new RadioButton(), Gtk.AccessibleRole.Radio);
		AssertRole(new System.Windows.Controls.Primitives.ToggleButton(), Gtk.AccessibleRole.ToggleButton);
		AssertRole(new TextBox(), Gtk.AccessibleRole.TextBox);
		AssertRole(new PasswordBox(), Gtk.AccessibleRole.TextBox);
		AssertRole(new TextBlock { Text = "t" }, Gtk.AccessibleRole.Label);
		AssertRole(new Label { Content = "l" }, Gtk.AccessibleRole.Label);
		AssertRole(new ComboBox(), Gtk.AccessibleRole.ComboBox);
		AssertRole(new Slider(), Gtk.AccessibleRole.Slider);
		AssertRole(new Window(), Gtk.AccessibleRole.Window);
	});

	[Fact]
	public void ListBox_and_its_items_get_list_roles() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add("a");
		var window = new Window { Content = list };
		_ = list.Widget;
		AssertRole(list, Gtk.AccessibleRole.List);
		AssertRole(list.ContainerAt(0)!, Gtk.AccessibleRole.ListItem);
	});

	sealed class LinkButton : Button
	{
		protected override System.Windows.Automation.Peers.AutomationPeer? OnCreateAutomationPeer() =>
			new LinkPeer(this);
	}

	sealed class LinkPeer : System.Windows.Automation.Peers.ButtonAutomationPeer
	{
		public LinkPeer(LinkButton owner) : base(owner) { }

		protected override System.Windows.Automation.Peers.AutomationControlType GetAutomationControlTypeCore() =>
			System.Windows.Automation.Peers.AutomationControlType.Hyperlink;
	}

	[Fact]
	public void A_custom_peer_control_type_sets_a_non_default_role_before_the_window_is_shown() => Run(() =>
	{
		var link = new LinkButton { Content = "docs" };
		AssertRole(link, Gtk.AccessibleRole.Link);
		var window = new Window { Content = link };
		window.Show();
		GtkPump.Until(() => window.Widget.IsMapped);
		Assert.True(GtkA11yProbe.HasRole(link.Widget, Gtk.AccessibleRole.Link));
	});

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
	public void Name_reaches_the_gtk_label_property() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
		var button = new Button { Content = "Accept" };
		_ = button.Widget;
		Assert.True(GtkA11yProbe.PropertyEquals(button.Widget, Gtk.AccessibleProperty.Label, "Accept"));
		Assert.False(GtkA11yProbe.PropertyEquals(button.Widget, Gtk.AccessibleProperty.Label, "Reject"));
		AutomationProperties.SetName(button, "Confirm");
		Assert.True(GtkA11yProbe.PropertyEquals(button.Widget, Gtk.AccessibleProperty.Label, "Confirm"));
	});

	[Fact]
	public void Name_from_a_labelling_element_is_pushed() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
		var box = new TextBox();
		AutomationProperties.SetLabeledBy(box, new TextBlock { Text = "User" });
		Assert.True(GtkA11yProbe.PropertyEquals(box.Widget, Gtk.AccessibleProperty.Label, "User"));
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
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
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

	[Fact]
	public void Role_set_after_realization_is_skipped_without_error() => Run(() =>
	{
		var window = new Window { Content = new Button() };
		window.Show();
		GtkPump.Until(() => window.Widget.IsMapped);
		var button = (Button)window.Content!;
		// Name changes still reach GTK after realization; the frozen role is not touched.
		AutomationProperties.SetName(button, "After");
		Assert.Equal(Gtk.AccessibleRole.Button, button.Widget.AccessibleRole);
	});

	[Fact]
	public void Shown_window_exposes_the_button_in_the_accessible_tree() => Run(() =>
	{
		var button = new Button { Content = "Tree" };
		var window = new Window { Content = button };
		window.Show();
		var gtkButton = button.Widget;
		GtkPump.Until(() => window.Widget.IsMapped);
		Assert.True(IsAccessibleDescendant(window.Widget, gtkButton));
	});

	static bool IsAccessibleDescendant(Gtk.IAccessible root, Gtk.Widget target)
	{
		for (var child = (root as Gtk.Widget)?.FirstAccessibleChild; child != null;
			child = (child as Gtk.Widget)?.NextAccessibleSibling)
		{
			if (child is Gtk.Widget w && w.Handle == target.Handle) return true;
			if (IsAccessibleDescendant(child, target)) return true;
		}
		return false;
	}
}
