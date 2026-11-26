using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>A panel whose visual children are not reported as logical children (as in generated trees).</summary>
public class VisualOnlyPanel : StackPanel
{
	protected internal override IEnumerator LogicalChildren => new object[0].GetEnumerator();
}

/// <summary>RadioButton grouping, Checked/Unchecked and Click through the real GTK signal path.</summary>
public class RadioButtonEventTests : WpfGtkTestBase
{
	[Fact]
	public void RadioButton_group_is_exclusive() => Run(() =>
	{
		var a = new RadioButton { GroupName = "g44" };
		var b = new RadioButton { GroupName = "g44" };
		var other = new RadioButton { GroupName = "g44other" };
		other.IsChecked = true;
		a.IsChecked = true;
		b.IsChecked = true;
		Assert.False(a.IsChecked);
		Assert.True(b.IsChecked);
		Assert.True(other.IsChecked);
	});

	[Fact]
	public void RadioButton_ungrouped_siblings_are_exclusive() => Run(() =>
	{
		var panel = new StackPanel();
		var a = new RadioButton();
		var b = new RadioButton();
		panel.Children.Add(a);
		panel.Children.Add(b);
		a.IsChecked = true;
		b.IsChecked = true;
		Assert.False(a.IsChecked);
		Assert.True(b.IsChecked);
	});

	[Fact]
	public void RadioButton_ungrouped_peers_are_the_visual_siblings_only() => Run(() =>
	{
		var outer = new StackPanel();
		var inner = new StackPanel();
		var a = new RadioButton();
		var b = new RadioButton();
		var far = new RadioButton();
		inner.Children.Add(a);
		inner.Children.Add(b);
		outer.Children.Add(inner);
		outer.Children.Add(far);
		Assert.Same(inner, System.Windows.Media.VisualTreeHelper.GetParent(a));
		far.IsChecked = true;
		a.IsChecked = true;
		b.IsChecked = true;
		Assert.False(a.IsChecked);
		Assert.True(b.IsChecked);
		Assert.True(far.IsChecked);
	});

	[Fact]
	public void RadioButton_ungrouped_peers_come_from_the_visual_tree_when_logical_children_omit_them() => Run(() =>
	{
		var panel = new VisualOnlyPanel();
		var a = new RadioButton();
		var b = new RadioButton();
		panel.Children.Add(a);
		panel.Children.Add(b);
		Assert.Empty(System.Windows.LogicalTreeHelper.GetChildren(panel).Cast<object>());
		a.IsChecked = true;
		b.IsChecked = true;
		Assert.False(a.IsChecked);
		Assert.True(b.IsChecked);
	});

	[Fact]
	public void RadioButton_raises_Checked_and_Unchecked_on_switch() => Run(() =>
	{
		var a = new RadioButton { GroupName = "g44ev" };
		var b = new RadioButton { GroupName = "g44ev" };
		int ac = 0, au = 0;
		a.Checked += (s, e) => ac++;
		a.Unchecked += (s, e) => au++;
		a.IsChecked = true;
		b.IsChecked = true;
		Assert.Equal(1, ac);
		Assert.Equal(1, au);
	});

	[Fact]
	public void RadioButton_click_checks_it_and_unchecks_peer() => Run(() =>
	{
		var a = new RadioButton { GroupName = "g44click" };
		var b = new RadioButton { GroupName = "g44click" };
		var ga = (Gtk.CheckButton)a.Widget;
		var gb = (Gtk.CheckButton)b.Widget;
		ga.Active = true;
		gb.Active = true;
		Assert.False(a.IsChecked);
		Assert.True(b.IsChecked);
	});

	[Fact]
	public void RadioButton_user_click_raises_Click_once_on_that_button_only() => Run(() =>
	{
		var a = new RadioButton { GroupName = "g44clk2" };
		var b = new RadioButton { GroupName = "g44clk2" };
		var gb = (Gtk.CheckButton)b.Widget;
		var ga = (Gtk.CheckButton)a.Widget;
		gb.Active = true;
		var log = new List<string>();
		a.Checked += (s, e) => log.Add("a.checked");
		a.Click += (s, e) => log.Add("a.click");
		b.Click += (s, e) => log.Add("b.click");

		ga.Active = true;

		Assert.Equal(new[] { "a.checked", "a.click" }, log);
	});

	[Fact]
	public void RadioButton_clicked_while_checked_and_alone_raises_Click_and_stays_checked() => Run(() =>
	{
		var a = new RadioButton();
		var ga = (Gtk.CheckButton)a.Widget;
		a.IsChecked = true;
		int clicks = 0;
		int unchecks = 0;
		a.Click += (s, e) => clicks++;
		a.Unchecked += (s, e) => unchecks++;

		ga.Active = false;

		Assert.Equal(1, clicks);
		Assert.Equal(0, unchecks);
		Assert.True(a.IsChecked);
		Assert.True(ga.Active);
	});

	[Fact]
	public void RadioButton_programmatic_check_raises_no_Click() => Run(() =>
	{
		var a = new RadioButton { GroupName = "g44prog" };
		var b = new RadioButton { GroupName = "g44prog" };
		_ = a.Widget;
		_ = b.Widget;
		int clicks = 0;
		a.Click += (s, e) => clicks++;
		b.Click += (s, e) => clicks++;
		a.IsChecked = true;
		b.IsChecked = true;
		Assert.Equal(0, clicks);
	});
}
