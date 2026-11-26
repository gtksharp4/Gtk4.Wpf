using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class PreviewInputEventTests : WpfGtkTestBase
{
	[Fact]
	public void Preview_key_events_have_CLR_events_and_tunnel_before_the_bubble_events() => Run(() =>
	{
		var inner = new Button();
		var outer = new StackPanel();
		outer.Children.Add(inner);
		var log = new List<string>();
		outer.PreviewKeyDown += (_, e) => log.Add("outer-preview");
		inner.KeyDown += (_, e) => log.Add("inner-keydown");
		inner.PreviewKeyUp += (_, _) => log.Add("preview-up");
		inner.RaiseKeyDownForTest(Key.A);
		inner.RaiseKeyUpForTest(Key.A);
		Assert.Equal(new[] { "outer-preview", "inner-keydown", "preview-up" }, log);
	});

	[Fact]
	public void Preview_mouse_and_text_events_have_CLR_events() => Run(() =>
	{
		var b = new Button();
		MouseButtonEventHandler down = (_, _) => { };
		b.PreviewMouseDown += down;
		b.PreviewMouseUp += down;
		b.PreviewTextInput += (_, _) => { };
		b.PreviewMouseDown -= down;
		b.PreviewMouseUp -= down;
		int seen = 0;
		b.PreviewMouseDown += (_, _) => seen++;
		b.RaiseEvent(new MouseButtonEventArgs(UIElement.PreviewMouseDownEvent, new Point(), MouseButton.Left, 1));
		Assert.Equal(1, seen);
	});

	[Fact]
	public void KeyConverter_converts_names_letters_and_digits() => Run(() =>
	{
		var c = new KeyConverter();
		Assert.Equal(Key.A, c.ConvertFromString("a"));
		Assert.Equal(Key.D7, c.ConvertFromString("7"));
		Assert.Equal(Key.Escape, c.ConvertFromString("escape"));
		Assert.Equal(Key.None, c.ConvertFromString(""));
		Assert.Throws<System.NotSupportedException>(() => c.ConvertFromString("NotAKey"));
		Assert.Equal("7", c.ConvertToString(Key.D7));
		Assert.Equal("Enter", c.ConvertToString(Key.Enter));
	});

	[Fact]
	public void RoutedCommand_overloads_accept_an_IInputElement_target() => Run(() =>
	{
		var cmd = new RoutedCommand();
		var b = new Button();
		int n = 0;
		b.CommandBindings.Add(new CommandBinding(cmd, (_, _) => n++, (_, e) => e.CanExecute = true));
		IInputElement target = b;
		Assert.True(cmd.CanExecute(null, target));
		cmd.Execute(null, target);
		Assert.Equal(1, n);
	});
}
