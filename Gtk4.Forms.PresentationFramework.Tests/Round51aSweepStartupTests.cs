using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

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
	public void Canvas_root_registered_under_another_file_name_is_hosted_with_its_thumb() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("SweepMain.xaml", () =>
		{
			var canvas = new Canvas();
			canvas.Children.Add(new System.Windows.Controls.Primitives.Thumb { Width = 20, Height = 20 });
			return canvas;
		});
		var app = new RecordingApp { StartupUri = new Uri("SweepMain.xaml", UriKind.Relative) };
		Window? main = null;
		app.RunLoopOverride = a => main = a.MainWindow;

		app.RunCore(new string[0]);

		var nav = Assert.IsType<NavigationWindow>(main);
		var canvas = Assert.IsType<Canvas>(nav.Content);
		Assert.IsType<System.Windows.Controls.Primitives.Thumb>(Assert.Single(canvas.Children));
	});
}
