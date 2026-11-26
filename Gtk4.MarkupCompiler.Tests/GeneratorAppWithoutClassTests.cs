using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// An ApplicationDefinition without x:Class still gets a generated class and Main (Concepts, CS5001).
/// </summary>
public class GeneratorAppWithoutClassTests : GeneratorV2TestBase
{
	const string App = "<Application " + Head + " StartupUri=\"D.xaml\"><Application.Resources>"
		+ "<Style x:Key=\"S\"><Setter Property=\"TextBlock.FontSize\" Value=\"8\"/></Style>"
		+ "</Application.Resources></Application>";

	[Fact]
	public void ApplicationDefinition_WithoutClass_EmitsMain()
	{
		string code = Gen(App, out string cls, hasCodeBehind: false, xamlPath: "App.xaml", rootNamespace: "N",
			itemType: "ApplicationDefinition");

		Assert.Equal("N.App", cls);
		Assert.Contains("static void Main", code);
		Assert.DoesNotContain("GtkFormsXamlRegistration", code);
		AssertCompiles(code);
	}

	[Fact]
	public void PageItem_WithApplicationRoot_StillGetsNoClass()
	{
		Assert.Null(Gen(App, out _, hasCodeBehind: false, xamlPath: "App.xaml", rootNamespace: "N",
			itemType: "Page"));
	}

	[Fact]
	public void HandWrittenAppClass_KeepsItsOwnMain_AndTheXamlClassIsInternal()
	{
		string code = Gen(App, out string cls, xamlPath: "App.xaml", rootNamespace: "N",
			itemType: "ApplicationDefinition",
			sources: new[] { "namespace N { public class App : System.Windows.Application { } }" });

		Assert.StartsWith("N.__Xaml_", cls);
		Assert.Contains("static void Main", code);
		AssertCompiles(code, "namespace N { public class App : System.Windows.Application { } }");
	}
}
