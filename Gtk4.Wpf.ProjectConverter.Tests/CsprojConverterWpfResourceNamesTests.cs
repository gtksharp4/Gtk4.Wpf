using Gtk4.Wpf.ProjectConverter;
using Xunit;

namespace Gtk4.Wpf.ProjectConverter.Tests;

/// <summary>
/// Transform rules: embedded resources are left untouched, so the manifest resource names the
/// SDK derives from them (e.g. <c>LocalizationResources.Data.stringtable...</c>) stay the WPF ones.
/// </summary>
public class CsprojConverterWpfResourceNamesTests
{
	[Fact]
	public void EmbeddedResourceItemsAreByteIdenticalAfterConversion()
	{
		var items =
			"""
			  <ItemGroup>
			    <EmbeddedResource Include="Properties\Resources.resx">
			      <Generator>ResXFileCodeGenerator</Generator>
			      <LastGenOutput>Resources.Designer.cs</LastGenOutput>
			    </EmbeddedResource>
			    <EmbeddedResource Include="Data\stringtable.en-us.restext" />
			    <EmbeddedResource Include="Strings.resx" LogicalName="App.Strings.resources" />
			    <EmbeddedResource Include="Other.resx" ManifestResourceName="X.Other" />
			    <EmbeddedResource Update="Dep.resx" DependentUpon="Dep.cs" />
			  </ItemGroup>
			""";
		var input =
			"<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
			"  <PropertyGroup>\n" +
			"    <TargetFramework>net10.0-windows</TargetFramework>\n" +
			"    <UseWPF>true</UseWPF>\n" +
			"    <RootNamespace>LocalizationResources</RootNamespace>\n" +
			"    <UICulture>en-US</UICulture>\n" +
			"  </PropertyGroup>\n" + items + "\n</Project>";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains(items, result);
		Assert.Contains("<RootNamespace>LocalizationResources</RootNamespace>", result);
		Assert.Contains("<UICulture>en-US</UICulture>", result);
	}
}
