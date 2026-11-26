using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>TextElement.* attached text properties need the owner namespace and a typed setter cast.</summary>
public class XamlCodeGeneratorTextElementAttachedTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body) =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	{body}
</Window>";

	[Fact]
	public void TextElement_FontSize_and_Foreground_emit_qualified_typed_setters()
	{
		string code = _gen.Generate(Window("<Grid TextElement.FontSize=\"20\" TextElement.Foreground=\"Red\"/>"), out _);
		Assert.Contains("System.Windows.Documents.TextElement.SetFontSize(", code);
		Assert.Contains("(double)System.Windows.Markup.XamlValueConverter.Convert(", code);
		Assert.Contains("System.Windows.Documents.TextElement.SetForeground(", code);
		Assert.Contains("(System.Windows.Media.Brush)System.Windows.Markup.XamlValueConverter.Convert(", code);
	}

	[Theory]
	[InlineData("FontFamily", "System.Windows.Media.FontFamily")]
	[InlineData("FontWeight", "System.Windows.FontWeight")]
	[InlineData("FontStyle", "System.Windows.FontStyle")]
	[InlineData("FontStretch", "System.Windows.FontStretch")]
	public void TextElement_font_properties_cast_to_their_type(string prop, string type)
	{
		string code = _gen.Generate(Window($"<Grid TextElement.{prop}=\"x\"/>"), out _);
		Assert.Contains($"({type})System.Windows.Markup.XamlValueConverter.Convert(", code);
	}
}
