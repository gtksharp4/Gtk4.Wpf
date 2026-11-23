using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.InputDevices;

public class TextCompositionTimestampTests : WpfGtkTestBase
{
	const uint KeyvalA = 0x61;

	[Fact]
	public void TextCompositionEventArgs_derives_from_InputEventArgs_and_keeps_its_ctor()
	{
		Assert.True(typeof(InputEventArgs).IsAssignableFrom(typeof(TextCompositionEventArgs)));
		int before = Environment.TickCount;
		var e = new TextCompositionEventArgs(UIElement.TextInputEvent, "x");
		int after = Environment.TickCount;
		Assert.Equal("x", e.Text);
		Assert.Same(UIElement.TextInputEvent, e.RoutedEvent);
		Assert.Same(Keyboard.PrimaryDevice, e.Device);
		Assert.InRange(e.Timestamp, before, after);
	}

	[Fact]
	public void A_null_text_reads_as_empty() =>
		Assert.Equal(string.Empty, new TextCompositionEventArgs(UIElement.TextInputEvent, null!).Text);

	[Fact]
	public void The_GTK_key_path_stamps_the_GDK_time_on_preview_and_bubble_text_input() => Run(() =>
	{
		var canvas = new Canvas { Focusable = true };
		var seen = new List<int>();
		canvas.PreviewTextInput += (_, e) => seen.Add(e.Timestamp);
		canvas.TextInput += (_, e) => seen.Add(e.Timestamp);
		canvas.DispatchGtkKeyPressed(KeyvalA, 0, 4242);
		Assert.Equal(new[] { 4242, 4242 }, seen);
	});

	[Fact]
	public void Keyboard_focus_changes_are_stamped_with_the_tick_count() => Run(() =>
	{
		var a = new Button();
		var panel = new StackPanel();
		panel.Children.Add(a);
		int stamp = 0;
		a.GotKeyboardFocus += (_, e) => stamp = e.Timestamp;
		int before = Environment.TickCount;
		Assert.True(a.Focus());
		int after = Environment.TickCount;
		Assert.InRange(stamp, before, after);
		a.SetFocusedInternal(false);
	});
}
