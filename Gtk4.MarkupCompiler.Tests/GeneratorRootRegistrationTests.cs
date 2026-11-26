using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Each x:Class root is registered under its XAML path, once per file.</summary>
public class GeneratorRootRegistrationTests : GeneratorV2TestBase
{
	const string CanvasRoot = "<Canvas " + Head + " x:Class=\"A.Pane\"><Button x:Name=\"b\"/></Canvas>";

	[Fact]
	public void NonWindowRoot_IsRegisteredUnderItsPath()
	{
		string code = Gen(CanvasRoot, out _, hasCodeBehind: false, xamlPath: "Views/MainWindow.xaml");

		Assert.Contains("GtkFormsXamlRegistration(\"Views/MainWindow.xaml\", typeof(global::A.Pane))", code);
		Assert.Equal(1, Count(code, "GtkFormsXamlRegistration("));
	}

	[Fact]
	public void WindowRootWithCodeBehind_IsRegistered()
	{
		string code = Gen("<Window " + Head + " x:Class=\"A.W\"/>", out _, xamlPath: "W.xaml",
			sources: new[] { "namespace A { public partial class W { public W() { InitializeComponent(); } } }" });

		Assert.Contains("GtkFormsXamlRegistration(\"W.xaml\", typeof(global::A.W))", code);
	}

	[Fact]
	public void ClassWithoutPublicDefaultConstructor_IsNotRegistered()
	{
		string code = Gen("<Window " + Head + " x:Class=\"A.W\"/>", out _, xamlPath: "W.xaml",
			sources: new[] { "namespace A { public partial class W { public W(int x) { } } }" });

		Assert.DoesNotContain("GtkFormsXamlRegistration", code);
	}

	[Fact]
	public void ApplicationRoot_IsNotRegistered()
	{
		string code = Gen("<Application " + Head + " x:Class=\"A.App\"/>", out _, hasCodeBehind: false, xamlPath: "App.xaml");

		Assert.DoesNotContain("GtkFormsXamlRegistration", code);
	}

	[Fact]
	public void RegistrationCompiles()
	{
		string code = Gen(CanvasRoot, out _, hasCodeBehind: false, xamlPath: "MainWindow.xaml");

		AssertCompiles(code);
	}

	static int Count(string text, string part)
	{
		int n = 0;
		for (int i = text.IndexOf(part, System.StringComparison.Ordinal); i >= 0;
			i = text.IndexOf(part, i + 1, System.StringComparison.Ordinal)) n++;
		return n;
	}
}
