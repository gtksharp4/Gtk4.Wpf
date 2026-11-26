using System.Windows.Automation;
using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Samples;

/// <summary>Gtk4.WpfSample, started from its own <c>App</c> type: StartupUri, binding and Loaded run.</summary>
public class WpfSampleTests : WpfUiTest<WpfSample.App>
{
	[WpfUiFact]
	public void StartupUri_shows_the_main_window_after_its_Loaded_handler_ran()
	{
		Assert.Equal("WPF on GTK 4 (loaded)", App.MainWindow.Current.Name);
		Assert.Equal(ControlType.Window, App.MainWindow.Current.ControlType);
	}

	[WpfUiFact]
	public void The_binding_fills_the_greeting()
	{
		var greeting = App.Find(By.AutomationId("greeting"));
		Assert.Equal("Hello from unmodified WPF XAML, rendered on GTK 4", greeting.Current.Name);
	}

	[WpfUiFact]
	public void The_button_is_found_by_its_content_and_is_enabled()
	{
		var button = App.Find(By.Name("Click me"));
		Assert.Equal(ControlType.Button, button.Current.ControlType);
		Assert.True(button.Current.IsEnabled);
	}

	[WpfUiFact]
	public void A_missing_control_reports_the_tree_of_the_sample()
	{
		var error = Assert.Throws<ElementNotFoundException>(() => App.Find(By.Name("Nope")));
		Assert.Contains("Button name=\"Click me\"", error.Message);
	}
}
