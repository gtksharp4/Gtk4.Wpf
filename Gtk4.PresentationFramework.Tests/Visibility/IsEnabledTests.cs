using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ElementVisibility;

/// <summary>IsEnabled coercion through the logical tree, IsEnabledCore and command sources.</summary>
public class IsEnabledTests : VisStateBase
{
	[Fact]
	public void Disabled_parent_forces_a_child_with_local_true_to_false()
	{
		var parent = new VisElement();
		var child = new VisElement(parent) { IsEnabled = true };
		parent.IsEnabled = false;
		Assert.False(child.IsEnabled);
	}

	[Fact]
	public void Re_enabling_the_parent_restores_the_child()
	{
		var parent = new VisElement();
		var child = new VisElement(parent) { IsEnabled = true };
		parent.IsEnabled = false;
		parent.IsEnabled = true;
		Assert.True(child.IsEnabled);
	}

	[Fact]
	public void A_child_local_false_stays_false_when_the_parent_is_enabled()
	{
		var parent = new VisElement();
		var child = new VisElement(parent) { IsEnabled = false };
		parent.IsEnabled = false;
		parent.IsEnabled = true;
		Assert.False(child.IsEnabled);
	}

	[Fact]
	public void Grandchildren_follow_the_chain()
	{
		var a = new VisElement();
		var b = new VisElement(a);
		var c = new VisElement(b);
		a.IsEnabled = false;
		Assert.False(c.IsEnabled);
		a.IsEnabled = true;
		Assert.True(c.IsEnabled);
	}

	[Fact]
	public void IsEnabledCore_override_is_honoured_after_recoerce()
	{
		var el = new CoreElement();
		Assert.True(el.IsEnabled);
		el.CoreEnabled = false;
		el.Recoerce();
		Assert.False(el.IsEnabled);
		el.CoreEnabled = true;
		el.Recoerce();
		Assert.True(el.IsEnabled);
	}

	[Fact]
	public void IsEnabledChanged_fires_parent_before_child_and_only_on_effective_change()
	{
		var parent = new VisElement();
		var child = new VisElement(parent);
		var log = new List<string>();
		parent.IsEnabledChanged += (s, e) => log.Add("parent:" + e.NewValue);
		child.IsEnabledChanged += (s, e) => log.Add("child:" + e.NewValue);

		parent.IsEnabled = false;
		parent.IsEnabled = false;

		Assert.Equal(new[] { "parent:False", "child:False" }, log);
	}

	[Fact]
	public void Child_local_false_does_not_raise_when_parent_disables()
	{
		var parent = new VisElement();
		var child = new VisElement(parent) { IsEnabled = false };
		int raised = 0;
		child.IsEnabledChanged += (s, e) => raised++;
		parent.IsEnabled = false;
		Assert.Equal(0, raised);
	}

	[Fact]
	public void Reparenting_under_a_disabled_parent_disables_the_element()
	{
		var disabled = new VisElement { IsEnabled = false };
		var el = new VisElement();
		el.Attach(disabled);
		Assert.False(el.IsEnabled);
		el.Attach(null);
		Assert.True(el.IsEnabled);
	}

	[Fact]
	public void Button_with_CanExecute_false_is_disabled_and_toggling_CanExecuteChanged_flips_it()
	{
		var cmd = new SwitchCommand { Can = false };
		var btn = new Button { Command = cmd };
		Assert.False(btn.IsEnabled);

		cmd.Can = true;
		cmd.Raise();
		Assert.True(btn.IsEnabled);

		cmd.Can = false;
		cmd.Raise();
		Assert.False(btn.IsEnabled);
	}

	[Fact]
	public void Button_command_enabled_state_respects_a_disabled_parent()
	{
		var panel = new StackPanel { IsEnabled = false };
		var btn = new Button { Command = new SwitchCommand() };
		panel.Children.Add(btn);
		Assert.False(btn.IsEnabled);
	}

	[Fact]
	public void Clearing_the_Button_command_re_enables_it()
	{
		var btn = new Button { Command = new SwitchCommand { Can = false } };
		btn.Command = null;
		Assert.True(btn.IsEnabled);
	}

	[Fact]
	public void MenuItem_command_coerces_IsEnabled_the_same_way()
	{
		var cmd = new SwitchCommand { Can = false };
		var item = new MenuItem { Command = cmd };
		Assert.False(item.IsEnabled);
		cmd.Can = true;
		cmd.Raise();
		Assert.True(item.IsEnabled);
	}
}
