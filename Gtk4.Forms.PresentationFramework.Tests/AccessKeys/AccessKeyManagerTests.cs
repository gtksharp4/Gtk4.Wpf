using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.AccessKeys;

public class AccessKeyManagerTests : AccessKeyTestBase
{
	[Fact]
	public void Register_unregister_and_IsKeyRegistered_are_case_insensitive() => Run(() =>
	{
		var button = new Button { Content = "x" };
		var window = Show(button);
		Assert.False(AccessKeyManager.IsKeyRegistered(window, "q"));
		AccessKeyManager.Register("q", button);
		Assert.True(AccessKeyManager.IsKeyRegistered(window, "Q"));
		Assert.True(AccessKeyManager.IsKeyRegistered(window, "q"));
		Assert.False(AccessKeyManager.IsKeyRegistered(new object(), "q"));
		AccessKeyManager.Unregister("Q", button);
		Assert.False(AccessKeyManager.IsKeyRegistered(window, "q"));
		window.Close();
	});

	[Fact]
	public void Invalid_keys_throw_like_WPF() => Run(() =>
	{
		var button = new Button();
		Assert.Throws<ArgumentException>(() => AccessKeyManager.Register("ab", button));
		Assert.Throws<ArgumentException>(() => AccessKeyManager.Register("", button));
		Assert.Throws<ArgumentNullException>(() => AccessKeyManager.Register(null!, button));
		Assert.Throws<ArgumentNullException>(() => AccessKeyManager.Register("a", null!));
		Assert.Throws<ArgumentException>(() => AccessKeyManager.Unregister("ab", button));
		Assert.Throws<ArgumentException>(() => AccessKeyManager.IsKeyRegistered(new object(), "ab"));
		AccessKeyManager.Register("\U0001F600", button);
		AccessKeyManager.Unregister("\U0001F600", button);
	});

	[Fact]
	public void Dead_references_are_purged() => Run(() =>
	{
		static void Register() => AccessKeyManager.Register("ñ", new Button());
		Register();
		for (int i = 0; i < 3; i++)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
		Assert.Equal(0, AccessKeyManager.RegisteredCount("ñ"));
	});

	[Fact]
	public void Disabled_and_hidden_elements_are_skipped() => Run(() =>
	{
		var enabled = new Button { Content = "_Go" };
		var disabled = new Button { Content = "_Go", IsEnabled = false };
		var hidden = new Button { Content = "_Go", Visibility = Visibility.Collapsed };
		var panel = new StackPanel();
		panel.Children.Add(disabled);
		panel.Children.Add(hidden);
		panel.Children.Add(enabled);
		var window = Show(panel);
		int clicks = 0;
		enabled.Click += (_, _) => clicks++;
		disabled.Click += (_, _) => clicks += 100;
		hidden.Click += (_, _) => clicks += 1000;
		Assert.True(AccessKeyManager.ProcessKey(window, "G", false));
		Assert.Equal(1, clicks);
		enabled.IsEnabled = false;
		Assert.False(AccessKeyManager.IsKeyRegistered(window, "G"));
		Assert.False(AccessKeyManager.ProcessKey(window, "G", false));
		window.Close();
	});

	[Fact]
	public void Two_buttons_sharing_a_key_cycle_focus_and_neither_clicks() => Run(() =>
	{
		var one = new Button { Content = "_Go" };
		var two = new Button { Content = "_Go" };
		var panel = new StackPanel();
		panel.Children.Add(one);
		panel.Children.Add(two);
		var window = Show(panel);
		int clicks = 0;
		one.Click += (_, _) => clicks++;
		two.Click += (_, _) => clicks++;
		var seen = new System.Collections.Generic.List<Button>();
		for (int i = 0; i < 4; i++)
		{
			Assert.True(AccessKeyManager.ProcessKey(window, "g", false));
			GtkPump.For(50);
			Assert.True(one.IsKeyboardFocused ^ two.IsKeyboardFocused);
			seen.Add(one.IsKeyboardFocused ? one : two);
		}
		Assert.NotSame(seen[0], seen[1]);
		Assert.Same(seen[0], seen[2]);
		Assert.Same(seen[1], seen[3]);
		Assert.Equal(0, clicks);
		window.Close();
	});

	[Fact]
	public void AccessKeyPressed_handlers_can_name_a_target() => Run(() =>
	{
		var host = new Border();
		var target = new Button { Content = "t" };
		var panel = new StackPanel();
		panel.Children.Add(host);
		panel.Children.Add(target);
		var window = Show(panel);
		int clicks = 0;
		target.Click += (_, _) => clicks++;
		AccessKeyManager.AddAccessKeyPressedHandler(host, (_, e) => e.Target = target);
		AccessKeyManager.Register("z", host);
		Assert.True(AccessKeyManager.ProcessKey(window, "Z", false));
		Assert.Equal(1, clicks);
		AccessKeyManager.Unregister("z", host);
		window.Close();
	});
}
