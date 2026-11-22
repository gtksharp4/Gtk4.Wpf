using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.InputDevices;

/// <summary>The GTK key and mouse dispatch stamps device, modifiers and the GDK event time on the event args.</summary>
public class InputDeviceGtkPathTests : WpfGtkTestBase
{
	static void Pump()
	{
		for (int i = 0; i < 200 && Gtk.Application.EventsPending(); i++)
			Gtk.Application.RunIteration(false);
	}

	static void Realized(System.Action<Canvas> body)
	{
		var canvas = new Canvas { Focusable = true };
		var win = new Gtk.Window();
		win.SetDefaultSize(200, 200);
		win.Child = canvas.Widget;
		win.Present();
		try
		{
			Pump();
			body(canvas);
		}
		finally
		{
			Mouse.Capture(null);
			Mouse.SetButtonState(MouseButton.Left, MouseButtonState.Released);
			Mouse.DirectlyOver = null;
			Keyboard.Modifiers = ModifierKeys.None;
			win.Destroy();
		}
	}

	const uint KeyvalA = 0x61;

	[Fact]
	public void The_GTK_key_press_carries_the_primary_keyboard_and_the_Control_modifier() => Run(() =>
		Realized(el =>
		{
			KeyEventArgs? down = null;
			el.KeyDown += (_, e) => down = e;
			el.DispatchGtkKeyPressed(KeyvalA, Gdk.ModifierType.ControlMask, 4242);

			Assert.NotNull(down);
			Assert.Same(Keyboard.PrimaryDevice, down!.KeyboardDevice);
			Assert.Same(down.Device, down.KeyboardDevice);
			Assert.Equal(Key.A, down.Key);
			Assert.True((Keyboard.Modifiers & ModifierKeys.Control) != 0);
			Assert.True((down.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0);
			Assert.True((down.Modifiers & ModifierKeys.Control) != 0);
		}));

	[Fact]
	public void Preview_and_bubble_key_events_share_the_GDK_time() => Run(() =>
		Realized(el =>
		{
			var seen = new List<int>();
			el.PreviewKeyDown += (_, e) => seen.Add(e.Timestamp);
			el.KeyDown += (_, e) => seen.Add(e.Timestamp);
			el.PreviewKeyUp += (_, e) => seen.Add(e.Timestamp);
			el.KeyUp += (_, e) => seen.Add(e.Timestamp);
			el.DispatchGtkKeyPressed(KeyvalA, 0, 4242);
			el.DispatchGtkKeyReleased(KeyvalA, 0, 4243);
			Assert.Equal(new[] { 4242, 4242, 4243, 4243 }, seen);
		}));

	[Fact]
	public void Focus_raises_GotKeyboardFocus_with_the_primary_keyboard_and_old_and_new_focus() => Run(() =>
		Realized(el =>
		{
			var other = new Canvas { Focusable = true };
			Assert.True(other.Focus());
			KeyboardFocusChangedEventArgs? got = null;
			el.GotKeyboardFocus += (_, e) => got = e;
			Assert.True(el.Focus());

			Assert.NotNull(got);
			Assert.Same(Keyboard.PrimaryDevice, got!.KeyboardDevice);
			Assert.Same(other, got.OldFocus);
			Assert.Same(el, got.NewFocus);
		}));

	[Fact]
	public void Preview_and_bubble_mouse_events_share_the_GDK_time() => Run(() =>
		Realized(el =>
		{
			var seen = new List<int>();
			el.PreviewMouseDown += (_, e) => seen.Add(e.Timestamp);
			el.MouseDown += (_, e) => seen.Add(e.Timestamp);
			el.PreviewMouseMove += (_, e) => seen.Add(e.Timestamp);
			el.MouseMove += (_, e) => seen.Add(e.Timestamp);
			el.DispatchGtkButton(new MouseEventKey(4, 123456, 1, 2), 1, new Point(5, 6), 1, pressed: true);
			el.DispatchGtkMotion(new MouseEventKey(3, 123457, 1, 2), new Point(7, 8));
			Assert.Equal(new[] { 123456, 123456, 123457, 123457 }, seen);
		}));

	[Fact]
	public void Programmatic_mouse_events_share_one_timestamp_per_dispatch() => Run(() =>
		Realized(el =>
		{
			var seen = new List<int>();
			el.PreviewMouseDown += (_, e) => seen.Add(e.Timestamp);
			el.MouseDown += (_, e) => seen.Add(e.Timestamp);
			el.RaiseMouseButton(MouseButton.Left, new Point(1, 1), 1, pressed: true);
			Assert.Equal(2, seen.Count);
			Assert.Equal(seen[0], seen[1]);
		}));

	[Fact]
	public void Capture_through_the_device_equals_Mouse_Captured() => Run(() =>
		Realized(el =>
		{
			Assert.True(Mouse.PrimaryDevice.Capture(el));
			Assert.Same(el, Mouse.Captured);
			Assert.Same(el, Mouse.PrimaryDevice.Captured);

			Assert.True(Mouse.PrimaryDevice.Capture(null));
			Assert.Null(Mouse.Captured);
			Assert.Null(Mouse.PrimaryDevice.Captured);
		}));

	[Fact]
	public void Device_GetPosition_null_matches_Mouse_GetPosition_null_after_motion() => Run(() =>
		Realized(el =>
		{
			el.DispatchGtkMotion(new MouseEventKey(3, 1000, 1, 2), new Point(9, 11));
			Assert.Equal(Mouse.GetPosition(null), Mouse.PrimaryDevice.GetPosition(null));
		}));
}
