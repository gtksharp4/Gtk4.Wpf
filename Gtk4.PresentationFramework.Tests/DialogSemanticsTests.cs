using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Button.IsDefault/IsCancel/IsDefaulted, settable Window.DialogResult, HeaderedContentControl.</summary>
public class DialogSemanticsTests : WpfGtkTestBase
{
	static void ScheduleClose(Action action) =>
		GLib.Timeout.Add(10, () => { action(); return false; });

	// A dialog that is never dismissed would pump forever; the watchdog closes it and fails the test.
	static bool? ShowDialogWithWatchdog(WpfWindow window)
	{
		bool done = false, timedOut = false;
		GLib.Timeout.Add(5000, () =>
		{
			if (done) return false;
			timedOut = true;
			window.Close();
			return false;
		});
		var result = window.ShowDialog();
		done = true;
		Assert.False(timedOut, "The dialog was not dismissed.");
		return result;
	}

	static void PressOnWindow(WpfWindow window, uint keyval)
	{
		_ = window.Widget;
		GLib.Signal.Emit(window._acceleratorController!, "key-pressed", keyval, 0u, Gdk.ModifierType.None);
	}

	sealed class Form
	{
		public WpfWindow Window = new WpfWindow();
		public TextBox Input = new TextBox();
		public Button Ok = new Button { Content = "OK", IsDefault = true };
		public Button Cancel = new Button { Content = "Cancel", IsCancel = true };
		public Button Other = new Button { Content = "Other" };

		public Form()
		{
			// Keyboard focus is process-wide: an element an earlier test left focused would swallow Enter/Esc.
			Keyboard.ClearFocus();
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
		Assert.True(ShowDialogWithWatchdog(win));
		Assert.True(closed);
		Assert.True(win.DialogResult);
	});

	[Fact]
	public void DialogResult_false_is_returned() => Run(() =>
	{
		var win = new WpfWindow { Title = "dr" };
		ScheduleClose(() => win.DialogResult = false);
		Assert.False(ShowDialogWithWatchdog(win));
	});

	[Fact]
	public void A_default_button_handler_can_accept_the_dialog() => Run(() =>
	{
		var f = new Form();
		f.Ok.Click += (_, _) => f.Window.DialogResult = true;
		ScheduleClose(() => f.Input.RaiseKeyDownForTest(Key.Enter));
		Assert.True(ShowDialogWithWatchdog(f.Window));
	});

	[Fact]
	public void A_cancel_button_handler_can_dismiss_the_dialog() => Run(() =>
	{
		var f = new Form();
		int clicks = 0;
		f.Cancel.Click += (_, _) => clicks++;
		ScheduleClose(() => f.Input.RaiseKeyDownForTest(Key.Escape));
		Assert.False(ShowDialogWithWatchdog(f.Window));
		Assert.Equal(1, clicks);
	});

	[Fact]
	public void A_cancel_button_without_a_handler_dismisses_the_dialog() => Run(() =>
	{
		var f = new Form();
		ScheduleClose(() => f.Cancel.ClickFromAccelerator());
		Assert.False(ShowDialogWithWatchdog(f.Window));
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
		ShowDialogWithWatchdog(f.Window);
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
		Assert.True(ShowDialogWithWatchdog(win));
	});

	[Fact]
	public void Closing_a_dialog_without_a_result_returns_false() => Run(() =>
	{
		var win = new WpfWindow { Title = "dr" };
		ScheduleClose(() => win.Close());
		Assert.Equal(false, ShowDialogWithWatchdog(win));
		Assert.False(win.DialogResult);
	});

	[Fact]
	public void Return_through_the_window_key_controller_clicks_the_default_button() => Run(() =>
	{
		var f = new Form();
		int ok = 0;
		f.Ok.Click += (_, _) => ok++;
		PressOnWindow(f.Window, 0xff0d);
		Assert.Equal(1, ok);
	});

