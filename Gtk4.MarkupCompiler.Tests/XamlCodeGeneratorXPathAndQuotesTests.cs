using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Quoted binding arguments are unquoted; XPath is emitted in both binding forms.</summary>
public class XamlCodeGeneratorXPathAndQuotesTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body) =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
	{body}
</Window>";

	string Gen(string body) => _gen.Generate(Window(body), out _);

	string GenAttr(string binding) => Gen($@"<TextBlock Text=""{binding}"" />");

	[Fact]
	public void A_quoted_StringFormat_with_the_brace_escape_is_emitted_without_quotes()
	{
		string code = GenAttr("{Binding Name, StringFormat='{}{0} Page'}");
		Assert.Contains(@"StringFormat = ""{0} Page""", code);
		Assert.DoesNotContain(@"""'", code);
	}

	[Fact]
	public void Quoted_FallbackValue_TargetNullValue_and_ConverterParameter_are_unquoted()
	{
		string code = GenAttr("{Binding Name, FallbackValue='a, b', TargetNullValue='none', ConverterParameter='p q'}");
		Assert.Contains(@"FallbackValue = ""a, b""", code);
		Assert.Contains(@"TargetNullValue = ""none""", code);
		Assert.Contains(@"ConverterParameter = ""p q""", code);
	}

	[Fact]
	public void A_bare_StringFormat_is_unchanged()
		=> Assert.Contains(@"StringFormat = ""N2""", GenAttr("{Binding Age, StringFormat=N2}"));

	[Fact]
	public void The_IconsPage_StringFormat_is_emitted_whole()
	{
		string code = GenAttr("{Binding Glyph, StringFormat='&lt;TextBlock FontFamily=&#x22;{{StaticResource Sym}}&#x22;"
			+ " Text=&#x22;{0}&#x22;/&gt;'}");
		Assert.Contains(@"StringFormat = ""<TextBlock FontFamily=\""{{StaticResource Sym}}\"" Text=\""{0}\""/>""", code);
	}

	[Fact]
	public void XPath_in_the_markup_extension_is_emitted()
		=> Assert.Contains(@"XPath = ""@Name""", GenAttr("{Binding XPath=@Name}"));

	[Fact]
	public void XPath_with_a_StaticResource_Source_is_emitted()
	{
		string code = GenAttr("{Binding Source={StaticResource X}, XPath=Item}");
		Assert.Contains(@"XPath = ""Item""", code);
		Assert.Contains("Source = ", code);
	}

	[Fact]
	public void A_quoted_XPath_keeps_commas_and_inner_quotes()
		=> Assert.Contains(@"XPath = ""Item[@a=""""x, y""""]""".Replace("\"\"", "\\\""),
			GenAttr("{Binding XPath='Item[@a=&quot;x, y&quot;]'}"));

	[Fact]
	public void XPath_on_a_Binding_element_is_emitted_intact()
	{
		string code = Gen(@"<ListBox><ListBox.ItemsSource><Binding Source=""{StaticResource InventoryData}""
			XPath=""*[@Stock='out'] | *[@Number&gt;=8 or @Number=3]""/></ListBox.ItemsSource></ListBox>");
		Assert.Contains(@"XPath = ""*[@Stock='out'] | *[@Number>=8 or @Number=3]""", code);
	}
}
