using System;
using System.Windows;
using System.Windows.Input;
using Gtk4.PresentationFramework.Tests.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.InputDevices;

public class MouseDeviceTests : MouseStateTestBase
{
	[Fact]
	public void PrimaryDevice_is_a_singleton_of_an_abstract_type()
	{
		Assert.Same(Mouse.PrimaryDevice, Mouse.PrimaryDevice);
		Assert.True(typeof(MouseDevice).IsAbstract);
		Assert.IsAssignableFrom<InputDevice>(Mouse.PrimaryDevice);
		Assert.Empty(typeof(MouseDevice).GetConstructors());
	}

	[Fact]
	public void Target_and_DirectlyOver_follow_the_mouse_state()
	{
		var el = new MouseTestElement();
		Mouse.DirectlyOver = el;
		Assert.Same(el, Mouse.PrimaryDevice.DirectlyOver);
		Assert.Same(el, Mouse.PrimaryDevice.Target);
	}

	[Fact]
	public void Capture_through_the_device_equals_Mouse_Captured()
	{
		var el = new MouseTestElement();
		Assert.Null(Mouse.PrimaryDevice.Captured);
		_ = Mouse.PrimaryDevice.Capture(el);
		Assert.Equal(Mouse.Captured, Mouse.PrimaryDevice.Captured);
		Assert.Equal(Mouse.Capture(el, CaptureMode.SubTree), Mouse.PrimaryDevice.Capture(el, CaptureMode.SubTree));
		Mouse.PrimaryDevice.Capture(null!, CaptureMode.None);
		Assert.Null(Mouse.Captured);
	}

	[Fact]
	public void Button_states_follow_the_mouse_state()
	{
		Mouse.SetButtonState(MouseButton.Right, MouseButtonState.Pressed);
		Mouse.SetButtonState(MouseButton.XButton2, MouseButtonState.Pressed);
		var d = Mouse.PrimaryDevice;
		Assert.Equal(MouseButtonState.Released, d.LeftButton);
		Assert.Equal(MouseButtonState.Pressed, d.RightButton);
		Assert.Equal(MouseButtonState.Released, d.MiddleButton);
		Assert.Equal(MouseButtonState.Released, d.XButton1);
		Assert.Equal(MouseButtonState.Pressed, d.XButton2);
	}

	[Fact]
	public void Event_args_base_types_match_WPF()
	{
		Assert.Equal(typeof(InputEventArgs), typeof(MouseEventArgs).BaseType);
		Assert.Equal(typeof(MouseEventArgs), typeof(MouseButtonEventArgs).BaseType);
		Assert.Equal(typeof(MouseEventArgs), typeof(MouseWheelEventArgs).BaseType);
		Assert.Equal(typeof(KeyboardEventArgs), typeof(KeyEventArgs).BaseType);
		Assert.Equal(typeof(InputEventArgs), typeof(KeyboardEventArgs).BaseType);
		Assert.Equal(typeof(RoutedEventArgs), typeof(InputEventArgs).BaseType);
	}

	[Fact]
	public void WPF_constructors_set_the_fields_and_leave_RoutedEvent_assignable()
	{
		var d = Mouse.PrimaryDevice;
		var m = new MouseEventArgs(d, 42);
		Assert.Same(d, m.MouseDevice);
		Assert.Same(d, m.Device);
		Assert.Equal(42, m.Timestamp);
		var b = new MouseButtonEventArgs(d, 7, MouseButton.Right);
		Assert.Equal(MouseButton.Right, b.ChangedButton);
		Assert.Equal(7, b.Timestamp);
		var w = new MouseWheelEventArgs(d, 9, -120);
		Assert.Equal(-120, w.Delta);
		Assert.IsAssignableFrom<InputEventArgs>(w);
		m.RoutedEvent = UIElement.MouseMoveEvent;
		var el = new MouseTestElement();
		int hits = 0;
		el.MouseMove += (_, _) => hits++;
		el.RaiseEvent(m);
		Assert.Equal(1, hits);
	}

	[Fact]
	public void WPF_constructed_args_read_live_from_the_device()
	{
		var b = new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left);
		Assert.Equal(MouseButtonState.Released, b.ButtonState);
		Mouse.SetButtonState(MouseButton.Left, MouseButtonState.Pressed);
		Assert.Equal(MouseButtonState.Pressed, b.ButtonState);
		Assert.Equal(MouseButtonState.Pressed, b.LeftButton);
	}

	[Fact]
	public void Null_device_throws()
	{
		Assert.Equal("mouse",
			Assert.Throws<ArgumentNullException>(() => new MouseEventArgs((MouseDevice)null!, 1)).ParamName);
		Assert.Throws<ArgumentNullException>(() => new MouseButtonEventArgs(null!, 1, MouseButton.Left));
		Assert.Throws<ArgumentNullException>(() => new MouseWheelEventArgs(null!, 1, 120));
		Assert.Null(new InputEventArgs(null!, 1).Device);
		Assert.Null(new KeyboardEventArgs(null!, 1).KeyboardDevice);
	}

	[Fact]
	public void MouseButtonEventArgs_validates_the_button()
	{
		var ex = Assert.Throws<System.ComponentModel.InvalidEnumArgumentException>(
			() => new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, (MouseButton)99));
		Assert.Equal("button", ex.ParamName);
		Assert.Throws<System.ComponentModel.InvalidEnumArgumentException>(
			() => new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, (MouseButton)(-1)));
		Assert.Equal(MouseButton.XButton2, new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.XButton2).ChangedButton);
	}

	[Fact]
	public void Legacy_constructors_use_the_primary_device()
	{
		var e = new MouseEventArgs(UIElement.MouseMoveEvent, new Point(1, 2));
		Assert.Same(Mouse.PrimaryDevice, e.MouseDevice);
		Assert.Same(Mouse.PrimaryDevice, e.Device);
		Assert.IsAssignableFrom<InputEventArgs>(e);
		Assert.Equal(new Point(1, 2), e.GetPosition(null));
	}

	[Fact]
	public void Key_args_carry_the_keyboard_device()
	{
		var el = new MouseTestElement();
		KeyEventArgs? got = null;
		el.KeyDown += (_, e) => got = e;
		el.RaiseKeyDownForTest(Key.A, ModifierKeys.Control);
		Assert.NotNull(got);
		Assert.Same(Keyboard.PrimaryDevice, got!.KeyboardDevice);
		Assert.Same(Keyboard.PrimaryDevice, got.Device);
		Assert.IsAssignableFrom<KeyboardEventArgs>(got);
		Assert.Equal(ModifierKeys.Control, got.KeyboardDevice.Modifiers);
		Assert.Equal(ModifierKeys.Control, got.Modifiers);
		Keyboard.Modifiers = ModifierKeys.None;
	}
}
