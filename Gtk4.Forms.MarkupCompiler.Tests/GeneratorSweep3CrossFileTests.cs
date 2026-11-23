using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>An unbuildable keyed resource keeps a placeholder when any XAML file of the project references it.</summary>
public class GeneratorSweep3CrossFileTests : Sweep3Base
{
	static string Generate(string xaml, ISet<string> keys)
	{
		string dir = Path.GetDirectoryName(typeof(GeneratorV2TestBase).Assembly.Location)!;
		string pf = Path.Combine(dir, "Gtk4.Forms.PresentationFramework.dll");
		using var gen = new XamlCodeGenerator
		{
			HasCodeBehind = true, CodeBehindSources = new[] { Stub }, ProjectStaticResourceKeys = keys
		};
		gen.SetReferencePaths(new[] { pf, Path.Combine(dir, "Gtk4.Forms.WindowsBase.dll") });
		gen.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(new[] { pf }));
		return gen.Generate(xaml, out _);
	}

	const string Unbuildable = "<BooleanToVisibilityConverter x:Key=\"b\" Foo=\"1\"/>";

	[Fact]
	public void Key_referenced_only_by_another_file_keeps_a_placeholder_entry()
	{
		string code = Generate(Page("<Button/>", Unbuildable), new HashSet<string> { "b" });

		Has(code, "Resources[\"b\"] = ");
	}

	[Fact]
	public void Key_referenced_by_no_file_gets_no_entry()
	{
		string code = Generate(Page("<Button/>", Unbuildable), new HashSet<string> { "other" });

		Lacks(code, "Resources[\"b\"]");
	}

	[Theory]
	[InlineData("<Button Tag=\"{StaticResource k1}\"/>", "k1")]
	[InlineData("<Button Tag=\"{StaticResource ResourceKey=k1, Foo=1}\"/>", "k1")]
	[InlineData("<Button><Button.Tag><StaticResource ResourceKey=\"k1\"/></Button.Tag></Button>", "k1")]
	[InlineData("<Button Tag=\"{StaticResource {x:Type Button}}\"/>", "")]
	public void Collector_finds_attribute_and_element_syntax_references(string xaml, string expected)
	{
		var keys = new HashSet<string>();

		XamlCodeGenerator.CollectStaticResourceKeys(xaml, keys);

		Assert.Equal(expected.Length == 0 ? new string[0] : new[] { expected }, keys.ToArray());
	}
}
