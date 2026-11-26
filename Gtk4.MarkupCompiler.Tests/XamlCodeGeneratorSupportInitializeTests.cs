using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// WPF's XAML loader brackets the root with <c>ISupportInitialize.BeginInit</c>/<c>EndInit</c>,
/// so the root's <c>Initialized</c> fires during <c>InitializeComponent</c> (after its
/// children's). The generated code reproduces that for every non-Application root.
/// </summary>
public class XamlCodeGeneratorSupportInitializeTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	const string BeginInit =
		"if ((object)this is System.ComponentModel.ISupportInitialize __initBeginInit) __initBeginInit.BeginInit();";
	const string EndInit =
		"if ((object)this is System.ComponentModel.ISupportInitialize __initEndInit) __initEndInit.EndInit();";

	const string WindowXaml = @"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" Title=""T"">
	<Grid x:Name=""Root"" />
</Window>";

	[Fact]
	public void Window_root_is_bracketed_by_BeginInit_and_EndInit()
	{
		string code = _gen.Generate(WindowXaml, out _);

		int guard = code.IndexOf("_contentLoaded = true;", System.StringComparison.Ordinal);
		int begin = code.IndexOf(BeginInit, System.StringComparison.Ordinal);
		int title = code.IndexOf("this.Title = \"T\";", System.StringComparison.Ordinal);
		int child = code.IndexOf("this.Content = ", System.StringComparison.Ordinal);
		int end = code.IndexOf(EndInit, System.StringComparison.Ordinal);

		Assert.True(guard >= 0 && begin > guard, "BeginInit follows the idempotent guard");
		Assert.True(title > begin && child > begin, "properties and children are set inside the bracket");
		Assert.True(end > title && end > child, "EndInit runs after properties and children");
	}

	[Fact]
	public void UserControl_root_is_bracketed_too()
	{
		string xaml = @"<UserControl x:Class=""MyApp.Part""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" />";

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(BeginInit, code);
		Assert.Contains(EndInit, code);
	}

	[Fact]
	public void Application_root_is_not_bracketed()
	{
		string xaml = @"<Application x:Class=""MyApp.App""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" />";

		string code = _gen.Generate(xaml, out _);

		Assert.DoesNotContain("ISupportInitialize", code);
	}
}
