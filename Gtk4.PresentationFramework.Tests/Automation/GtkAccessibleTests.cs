using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

/// <summary>The GTK accessible role, name and state of widgets backed by peers, read from real GTK.</summary>
public partial class GtkAccessibleTests : WpfGtkTestBase
{
	static void AssertRole(Gtk.Widget widget, Gtk.AccessibleRole role)
	{
		Assert.Equal(role, widget.AccessibleRole);
		Assert.True(GtkA11yProbe.HasRole(widget, role), "GTK reports role " + role);
	}

	// A button whose peer reports any control type: its role is never the GtkButton default for most types.
	sealed class TypedButton : Button
	{
		internal AutomationControlType Type = AutomationControlType.Hyperlink;

		protected override AutomationPeer? OnCreateAutomationPeer() => new TypedPeer(this);
	}

	sealed class TypedPeer : ButtonAutomationPeer
	{
		readonly TypedButton _owner;

		public TypedPeer(TypedButton owner) : base(owner) => _owner = owner;

		protected override AutomationControlType GetAutomationControlTypeCore() => _owner.Type;
	}

	[Theory]
	[InlineData(AutomationControlType.Hyperlink, Gtk.AccessibleRole.Link)]
	[InlineData(AutomationControlType.Image, Gtk.AccessibleRole.Img)]
	[InlineData(AutomationControlType.MenuItem, Gtk.AccessibleRole.MenuItem)]
	[InlineData(AutomationControlType.TabItem, Gtk.AccessibleRole.Tab)]
	[InlineData(AutomationControlType.ProgressBar, Gtk.AccessibleRole.ProgressBar)]
	[InlineData(AutomationControlType.ToolBar, Gtk.AccessibleRole.Toolbar)]
	[InlineData(AutomationControlType.Group, Gtk.AccessibleRole.Group)]
	public void A_peer_control_type_is_pushed_as_the_gtk_role(AutomationControlType type, Gtk.AccessibleRole role) =>
		Run(() => AssertRole(new TypedButton { Type = type }.Widget, role));

	[Fact]
	public void Custom_control_type_keeps_the_widget_role() => Run(() =>
		AssertRole(new TypedButton { Type = AutomationControlType.Custom }.Widget, Gtk.AccessibleRole.Button));

	[Fact]
	public void Label_and_editable_ComboBox_put_the_role_on_the_inner_widget_only() => Run(() =>
	{
		var label = new Label { Content = "l" };
		var wrapper = label.Widget;
		Assert.NotEqual(Gtk.AccessibleRole.Label, wrapper.AccessibleRole);
		AssertRole(label.AccessibleTarget, Gtk.AccessibleRole.Label);

		var combo = new ComboBox { IsEditable = true };
		var host = combo.Widget;
		Assert.NotEqual(Gtk.AccessibleRole.ComboBox, host.AccessibleRole);
		AssertRole(combo.AccessibleTarget, Gtk.AccessibleRole.ComboBox);
		Assert.NotSame(host, combo.AccessibleTarget);
	});

	[Fact]
	public void Multi_line_TextBox_pushes_to_the_text_view_not_the_scrolled_window() => Run(() =>
	{
		var box = new TextBox { AcceptsReturn = true };
		var host = box.Widget;
		Assert.NotSame(host, box.FocusWidget);
		Assert.True(GtkA11yProbe.HasProperty(box.FocusWidget, Gtk.AccessibleProperty.MultiLine));
		Assert.False(GtkA11yProbe.HasProperty(host, Gtk.AccessibleProperty.MultiLine));
	});

	[Fact]
	public void A_peer_role_is_set_before_the_window_is_shown() => Run(() =>
	{
		var link = new TypedButton { Content = "docs" };
		AssertRole(link.Widget, Gtk.AccessibleRole.Link);
		var window = new Window { Content = link };
		window.Show();
		GtkPump.Until(() => window.Widget.IsMapped);
		Assert.True(GtkA11yProbe.HasRole(link.Widget, Gtk.AccessibleRole.Link));
	});

	[Fact]
	public void A_role_change_after_realization_is_ignored_and_other_pushes_still_work() => Run(() =>
	{
		var button = new TypedButton { Content = "x" };
		var window = new Window { Content = button };
		window.Show();
		Assert.True(GtkPump.Until(() => window.Widget.IsMapped));
		button.Type = AutomationControlType.Image;
		System.Windows.Automation.AutomationProperties.SetName(button, "After");
		Assert.Equal(Gtk.AccessibleRole.Link, button.Widget.AccessibleRole);
		Assert.True(GtkA11yProbe.HasProperty(button.Widget, Gtk.AccessibleProperty.Label));
	});

	[Fact]
	public void ListBox_and_its_items_get_list_roles() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add("a");
		var window = new Window { Content = list };
		_ = list.Widget;
		AssertRole(list.Widget, Gtk.AccessibleRole.List);
		AssertRole(list.ContainerAt(0)!.Widget, Gtk.AccessibleRole.ListItem);
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
