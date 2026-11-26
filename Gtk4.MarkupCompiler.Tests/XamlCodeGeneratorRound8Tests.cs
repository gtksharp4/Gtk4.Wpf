using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Round-8 compiler pinning tests: child &lt;Binding&gt; Converter/StringFormat emission (R9).
/// Each test fails without the matching compiler change.
/// </summary>
public class XamlCodeGeneratorRound8Tests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body = "") =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	{body}
</Window>";

	// ── R9: child <Binding> Converter emission ────────────────────────────────

	[Fact]
	public void R9_MultiBinding_child_Converter_emitted()
	{
		string xaml = Window(@"
<TextBlock>
  <TextBlock.Text>
    <MultiBinding Converter=""{StaticResource mc}"">
      <Binding Path=""First"" Converter=""{StaticResource myConv}"" />
      <Binding Path=""Last"" />
    </MultiBinding>
  </TextBlock.Text>
</TextBlock>");
		string code = _gen.Generate(xaml, out _);
		// The child Binding must emit its Converter.
		Assert.Contains("StaticResourceScope.Find(\"myConv\", this", code);
	}

	[Fact]
	public void R9_MultiBinding_child_StringFormat_emitted()
	{
		string xaml = Window(@"
<TextBlock>
  <TextBlock.Text>
    <MultiBinding Converter=""{StaticResource mc}"">
      <Binding Path=""First"" StringFormat=""Hello {0}"" />
      <Binding Path=""Last"" />
    </MultiBinding>
  </TextBlock.Text>
</TextBlock>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("StringFormat = \"Hello {0}\"", code);
	}

	[Fact]
	public void R9_MultiBinding_child_without_Converter_no_spurious_FindResource()
	{
		// A child Binding without Converter must NOT emit a FindResource call.
		string xaml = Window(@"
<TextBlock>
  <TextBlock.Text>
    <MultiBinding Converter=""{StaticResource mc}"">
      <Binding Path=""Name"" />
    </MultiBinding>
  </TextBlock.Text>
</TextBlock>");
		string code = _gen.Generate(xaml, out _);
		// Only one FindResource for the MultiBinding's own Converter="mc".
		int count = 0;
		int idx = 0;
		while ((idx = code.IndexOf("StaticResourceScope.Find(", idx, System.StringComparison.Ordinal)) >= 0)
		{ count++; idx++; }
		Assert.Equal(1, count);
	}
}