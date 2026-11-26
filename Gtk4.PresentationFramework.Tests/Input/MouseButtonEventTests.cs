using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>
/// Button-specific events cracked from (Preview)MouseDown/Up by UIElement class handlers,
/// driven through the same managed raise path the GTK GestureClick handlers use.
/// </summary>
public class MouseButtonEventTests : MouseStateTestBase
{
	static readonly Point P = new Point(3, 4);

	[Fact]
	public void Button_events_are_Direct_MouseButtonEventHandler_owned_by_UIElement()
	{
		foreach (var re in new[]
		{
			UIElement.PreviewMouseLeftButtonDownEvent, UIElement.MouseLeftButtonDownEvent,
			UIElement.PreviewMouseLeftButtonUpEvent, UIElement.MouseLeftButtonUpEvent,
			UIElement.PreviewMouseRightButtonDownEvent, UIElement.MouseRightButtonDownEvent,
			UIElement.PreviewMouseRightButtonUpEvent, UIElement.MouseRightButtonUpEvent,
		})
		{
			Assert.Equal(RoutingStrategy.Direct, re.RoutingStrategy);
			Assert.Equal(typeof(MouseButtonEventHandler), re.HandlerType);
			Assert.Equal(typeof(UIElement), re.OwnerType);
		}
	}

	[Fact]
	public void Left_down_raises_specific_before_generic_on_each_element_bubbling()
	{
		var parent = new MouseTestElement("parent");
		var child = new MouseTestElement("child", parent);
		var log = new List<string>();
		foreach (var el in new[] { parent, child })
		{
			el.MouseDown += (s, e) => log.Add(Tag(s) + ":MouseDown");
			el.MouseLeftButtonDown += (s, e) => log.Add(Tag(s) + ":Left");
		}

		child.RaiseMouseButton(MouseButton.Left, P, 1, pressed: true);

		Assert.Equal(new[] { "child:Left", "child:MouseDown", "parent:Left", "parent:MouseDown" }, log);
	}

	[Fact]
	public void Handling_MouseLeftButtonDown_marks_MouseDown_handled()
	{
		var parent = new MouseTestElement("parent");
		var child = new MouseTestElement("child", parent);
		child.MouseLeftButtonDown += (s, e) => e.Handled = true;
		bool childGeneric = false, parentGeneric = false, parentTooHandled = false;
		child.MouseDown += (s, e) => childGeneric = true;
		parent.MouseDown += (s, e) => parentGeneric = true;
		parent.AddHandler(UIElement.MouseDownEvent,
			new MouseButtonEventHandler((s, e) => parentTooHandled = e.Handled), true);

		child.RaiseMouseButton(MouseButton.Left, P, 1, pressed: true);

		Assert.False(childGeneric);
		Assert.False(parentGeneric);
		Assert.True(parentTooHandled);
	}

	[Fact]
	public void Right_raises_Right_events_and_not_Left()
	{
		var el = new MouseTestElement("el");
		var log = new List<string>();
		el.MouseLeftButtonDown += (s, e) => log.Add("LeftDown");
		el.MouseRightButtonDown += (s, e) => log.Add("RightDown");
		el.PreviewMouseRightButtonUp += (s, e) => log.Add("PreviewRightUp");
		el.MouseRightButtonUp += (s, e) => log.Add("RightUp");

		el.RaiseMouseButton(MouseButton.Right, P, 1, pressed: true);
		el.RaiseMouseButton(MouseButton.Right, P, 1, pressed: false);

		Assert.Equal(new[] { "RightDown", "PreviewRightUp", "RightUp" }, log);
	}

	[Fact]
	public void Middle_raises_no_specific_event_but_generic_MouseDown()
	{
		var el = new MouseTestElement("el");
		var log = new List<string>();
		el.MouseLeftButtonDown += (s, e) => log.Add("Left");
		el.MouseRightButtonDown += (s, e) => log.Add("Right");
		el.MouseDown += (s, e) => log.Add("MouseDown:" + e.ChangedButton);

		el.RaiseMouseButton(MouseButton.Middle, P, 1, pressed: true);

		Assert.Equal(new[] { "MouseDown:Middle" }, log);
		Assert.Equal(MouseButtonState.Pressed, Mouse.MiddleButton);
	}

	[Fact]
	public void PreviewMouseLeftButtonDown_follows_tunnel_order()
	{
		var root = new MouseTestElement("root");
		var mid = new MouseTestElement("mid", root);
		var leaf = new MouseTestElement("leaf", mid);
		var log = new List<string>();
		foreach (var el in new[] { root, mid, leaf })
			el.PreviewMouseLeftButtonDown += (s, e) => log.Add(Tag(s));

		leaf.RaiseMouseButton(MouseButton.Left, P, 1, pressed: true);

		Assert.Equal(new[] { "root", "mid", "leaf" }, log);
	}

	[Fact]
	public void ButtonState_is_Pressed_on_down_and_Released_on_up()
	{
		var el = new MouseTestElement("el");
		var states = new List<MouseButtonState>();
		el.MouseLeftButtonDown += (s, e) => states.Add(e.ButtonState);
		el.MouseLeftButtonUp += (s, e) => states.Add(e.ButtonState);

		el.RaiseMouseButton(MouseButton.Left, P, 1, pressed: true);
		el.RaiseMouseButton(MouseButton.Left, P, 1, pressed: false);

		Assert.Equal(new[] { MouseButtonState.Pressed, MouseButtonState.Released }, states);
	}

	[Fact]
	public void Specific_event_args_carry_specific_RoutedEvent_then_restore_generic()
	{
		var el = new MouseTestElement("el");
		RoutedEvent? seenSpecific = null, seenGeneric = null;
		el.MouseLeftButtonDown += (s, e) => seenSpecific = e.RoutedEvent;
		el.MouseDown += (s, e) => seenGeneric = e.RoutedEvent;

		el.RaiseMouseButton(MouseButton.Left, P, 1, pressed: true);

		Assert.Same(UIElement.MouseLeftButtonDownEvent, seenSpecific);
		Assert.Same(UIElement.MouseDownEvent, seenGeneric);
	}

	[Fact]
	public void Direct_specific_event_is_not_seen_by_parent_instance_handler_of_child_raise()
	{
		// Each element gets its own per-element re-raise; a Direct raise on the child alone
		// (no generic route) does not reach the parent.
		var parent = new MouseTestElement("parent");
		var child = new MouseTestElement("child", parent);
		bool parentSaw = false;
		parent.MouseLeftButtonDown += (s, e) => parentSaw = true;

		var args = new MouseButtonEventArgs(UIElement.MouseLeftButtonDownEvent, P, MouseButton.Left);
		RoutedEventHost.RaiseEvent(child, args);

		Assert.False(parentSaw);
	}

	[Fact]
	public void Mouse_button_states_track_middle_and_x_buttons()
	{
		var el = new MouseTestElement("el");
		el.RaiseMouseButton(MouseButton.XButton1, P, 1, pressed: true);
		el.RaiseMouseButton(MouseButton.XButton2, P, 1, pressed: true);
		Assert.Equal(MouseButtonState.Pressed, Mouse.XButton1);
		Assert.Equal(MouseButtonState.Pressed, Mouse.XButton2);

		MouseButtonEventArgs? up = null;
		el.MouseUp += (s, e) => up = e;
		el.RaiseMouseButton(MouseButton.XButton1, P, 1, pressed: false);
		Assert.Equal(MouseButtonState.Released, up!.ButtonState);
		Assert.Equal(MouseButtonState.Pressed, up.XButton2);
	}
}
