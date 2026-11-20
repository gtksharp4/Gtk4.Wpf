using System;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Button.IsDefault/IsCancel/IsDefaulted, settable Window.DialogResult, ContentSource, HeaderedContentControl.</summary>
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
	public void IsDefaulted_follows_IsDefault() => Run(() =>
	{
		var b = new Button { IsDefault = true };
		Assert.True(b.IsDefaulted);
		b.IsDefault = false;
		Assert.False(b.IsDefaulted);
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
		f.Cancel.Click += (_, _) => f.Window.DialogResult = false;
		ScheduleClose(() => f.Input.RaiseKeyDownForTest(Key.Escape));
		Assert.False(f.Window.ShowDialog());
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
}
