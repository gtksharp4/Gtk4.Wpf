using System;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Button.IsDefault/IsCancel/IsDefaulted, settable Window.DialogResult, HeaderedContentControl.</summary>
public class DialogSemanticsTests : WpfGtkTestBase
{
	static void ScheduleClose(Action action) =>
		GLib.Timeout.Add(10, () => { action(); return false; });

	sealed class Form
	{
		public WpfWindow Window = new WpfWindow();
		public TextBox Input = new TextBox();
		public Button Ok = new Button { Content = "OK", IsDefault = true };
		public Button Cancel = new Button { Content = "Cancel", IsCancel = true };
		public Button Other = new Button { Content = "Other" };

		public Form()
		{
			var panel = new StackPanel();
			panel.Children.Add(Input);
			panel.Children.Add(Ok);
			panel.Children.Add(Cancel);
			panel.Children.Add(Other);
			Window.Content = panel;
		}
	}

	[Fact]
	public void Button_defaults() => Run(() =>
	{
		var b = new Button();
		Assert.False(b.IsDefault);
		Assert.False(b.IsCancel);
		Assert.False(b.IsDefaulted);
	});

	[Fact]
	public void Enter_in_a_TextBox_clicks_the_default_button() => Run(() =>
	{
		var f = new Form();
		int ok = 0, cancel = 0;
		f.Ok.Click += (_, _) => ok++;
		f.Cancel.Click += (_, _) => cancel++;
		f.Input.RaiseKeyDownForTest(Key.Enter);
		Assert.Equal(1, ok);
		Assert.Equal(0, cancel);
	});

	[Fact]
	public void Escape_clicks_the_cancel_button() => Run(() =>
	{
		var f = new Form();
		int ok = 0, cancel = 0;
		f.Ok.Click += (_, _) => ok++;
		f.Cancel.Click += (_, _) => cancel++;
		f.Input.RaiseKeyDownForTest(Key.Escape);
		Assert.Equal(0, ok);
		Assert.Equal(1, cancel);
	});

	[Fact]
	public void Enter_on_a_focused_button_is_left_to_that_button() => Run(() =>
	{
		var f = new Form();
		int ok = 0;
		f.Ok.Click += (_, _) => ok++;
		f.Other.RaiseKeyDownForTest(Key.Enter);
		Assert.Equal(0, ok);
	});

	[Fact]
	public void Enter_in_a_multiline_TextBox_is_not_stolen() => Run(() =>
	{
		var f = new Form();
		f.Input.AcceptsReturn = true;
		int ok = 0;
		f.Ok.Click += (_, _) => ok++;
		f.Input.RaiseKeyDownForTest(Key.Enter);
		Assert.Equal(0, ok);
	});

	[Fact]
	public void A_disabled_default_button_is_not_clicked() => Run(() =>
	{
		var f = new Form();
		f.Ok.IsEnabled = false;
		int ok = 0;
		f.Ok.Click += (_, _) => ok++;
		f.Input.RaiseKeyDownForTest(Key.Enter);
		Assert.Equal(0, ok);
	});

	[Fact]
	public void IsDefaulted_needs_a_window_that_holds_focus() => Run(() =>
	{
		var loose = new Button { IsDefault = true };
		Assert.False(loose.IsDefaulted);

		var f = new Form();
		f.Input.SetFocusedInternal(true);
		Assert.True(f.Ok.IsDefaulted);
		f.Other.SetFocusedInternal(true);
		Assert.False(f.Ok.IsDefaulted);
		f.Input.SetFocusedInternal(true);
		f.Ok.IsDefault = false;
		Assert.False(f.Ok.IsDefaulted);
		f.Input.SetFocusedInternal(false);
	});

	[Fact]
	public void DialogResult_set_outside_a_dialog_throws() => Run(() =>
	{
		var win = new WpfWindow();
		Assert.Throws<InvalidOperationException>(() => win.DialogResult = true);
	});

	[Fact]
	public void Setting_DialogResult_closes_the_dialog_and_ShowDialog_returns_it() => Run(() =>
	{
		var win = new WpfWindow { Title = "dr" };
		bool closed = false;
		win.Closed += (_, _) => closed = true;
		ScheduleClose(() => win.DialogResult = true);
		Assert.True(win.ShowDialog());
		Assert.True(closed);
		Assert.True(win.DialogResult);
	});

	[Fact]
	public void DialogResult_false_is_returned() => Run(() =>
	{
		var win = new WpfWindow { Title = "dr" };
		ScheduleClose(() => win.DialogResult = false);
		Assert.False(win.ShowDialog());
	});

