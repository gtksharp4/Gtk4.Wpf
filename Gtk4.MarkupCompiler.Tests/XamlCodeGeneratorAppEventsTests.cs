using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// App.xaml lifecycle events (Round 38) are wired with <c>+=</c>; ShutdownMode is an enum assignment.
/// </summary>
public class XamlCodeGeneratorAppEventsTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	const string AppOpen =
		@"<Application x:Class=""MyApp.App""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""";

	[Theory]
	[InlineData("Startup", "OnAppStartup")]
	[InlineData("Exit", "OnAppExit")]
	[InlineData("DispatcherUnhandledException", "OnErr")]
	[InlineData("Activated", "OnAct")]
	[InlineData("Deactivated", "OnDeact")]
	[InlineData("SessionEnding", "OnEnding")]
	public void App_root_event_is_wired_with_plus_equals(string evt, string handler)
	{
		string xaml = AppOpen + " " + evt + "=\"" + handler + "\">\n</Application>";

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("this." + evt + " += this." + handler + ";", code);
	}

	[Fact]
	public void App_root_with_all_attributes_emits_events_and_ShutdownMode()
	{
		string xaml = AppOpen +
			" Startup=\"OnAppStartup\" Exit=\"OnAppExit\" DispatcherUnhandledException=\"OnErr\"" +
			" ShutdownMode=\"OnMainWindowClose\">\n</Application>";

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("this.Startup += this.OnAppStartup;", code);
		Assert.Contains("this.Exit += this.OnAppExit;", code);
		Assert.Contains("this.DispatcherUnhandledException += this.OnErr;", code);
		Assert.Contains("ShutdownMode", code);
		Assert.Contains("OnMainWindowClose", code);
		Assert.DoesNotContain("ShutdownMode = \"", code);
	}
}
