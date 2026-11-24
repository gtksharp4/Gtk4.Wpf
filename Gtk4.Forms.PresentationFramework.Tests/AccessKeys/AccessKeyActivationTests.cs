using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.AccessKeys;

/// <summary>Alt+key and plain-key activation through the window's key handling.</summary>
public class AccessKeyActivationTests : AccessKeyTestBase
{
	const uint KeyC = 'c', KeyF = 'f', KeyN = 'n', KeyO = 'o', KeyR = 'r';

	sealed class CountingCommand : ICommand
	{
		public int Executed;
#pragma warning disable CS0067
		public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
		public bool CanExecute(object? parameter) => true;
		public void Execute(object? parameter) => Executed++;
	}

	static bool Alt(Window w, uint key) => w.HandleAccessKeyval(key, Gdk.ModifierType.AltMask);

	static StackPanel Stack(params UIElement[] children)
	{
		var panel = new StackPanel();
		foreach (var child in children) panel.Children.Add(child);
		return panel;
	}

	[Fact]
	public void Alt_key_clicks_a_button_exactly_once_and_runs_the_command_once() => Run(() =>
	{
		var command = new CountingCommand();
		var button = new Button { Content = "_Open", Command = command };
		var window = Show(Stack(button));
		int clicks = 0;
		button.Click += (_, _) => clicks++;
		Assert.True(Alt(window, KeyO));
		GtkPump.For(100);
		Assert.Equal(1, clicks);
		Assert.Equal(1, command.Executed);
		Assert.False(Alt(window, 'x'));
		window.Close();
	});

	[Fact]
	public void Alt_key_toggles_a_check_box_and_checks_a_radio_button() => Run(() =>
	{
		var check = new CheckBox { Content = "_Check" };
		var radio = new RadioButton { Content = "_Radio" };
		var window = Show(Stack(check, radio));
		Assert.True(Alt(window, KeyC));
		Assert.True(check.IsChecked);
		Assert.True(Alt(window, KeyC));
		Assert.False(check.IsChecked);
		Assert.True(Alt(window, KeyR));
		Assert.True(radio.IsChecked);
		window.Close();
	});

	[Fact]
	public void Label_target_receives_focus_and_a_label_without_target_does_nothing() => Run(() =>
	{
		var box = new TextBox();
		var named = new Label { Content = "_Name", Target = box };
		var orphan = new Label { Content = "_Other" };
		var window = Show(Stack(named, box, orphan));
		Assert.True(Alt(window, KeyN));
		GtkPump.For(100);
		Assert.True(box.IsKeyboardFocused);
		Assert.True(Alt(window, 'o')); // swallowed: registered, but names no target
		Assert.True(box.IsKeyboardFocused);
		window.Close();
	});

	[Fact]
	public void Disabled_buttons_and_other_windows_are_ignored() => Run(() =>
	{
		var disabled = new Button { Content = "_Open", IsEnabled = false };
		var other = new Button { Content = "_Open" };
		var win1 = Show(Stack(disabled));
		var win2 = Show(Stack(other));
		int clicks = 0;
		disabled.Click += (_, _) => clicks++;
		other.Click += (_, _) => clicks += 10;
		Assert.False(Alt(win1, KeyO));
		Assert.Equal(0, clicks);
		Assert.True(Alt(win2, KeyO));
		Assert.Equal(10, clicks);
		win1.Close();
		win2.Close();
	});

	[Fact]
	public void A_focused_TextBox_types_plain_keys_but_Alt_key_still_clicks() => Run(() =>
	{
		var box = new TextBox();
		var button = new Button { Content = "_Open" };
		var window = Show(Stack(box, button));
		int clicks = 0;
		button.Click += (_, _) => clicks++;
		box.Focus();
		GtkPump.For(100);
		box.DispatchGtkKeyPressed(KeyO, 0, 1);
		Assert.Equal(0, clicks);
		Assert.True(Alt(window, KeyO));
		Assert.Equal(1, clicks);
		window.Close();
	});

	[Fact]
	public void A_plain_key_fires_when_the_focused_element_leaves_TextInput_unhandled() => Run(() =>
	{
		var other = new Button { Content = "x" };
		var button = new Button { Content = "_Open" };
		var window = Show(Stack(other, button));
		int clicks = 0;
		button.Click += (_, _) => clicks++;
		other.Focus();
		GtkPump.For(100);
		other.DispatchGtkKeyPressed(KeyO, 0, 1);
		Assert.Equal(1, clicks);
		window.Close();
	});

	[Fact]
	public void Menu_item_opens_with_Alt_key_and_its_open_submenu_takes_plain_keys() => Run(() =>
	{
		var open = new MenuItem { Header = "_Open" };
		var file = new MenuItem { Header = "_File" };
		file.Items.Add(open);
		var menu = new Menu();
		menu.Items.Add(file);
		var window = Show(Stack(menu));
		int clicks = 0;
		open.Click += (_, _) => clicks++;
		Assert.False(Alt(window, KeyO));
		Assert.True(Alt(window, KeyF));
		GtkPump.For(100);
		Assert.True(file.IsSubmenuOpen);
		file.DispatchGtkKeyPressed(KeyO, 0, 1);
		Assert.Equal(1, clicks);
		Assert.False(file.IsSubmenuOpen);
		window.Close();
	});

	[Fact]
	public void Peers_report_the_normalized_key_and_the_stripped_name() => Run(() =>
	{
		var button = new Button { Content = "_Open" };
		var box = new TextBox();
		var label = new Label { Content = "_Name", Target = box };
		var window = Show(Stack(button, label, box));
		var peer = UIElementAutomationPeer.CreatePeerForElement(button)!;
		Assert.Equal("O", peer.GetAccessKey());
		Assert.Equal("Open", peer.GetName());
		Assert.Equal("N", UIElementAutomationPeer.CreatePeerForElement(box)!.GetAccessKey());
		Assert.Equal(string.Empty, UIElementAutomationPeer.CreatePeerForElement(new Button())!.GetAccessKey());
		window.Close();
	});
}