	[Fact]
	public void A_default_button_handler_can_accept_the_dialog() => Run(() =>
	{
		var f = new Form();
		f.Ok.Click += (_, _) => f.Window.DialogResult = true;
		ScheduleClose(() => f.Input.RaiseKeyDownForTest(Key.Enter));
		Assert.True(f.Window.ShowDialog());
	});

	[Fact]
	public void A_cancel_button_handler_can_dismiss_the_dialog() => Run(() =>
	{
		var f = new Form();
		int clicks = 0;
		f.Cancel.Click += (_, _) => clicks++;
		ScheduleClose(() => f.Input.RaiseKeyDownForTest(Key.Escape));
		Assert.False(f.Window.ShowDialog());
		Assert.Equal(1, clicks);
	});

	[Fact]
	public void A_cancel_button_without_a_handler_dismisses_the_dialog() => Run(() =>
	{
		var f = new Form();
		ScheduleClose(() => f.Cancel.ClickFromAccelerator());
		Assert.False(f.Window.ShowDialog());
		Assert.True(f.Window.DialogResult == false);
	});

	[Fact]
	public void A_cancel_button_with_a_command_does_not_dismiss_the_dialog() => Run(() =>
	{
		var f = new Form();
		f.Cancel.Command = System.Windows.Input.ApplicationCommands.Close;
		ScheduleClose(() =>
		{
			f.Cancel.ClickFromAccelerator();
			Assert.Null(f.Window.DialogResult);
			f.Window.Close();
		});
		f.Window.ShowDialog();
	});

	[Fact]
	public void ContentPresenter_ContentSource_defaults_to_Content_and_is_settable() => Run(() =>
	{
		var cp = new ContentPresenter();
		Assert.Equal("Content", cp.ContentSource);
		cp.ContentSource = "Header";
		Assert.Equal("Header", cp.ContentSource);
	});

	[Fact]
	public void HeaderedContentControl_is_concrete() => Run(() =>
	{
		Assert.False(typeof(HeaderedContentControl).IsAbstract);
		var hcc = new HeaderedContentControl { Header = "h", Content = "c" };
		Assert.Equal("h", hcc.Header);
		Assert.Equal("c", hcc.Content);
	});

	[Fact]
	public void A_Closing_handler_can_set_DialogResult() => Run(() =>
	{
		var win = new WpfWindow { Title = "dr" };
		win.Closing += (_, _) => win.DialogResult = true;
		ScheduleClose(() => win.Close());
		Assert.True(win.ShowDialog());
	});

	[Fact]
	public void Closing_a_dialog_without_a_result_returns_false() => Run(() =>
	{
		var win = new WpfWindow { Title = "dr" };
		ScheduleClose(() => win.Close());
		Assert.Equal(false, win.ShowDialog());
	});

	[Fact]
	public void Return_through_the_window_key_controller_clicks_the_default_button() => Run(() =>
	{
		var f = new Form();
		int ok = 0;
		f.Ok.Click += (_, _) => ok++;
		f.Input.SetFocusedInternal(true);
		bool handled = f.Window.HandleAcceleratorKeyval(0xff0d, default);
		f.Input.SetFocusedInternal(false);
		Assert.True(handled);
		Assert.Equal(1, ok);
	});

	[Fact]
	public void Keypad_Enter_clicks_the_default_button_too() => Run(() =>
	{
		var f = new Form();
		int ok = 0;
		f.Ok.Click += (_, _) => ok++;
		Assert.True(f.Window.HandleAcceleratorKeyval(0xff8d, default));
		Assert.Equal(1, ok);
	});

	[Fact]
	public void Escape_with_no_focused_element_clicks_the_cancel_button() => Run(() =>
	{
		var f = new Form();
		(Keyboard.FocusedElement as System.Windows.UIElement)?.SetFocusedInternal(false);
		int cancel = 0;
		f.Cancel.Click += (_, _) => cancel++;
		Assert.True(f.Window.HandleAcceleratorKeyval(0xff1b, default));
		Assert.Equal(1, cancel);
	});

	[Fact]
	public void Return_is_left_to_a_multiline_TextBox_or_a_focused_button() => Run(() =>
	{
		var f = new Form();
		int ok = 0;
		f.Ok.Click += (_, _) => ok++;
		f.Input.AcceptsReturn = true;
		f.Input.SetFocusedInternal(true);
		Assert.False(f.Window.HandleAcceleratorKeyval(0xff0d, default));
		f.Other.SetFocusedInternal(true);
		Assert.False(f.Window.HandleAcceleratorKeyval(0xff0d, default));
		f.Other.SetFocusedInternal(false);
		Assert.Equal(0, ok);
	});
}
