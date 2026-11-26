using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>IsMouseOver / IsMouseDirectlyOver read-only DPs and the live hover trigger.</summary>
public class MouseOverTests : MouseStateTestBase
{
	[Fact]
	public void DPs_are_read_only_bool_default_false_owned_by_UIElement()
	{
		foreach (var dp in new[] { UIElement.IsMouseOverProperty, UIElement.IsMouseDirectlyOverProperty })
		{
			Assert.True(dp.IsReadOnly);
			Assert.Equal(typeof(bool), dp.PropertyType);
			Assert.Equal(typeof(UIElement), dp.OwnerType);
			Assert.Equal(false, dp.DefaultMetadata.DefaultValue);
		}
		var el = new MouseTestElement();
		Assert.False(el.IsMouseOver);
		Assert.False(el.IsMouseDirectlyOver);
	}

	[Fact]
	public void SetValue_on_read_only_DP_throws()
	{
		var el = new MouseTestElement();
		Assert.ThrowsAny<InvalidOperationException>(() => el.SetValue(UIElement.IsMouseOverProperty, true));
	}

	[Fact]
	public void Enter_sets_IsMouseOver_on_element_and_ancestors()
	{
		var root = new MouseTestElement("root");
		var child = new MouseTestElement("child", root);
		child.SimulateMouseEnterForTest();
		Assert.True(child.IsMouseOver);
		Assert.True(root.IsMouseOver);
	}

	[Fact]
	public void IsMouseDirectlyOver_only_on_innermost_and_Mouse_DirectlyOver_tracks_it()
	{
		var root = new MouseTestElement("root");
		var child = new MouseTestElement("child", root);
		child.SimulateMouseEnterForTest();
		Assert.True(child.IsMouseDirectlyOver);
		Assert.False(root.IsMouseDirectlyOver);
		Assert.Same(child, Mouse.DirectlyOver);
	}

	[Fact]
	public void IsMouseDirectlyOverChanged_fires_with_old_and_new_values()
	{
		var el = new MouseTestElement();
		var changes = new List<(object? oldV, object? newV)>();
		el.IsMouseDirectlyOverChanged += (s, e) => changes.Add((e.OldValue, e.NewValue));
		el.SimulateMouseEnterForTest();
		el.SimulateMouseLeaveForTest();
		Assert.Equal(new List<(object?, object?)> { (false, true), (true, false) }, changes);
	}

	[Fact]
	public void Leave_clears_both_flags_up_the_chain()
	{
		var root = new MouseTestElement("root");
		var child = new MouseTestElement("child", root);
		child.SimulateMouseEnterForTest();
		child.SimulateMouseLeaveForTest();
		Assert.False(child.IsMouseOver);
		Assert.False(child.IsMouseDirectlyOver);
		Assert.False(root.IsMouseOver);
		Assert.Null(Mouse.DirectlyOver);
	}

	[Fact]
	public void UpdateMouseOver_maps_contains_pointer_and_is_pointer_independently()
	{
		var el = new MouseTestElement();
		el.UpdateMouseOver(containsPointer: true, isPointer: false);
		Assert.True(el.IsMouseOver);
		Assert.False(el.IsMouseDirectlyOver);
	}

	[Fact]
	public void Style_IsMouseOver_trigger_applies_when_hovered()
	{
		var el = new MouseTestElement();
		var style = new Style(typeof(MouseTestElement));
		var trigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = UIElement.OpacityProperty, Value = 0.5 });
		style.Triggers.Add(trigger);
		el.Style = style;
		Assert.Equal(1.0, el.Opacity);

		el.SimulateMouseEnterForTest();
		Assert.Equal(0.5, el.Opacity);

		el.SimulateMouseLeaveForTest();
		Assert.Equal(1.0, el.Opacity);
	}
}
