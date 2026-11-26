using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.FlowDirectionTests;

/// <summary>The widgets a control owns inside its backing widget follow its FlowDirection too.</summary>
public class FlowDirectionPartsTests : WpfGtkTestBase
{
	static List<Gtk.Widget> Descendants(Gtk.Widget root)
	{
		var all = new List<Gtk.Widget> { root };
		for (var child = root.FirstChild; child != null; child = child.NextSibling)
			all.AddRange(Descendants(child));
		return all;
	}

	static void AssertAllRtl(Gtk.Widget root)
	{
		foreach (var w in Descendants(root))
			Assert.True(w.Direction == Gtk.TextDirection.Rtl, w.GetType().Name + " is " + w.Direction);
	}

	[Fact]
	public void A_single_line_TextBox_and_its_inner_text_widget_are_Rtl() => Run(() =>
	{
		var box = new TextBox { Text = "a", FlowDirection = FlowDirection.RightToLeft };

		Assert.True(Descendants(box.Widget).Count > 1);
		AssertAllRtl(box.Widget);
	});

	[Fact]
	public void A_multiline_TextBox_has_an_Rtl_TextView() => Run(() =>
	{
		var box = new TextBox { AcceptsReturn = true, FlowDirection = FlowDirection.RightToLeft };

		Assert.Contains(Descendants(box.Widget), w => w is Gtk.TextView);
		AssertAllRtl(box.Widget);
	});

	[Fact]
	public void A_ComboBox_and_its_DropDown_parts_are_Rtl() => Run(() =>
	{
		var combo = new ComboBox { FlowDirection = FlowDirection.RightToLeft };
		combo.Items.Add("a");

		Assert.Contains(Descendants(combo.Widget), w => w is Gtk.DropDown);
		AssertAllRtl(combo.Widget);
	});

	[Fact]
	public void The_value_applies_to_the_parts_when_set_after_creation_and_switches_back() => Run(() =>
	{
		var box = new TextBox { Text = "a" };
		_ = box.Widget;
		box.FlowDirection = FlowDirection.RightToLeft;
		AssertAllRtl(box.Widget);
		box.FlowDirection = FlowDirection.LeftToRight;

		Assert.All(Descendants(box.Widget), w => Assert.Equal(Gtk.TextDirection.Ltr, w.Direction));
	});

	[Fact]
	public void A_nested_element_keeps_its_own_direction_when_the_parent_changes() => Run(() =>
	{
		var panel = new StackPanel();
		var ltr = new TextBox { FlowDirection = FlowDirection.LeftToRight };
		panel.Children.Add(ltr);
		_ = panel.Widget;
		_ = ltr.Widget;

		panel.FlowDirection = FlowDirection.RightToLeft;

		Assert.All(Descendants(ltr.Widget), w => Assert.Equal(Gtk.TextDirection.Ltr, w.Direction));
	});

	[Fact]
	public void A_DockPanel_in_an_RTL_window_docks_Left_on_the_right() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var left = new Button { Content = "left", Width = 60 };
		DockPanel.SetDock(left, Dock.Left);
		var dock = new DockPanel();
		dock.Children.Add(left);
		dock.Children.Add(new Button { Content = "fill" });
		var win = new Window { Width = 400, Height = 200, FlowDirection = FlowDirection.RightToLeft, Content = dock };
		try
		{
			win.Show();
			GtkState.Require(() => dock.ActualWidth > 0, "dock layout", win, dock);
			GtkPump.For(100);

			Assert.True(left.Widget.ComputeBounds(dock.Widget, out var bounds));
			Assert.True(bounds.X > dock.ActualWidth / 2, $"left child X was {bounds.X} of {dock.ActualWidth}");
		}
		finally { win.Close(); }
	});
}
