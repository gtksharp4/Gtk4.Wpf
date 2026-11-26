using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Element-lifecycle and size events (Round 36) are wired with <c>+=</c> by the codegen.
/// With no reference assemblies the fallback event-name set decides, so it must list them.
/// </summary>
public class XamlCodeGeneratorLifecycleEventTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	const string RootOpen =
		@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""";

	[Theory]
	[InlineData("Loaded", "OnLoaded")]
	[InlineData("Unloaded", "OnUnloaded")]
	[InlineData("Initialized", "OnInitializedHandler")]
	[InlineData("SizeChanged", "OnSizeChanged")]
	[InlineData("ContentRendered", "OnContentRendered")]
	[InlineData("SourceInitialized", "OnSourceInitialized")]
	public void Root_lifecycle_event_is_wired_with_plus_equals(string evt, string handler)
	{
		string xaml = RootOpen + " " + evt + "=\"" + handler + "\">\n</Window>";

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("this." + evt + " += this." + handler + ";", code);
		Assert.DoesNotContain(evt + " = \"", code);
	}

	[Fact]
	public void Child_SizeChanged_and_Loaded_are_wired_on_named_element()
	{
		string xaml = RootOpen + @">
	<Grid x:Name=""Root"" SizeChanged=""Root_SizeChanged"" Loaded=""Root_Loaded"" />
</Window>";

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".SizeChanged += this.Root_SizeChanged;", code);
		Assert.Contains(".Loaded += this.Root_Loaded;", code);
	}

	[Theory]
	[InlineData("SizeChanged")]
	[InlineData("ContentRendered")]
	[InlineData("SourceInitialized")]
	[InlineData("MouseWheel")]
	[InlineData("PreviewMouseWheel")]
	[InlineData("MouseRightButtonDown")]
	[InlineData("MouseRightButtonUp")]
	[InlineData("PreviewMouseLeftButtonDown")]
	[InlineData("PreviewMouseLeftButtonUp")]
	[InlineData("GotMouseCapture")]
	[InlineData("LostMouseCapture")]
	public void Fallback_event_set_contains_new_names(string name)
	{
		Assert.True(DpSymbolResolver.FallbackEventHeuristic(name));
	}

	[Fact]
	public void Fallback_event_set_still_rejects_properties()
	{
		Assert.False(DpSymbolResolver.FallbackEventHeuristic("Width"));
		Assert.False(DpSymbolResolver.FallbackEventHeuristic("ActualWidth"));
	}
}
