using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.AccessKeys;

/// <summary>Scopes, ProcessKey results, registration rules and GTK mnemonic suppression.</summary>
public class AccessKeyScopeTests : AccessKeyTestBase
{
	const uint KeyF = 'f', KeyO = 'o';

	static bool Alt(Window w, uint key) => w.HandleAccessKeyval(key, Gdk.ModifierType.AltMask);

	static StackPanel Stack(params UIElement[] children)
	{
		var panel = new StackPanel();
		foreach (var child in children) panel.Children.Add(child);
		return panel;
	}

	[Fact]
	public void Alt_key_clicks_the_button_of_its_own_window_only() => Run(() =>
	{
		var button1 = new Button { Content = "_Open" };
		var button2 = new Button { Content = "_Open" };
		var win1 = Show(Stack(button1));
		var win2 = Show(Stack(button2));
		int clicks1 = 0, clicks2 = 0;
		button1.Click += (_, _) => clicks1++;
		button2.Click += (_, _) => clicks2++;
		Assert.True(Alt(win2, KeyO));
		Assert.Equal(1, clicks2);
		Assert.Equal(0, clicks1);
		win1.Close();
		win2.Close();
	});

	[Fact]
	public void Alt_key_opens_the_menu_of_its_own_window_only() => Run(() =>
	{
		MenuItem Item() => new MenuItem { Header = "_File", Items = { new MenuItem { Header = "x" } } };
		var file1 = Item();
		var file2 = Item();
		var win1 = Show(Stack(new Menu { Items = { file1 } }));
		var win2 = Show(Stack(new Menu { Items = { file2 } }));
		Assert.True(Alt(win2, KeyF));
		Assert.True(file2.IsSubmenuOpen);
		Assert.False(file1.IsSubmenuOpen);
		win1.Close();
		win2.Close();
	});

	[Fact]
	public void ProcessKey_returns_false_for_one_match_and_true_for_several() => Run(() =>
	{
		var one = new Button { Content = "_Go" };
		var panel = Stack(one);
		var window = Show(panel);
		int clicks = 0;
		one.Click += (_, _) => clicks++;
		Assert.False(AccessKeyManager.ProcessKey(window, "G", false));
		Assert.Equal(1, clicks);

		var two = new Button { Content = "_Go" };
		panel.Children.Add(two);
		GtkPump.Until(() => two.IsVisible);
		two.Click += (_, _) => clicks++;
		one.Focus();
		GtkPump.For(100);
		Assert.True(AccessKeyManager.ProcessKey(window, "G", false));
		GtkPump.For(50);
		Assert.True(two.IsKeyboardFocused);
		Assert.Equal(1, clicks);
		window.Close();
	});

	[Fact]
	public void An_open_submenu_item_narrows_the_key_to_its_own_scope() => Run(() =>
	{
		var open = new MenuItem { Header = "_Open" };
		var file = new MenuItem { Header = "_File", Items = { open } };
		var button = new Button { Content = "_Open" };
		var window = Show(Stack(new Menu { Items = { file } }, button));
		int menuClicks = 0, buttonClicks = 0;
		open.Click += (_, _) => menuClicks++;
		button.Click += (_, _) => buttonClicks++;
		file.IsSubmenuOpen = true;
		GtkPump.For(100);
		open.DispatchGtkKeyPressed(KeyO, 0, 1);
		Assert.Equal(1, menuClicks);
		Assert.Equal(0, buttonClicks);
		window.Close();
	});

	[Fact]
	public void A_label_without_target_is_not_registered_and_gains_a_target() => Run(() =>
	{
		var other = new Label { Content = "_Other" };
		var box = new TextBox();
		var window = Show(Stack(other, box));
		Assert.False(AccessKeyManager.IsKeyRegistered(window, "O"));
		other.Target = box;
		Assert.True(AccessKeyManager.IsKeyRegistered(window, "O"));
		window.Close();
	});

	[Fact]
	public void A_key_of_a_label_without_target_is_swallowed_and_focus_stays() => Run(() =>
	{
		var other = new Label { Content = "_Other" };
		var box = new TextBox();
		var window = Show(Stack(other, box));
		box.Focus();
		GtkPump.For(100);
		Assert.True(Alt(window, KeyO));
		Assert.True(box.IsKeyboardFocused);
		window.Close();
	});

	[Fact]
	public void The_empty_key_registers_and_longer_keys_still_throw() => Run(() =>
	{
		var button = new Button();
		var window = Show(button);
		AccessKeyManager.Register("", button);
		Assert.True(AccessKeyManager.IsKeyRegistered(window, ""));
		AccessKeyManager.Unregister("", button);
		Assert.Throws<ArgumentException>(() => AccessKeyManager.Register("ab", button));
		window.Close();
	});

	[Fact]
	public void Keyvals_map_through_GDK_for_every_layout() => Run(() =>
	{
		Assert.Equal("о", GdkKeyText.FromKeyval(0x6cf));
		Assert.Equal("o", GdkKeyText.FromKeyval('o'));
		Assert.Null(GdkKeyText.FromKeyval(0xff0d));
		Assert.Null(GdkKeyText.FromKeyval(0xffe3));
		var button = new Button { Content = "_оk" };
		var window = Show(Stack(button));
		int clicks = 0;
		button.Click += (_, _) => clicks++;
		Assert.True(Alt(window, 0x6cf));
		Assert.Equal(1, clicks);
		window.Close();
	});

	// A real key event cannot be injected: GtkSharp4 binds no gdk event constructor or test-send helper, and
	// GDK builds key events only inside its backends. So the test asserts the cause instead: the window's own
	// shortcut controllers (GTK's mnemonic matcher) accept no modifier a keyboard can produce.
	[Fact]
	public void GTK_mnemonic_matching_is_switched_off_on_every_window() => Run(() =>
	{
		var window = Show(Stack(new Button { Content = "_Open" }, new CheckBox { Content = "_Check" }));
		var controllers = Window.MnemonicControllers((Gtk.Window)window.Widget);
		Assert.NotEmpty(controllers);
		Assert.All(controllers, c => Assert.Equal(Gdk.ModifierType.HyperMask, c.MnemonicsModifiers));
		window.Close();
	});

	[Fact]
	public void Alt_key_clicks_a_button_checks_a_box_and_opens_a_menu_once() => Run(() =>
	{
		var executed = 0;
		var button = new Button { Content = "_Open", Command = new Relay(() => executed++) };
		var check = new CheckBox { Content = "_Check" };
		var file = new MenuItem { Header = "_File", Items = { new MenuItem { Header = "x" } } };
		var window = Show(Stack(new Menu { Items = { file } }, button, check));
		int clicks = 0, toggles = 0;
		button.Click += (_, _) => clicks++;
		check.Checked += (_, _) => toggles++;
		check.Unchecked += (_, _) => toggles++;
		Assert.True(Alt(window, KeyO));
		Assert.True(Alt(window, 'c'));
		Assert.True(Alt(window, KeyF));
		GtkPump.For(100);
		Assert.Equal(1, clicks);
		Assert.Equal(1, executed);
		Assert.Equal(1, toggles);
		Assert.True(file.IsSubmenuOpen);
		window.Close();
	});

	sealed class Relay : ICommand
	{
		readonly Action _run;
		public Relay(Action run) => _run = run;
#pragma warning disable CS0067
		public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
		public bool CanExecute(object? parameter) => true;
		public void Execute(object? parameter) => _run();
	}
}
