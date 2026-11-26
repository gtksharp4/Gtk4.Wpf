using System;
using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class WindowIsActiveTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	[Fact]
	public void IsActiveProperty_is_a_read_only_false_default_bool()
	{
		var p = Window.IsActiveProperty;
		Assert.Equal("IsActive", p.Name);
		Assert.Equal(typeof(bool), p.PropertyType);
		Assert.Equal(typeof(Window), p.OwnerType);
		Assert.True(p.ReadOnly);
		Assert.Equal(false, p.DefaultMetadata.DefaultValue);
	}

	[Fact]
	public void IsActive_is_set_before_Activated_and_cleared_before_Deactivated() => Run(() =>
	{
		var window = new Window();
		var seen = new List<string>();
		window.Activated += (_, _) => seen.Add("activated:" + window.IsActive);
		window.Deactivated += (_, _) => seen.Add("deactivated:" + window.IsActive);
		Assert.False(window.IsActive);

		window.NotifyActivation(true);
		Assert.True(window.IsActive);
		window.NotifyActivation(false);
		Assert.False(window.IsActive);
		Assert.Equal(new[] { "activated:True", "deactivated:False" }, seen);
	});

	[Fact]
	public void A_repeated_notification_raises_nothing() => Run(() =>
	{
		var window = new Window();
		int activated = 0, deactivated = 0;
		window.Activated += (_, _) => activated++;
		window.Deactivated += (_, _) => deactivated++;
		window.NotifyActivation(false);
		window.NotifyActivation(true);
		window.NotifyActivation(true);
		window.NotifyActivation(false);
		window.NotifyActivation(false);
		Assert.Equal(1, activated);
		Assert.Equal(1, deactivated);
	});

	[Fact]
	public void IsActive_cannot_be_set_through_SetValue_on_the_public_property() => Run(() =>
	{
		var window = new Window();
		Assert.Throws<InvalidOperationException>(() => window.SetValue(Window.IsActiveProperty, true));
	});

	[Fact]
	public void IsActive_checks_thread_affinity() => Run(() =>
	{
		var window = new Window();
		var original = System.Windows.Threading.Dispatcher.IsOnDispatcherThread;
		System.Windows.Threading.Dispatcher.IsOnDispatcherThread = () => false;
		try { Assert.Throws<InvalidOperationException>(() => window.IsActive); }
		finally { System.Windows.Threading.Dispatcher.IsOnDispatcherThread = original; }
	});

	[Fact]
	public void IsActive_follows_the_real_gtk_activation() => Run(() =>
	{
		var window = new Window { Width = 200, Height = 120 };
		window.Show();
		bool active = Lifecycle.GtkPump.Until(() => window.IsActive, 3000);
		Assert.SkipUnless(active, "the window manager did not activate the test window");
		Assert.True(window.IsActive);
		window.Close();
	});
}
