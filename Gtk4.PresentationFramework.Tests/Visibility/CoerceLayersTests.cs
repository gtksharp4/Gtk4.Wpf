using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ElementVisibility;

/// <summary>Coercion is reversible whatever layer the base value comes from (clear, style, trigger).</summary>
public class CoerceLayersTests : VisStateBase
{
	sealed class Src
	{
		public bool On { get; set; } = true;
	}

	static Style Setting(bool value)
	{
		var style = new Style(typeof(VisElement));
		style.Setters.Add(new Setter { Property = UIElement.IsEnabledProperty, Value = value });
		return style;
	}

	[Fact]
	public void ClearValue_under_a_disabled_parent_stays_disabled_then_follows_the_parent()
	{
		var parent = new VisElement();
		var child = new VisElement(parent);
		parent.IsEnabled = false;
		child.IsEnabled = false;
		child.ClearValue(UIElement.IsEnabledProperty);
		Assert.False(child.IsEnabled);
		parent.IsEnabled = true;
		Assert.True(child.IsEnabled);
	}

	[Fact]
	public void ClearValue_on_a_button_whose_command_cannot_execute_stays_disabled()
	{
		var btn = new Button { Command = new SwitchCommand { Can = false } };
		btn.IsEnabled = false;
		btn.ClearValue(UIElement.IsEnabledProperty);
		Assert.False(btn.IsEnabled);
	}

	[Fact]
	public void ReadLocalValue_returns_the_raw_value_under_a_disabled_parent()
	{
		var parent = new VisElement { IsEnabled = false };
		var child = new VisElement(parent);
		child.IsEnabled = true;
		Assert.Equal(true, child.ReadLocalValue(UIElement.IsEnabledProperty));
		Assert.False(child.IsEnabled);
	}

	[Fact]
	public void Style_setter_true_under_a_disabled_parent_recovers_when_the_parent_is_enabled()
	{
		var parent = new VisElement { IsEnabled = false };
		var child = new VisElement(parent);
		child.Style = Setting(true);
		Assert.False(child.IsEnabled);
		parent.IsEnabled = true;
		Assert.True(child.IsEnabled);
	}

	[Fact]
	public void DataTrigger_setter_true_under_a_disabled_parent_recovers_when_the_parent_is_enabled()
	{
		var parent = new VisElement { IsEnabled = false };
		var child = new VisElement(parent) { DataContext = new Src() };
		var style = new Style(typeof(VisElement));
		var trigger = new DataTrigger { Binding = new Binding("On"), Value = true };
		trigger.Setters.Add(new Setter { Property = UIElement.IsEnabledProperty, Value = true });
		style.Triggers.Add(trigger);
		child.Style = style;
		Assert.False(child.IsEnabled);
		parent.IsEnabled = true;
		Assert.True(child.IsEnabled);
	}

	[Fact]
	public void Removing_a_style_false_setter_under_a_disabled_parent_stays_disabled_then_follows_the_parent()
	{
		var parent = new VisElement { IsEnabled = false };
		var child = new VisElement(parent);
		child.Style = Setting(false);
		child.Style = null;
		Assert.False(child.IsEnabled);
		parent.IsEnabled = true;
		Assert.True(child.IsEnabled);
	}

	[Fact]
	public void Style_setter_true_on_a_button_ends_enabled_when_the_command_becomes_executable()
	{
		var cmd = new SwitchCommand { Can = false };
		var btn = new Button { Command = cmd };
		var style = new Style(typeof(Button));
		style.Setters.Add(new Setter { Property = UIElement.IsEnabledProperty, Value = true });
		btn.Style = style;
		Assert.False(btn.IsEnabled);
		cmd.Can = true;
		cmd.Raise();
		Assert.True(btn.IsEnabled);
	}
}