	[Fact]
	public void A_focus_left_in_another_window_does_not_block_the_default_button() => Run(() =>
	{
		var other = new Form();
		var f = new Form();
		int ok = 0;
		f.Ok.Click += (_, _) => ok++;
		other.Input.SetFocusedInternal(true);
		try
		{
			PressOnWindow(f.Window, 0xff0d);
			Assert.Equal(1, ok);
		}
		finally { other.Input.SetFocusedInternal(false); }
	});


	[Fact]
	public void Enter_in_a_TextBox_inside_a_Popup_does_not_click_the_default_button() => Run(() =>
	{
		var f = new Form();
		int ok = 0;
		f.Ok.Click += (_, _) => ok++;
		var box = new TextBox();
		new Popup().Child = box;
		box.SetFocusedInternal(true);
		try
		{
			PressOnWindow(f.Window, 0xff0d);
			Assert.Equal(0, ok);
		}
		finally { box.SetFocusedInternal(false); }
	});
	[Fact]
	public void Keypad_Enter_through_the_window_key_controller_clicks_the_default_button() => Run(() =>
	{
		var f = new Form();
		int ok = 0;
		f.Ok.Click += (_, _) => ok++;
		PressOnWindow(f.Window, 0xff8d);
		Assert.Equal(1, ok);
	});

	[Fact]
	public void Escape_through_the_window_key_controller_clicks_the_cancel_button() => Run(() =>
	{
		var f = new Form();
		int cancel = 0;
		f.Cancel.Click += (_, _) => cancel++;
		PressOnWindow(f.Window, 0xff1b);
		Assert.Equal(1, cancel);
	});

	[Fact]
	public void The_window_controller_leaves_a_focused_elements_Enter_to_its_KeyDown() => Run(() =>
	{
		var f = new Form();
		int ok = 0;
		f.Ok.Click += (_, _) => ok++;
		f.Input.SetFocusedInternal(true);
		PressOnWindow(f.Window, 0xff0d);
		f.Input.SetFocusedInternal(false);
		Assert.Equal(0, ok);
	});

	[Fact]
	public void A_KeyDown_handler_that_handles_Enter_stops_the_default_button() => Run(() =>
	{
		var f = new Form();
		int ok = 0, seen = 0;
		f.Ok.Click += (_, _) => ok++;
		f.Input.KeyDown += (_, e) => { seen++; e.Handled = true; };
		f.Input.RaiseKeyDownForTest(Key.Enter);
		Assert.Equal(1, seen);
		Assert.Equal(0, ok);
	});

	[Fact]
	public void Enter_in_a_RichTextBox_and_Escape_in_an_open_ComboBox_do_not_click_buttons() => Run(() =>
	{
		var f = new Form();
		int ok = 0, cancel = 0;
		f.Ok.Click += (_, _) => ok++;
		f.Cancel.Click += (_, _) => cancel++;
		var rtb = new RichTextBox();
		((StackPanel)f.Window.Content!).Children.Add(rtb);
		rtb.RaiseKeyDownForTest(Key.Enter);
		var combo = new ComboBox { IsDropDownOpen = true };
		((StackPanel)f.Window.Content!).Children.Add(combo);
		combo.RaiseKeyDownForTest(Key.Escape);
		Assert.Equal(0, ok);
		Assert.Equal(0, cancel);
	});

	[Fact]
	public void Return_is_left_to_a_multiline_TextBox_or_a_focused_button() => Run(() =>
	{
		var f = new Form();
		int ok = 0;
		f.Ok.Click += (_, _) => ok++;
		f.Input.AcceptsReturn = true;
		f.Input.SetFocusedInternal(true);
		f.Input.RaiseKeyDownForTest(Key.Enter);
		f.Other.SetFocusedInternal(true);
		f.Other.RaiseKeyDownForTest(Key.Enter);
		f.Other.SetFocusedInternal(false);
		f.Input.SetFocusedInternal(false);
		Assert.Equal(0, ok);
	});
}
