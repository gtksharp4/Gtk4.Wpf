using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class StartupPageProbe : Page
{
	public StartupPageProbe()
	{
		WindowTitle = "Probe window title";
		Content = new TextBlock { Text = "hello" };
	}
}

public class StartupTitledPageProbe : Page
{
	public StartupTitledPageProbe() => Title = "Probe page title";
}

/// <summary>A Page named by StartupUri is hosted in a NavigationWindow, as WPF does.</summary>
public class StartupPageHostTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	[Fact]
	public void Page_startup_uri_is_shown_in_a_NavigationWindow() => Run(() =>
	{
		var app = new RecordingApp { StartupUri = new Uri("StartupPageProbe.xaml", UriKind.Relative) };
		Window? main = null;
		Visibility vis = Visibility.Collapsed;
		app.RunLoopOverride = a => { main = a.MainWindow; vis = main!.Visibility; };

		app.RunCore(new string[0]);

		var nav = Assert.IsType<NavigationWindow>(main);
		Assert.IsType<StartupPageProbe>(nav.Content);
		Assert.Same(nav.Content, nav.NavigationService.Content);
		Assert.Equal("Probe window title", nav.Title);
		Assert.Equal(Visibility.Visible, vis);
	});

	[Fact]
	public void Page_Title_is_the_window_title_when_WindowTitle_is_unset() => Run(() =>
	{
		var app = new RecordingApp { StartupUri = new Uri("StartupTitledPageProbe.xaml", UriKind.Relative) };
		Window? main = null;
		app.RunLoopOverride = a => main = a.MainWindow;

		app.RunCore(new string[0]);

		Assert.Equal("Probe page title", main!.Title);
	});

	[Fact]
	public void Registered_factory_creates_the_startup_page_in_a_NavigationWindow() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("Start.xaml", () => new StartupPageProbe());
		var app = new RecordingApp();

		Window? host = app.CreateWindowFromStartupUri(new Uri("Start.xaml", UriKind.Relative));

		var nav = Assert.IsType<NavigationWindow>(host);
		Assert.IsType<StartupPageProbe>(nav.Content);
	});

	[Fact]
	public void Registry_matches_pack_uris_and_leaf_names() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("Views/Other.xaml", () => new StartupPageProbe());
		Assert.IsType<StartupPageProbe>(
			GtkFormsXamlRegistry.TryCreate(new Uri("pack://application/Views/Other.xaml")));
		Assert.IsType<StartupPageProbe>(GtkFormsXamlRegistry.TryCreate(new Uri("Other.xaml", UriKind.Relative)));
		Assert.Null(GtkFormsXamlRegistry.TryCreate(new Uri("Missing.xaml", UriKind.Relative)));
	});
}
