using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Background/Foreground/Font* attributes compile to DP <c>SetValue</c> calls whose value goes through
/// <c>XamlValueConverter</c>; nothing here needs a markup-compiler change, the DPs now exist.
/// </summary>
public class XamlCodeGeneratorAppearanceTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body, string attrs = "") =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	{attrs}>
	{body}
</Window>";

	[Fact]
	public void Button_Background_FontSize_FontWeight_emit_SetValue_with_converter()
	{
		string code = _gen.Generate(
			Window("<Button Background=\"Red\" FontSize=\"14\" FontWeight=\"Bold\" />"), out _);
		Assert.Contains("BackgroundProperty", code);
		Assert.Contains("FontSizeProperty", code);
		Assert.Contains("FontWeightProperty", code);
		Assert.Contains("SetValue(", code);
		Assert.Contains("XamlValueConverter.Convert(", code);
		Assert.Contains("\"Red\"", code);
		Assert.Contains("\"14\"", code);
		Assert.Contains("\"Bold\"", code);
	}

	[Fact]
	public void Window_Foreground_and_FontSize_emit_dp_assignments()
	{
		string code = _gen.Generate(
			Window("<TextBlock />", "FontSize=\"16pt\" Foreground=\"Red\""), out _);
		Assert.Contains("FontSizeProperty", code);
		Assert.Contains("ForegroundProperty", code);
		Assert.Contains("\"16pt\"", code);
	}
}
