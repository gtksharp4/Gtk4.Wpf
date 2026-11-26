using System;
using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using System.Windows.Markup;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Panel root with an x:Class and no constructor call to InitializeComponent, as in ContextMenuOpening.</summary>
public class SweepPane : StackPanel
{
	bool _contentLoaded;

	public void InitializeComponent()
	{
		if (_contentLoaded) return;
		_contentLoaded = true;
		Children.Add(new TextBlock { Text = "loaded" });
	}
}

/// <summary>Registry root whose factory only calls new T(): the child comes from InitializeComponent.</summary>
public class RegisteredPane : Canvas
{
	bool _contentLoaded;

	public void InitializeComponent()
	{
		if (_contentLoaded) return;
		_contentLoaded = true;
		Children.Add(new System.Windows.Controls.Primitives.Thumb { Width = 20, Height = 20 });
	}
}

/// <summary>Round 51a: a StartupUri that names a non-Window, non-Page root shows a window.</summary>
public class Round51aSweepStartupTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	[Fact]
	public void Panel_root_startup_uri_is_hosted_and_initialised() => Run(() =>
	{
		var app = new RecordingApp { StartupUri = new Uri("SweepPane.xaml", UriKind.Relative) };
		Window? main = null;
		app.RunLoopOverride = a => main = a.MainWindow;

		app.RunCore(new string[0]);

		var nav = Assert.IsType<NavigationWindow>(main);
		var pane = Assert.IsType<SweepPane>(nav.Content);
		Assert.Single(pane.Children);
	});

	[Fact]
	public void Registered_root_without_a_constructor_call_is_initialised_by_the_host() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("RegisteredPaneHost.xaml", () => new RegisteredPane());
		var app = new RecordingApp();

		var nav = Assert.IsType<NavigationWindow>(
			app.CreateWindowFromStartupUri(new Uri("RegisteredPaneHost.xaml", UriKind.Relative)));

		Assert.Single(Assert.IsType<RegisteredPane>(nav.Content).Children);
		nav.Show();
		Assert.True(GtkPump.Until(() => nav.IsVisible));
		Assert.Single(Assert.IsType<RegisteredPane>(nav.Content).Children);
		nav.Close();
	});
}
