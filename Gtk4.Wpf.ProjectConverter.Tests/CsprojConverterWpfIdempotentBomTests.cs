using System.Text;
using Gtk4.Wpf.ProjectConverter;
using Xunit;

namespace Gtk4.Wpf.ProjectConverter.Tests;

/// <summary>Transform rules: idempotency, BOM preservation, unrelated content preserved.</summary>
public class CsprojConverterWpfIdempotentBomTests
{
	const string FullyConverted =
		"""
		<Project Sdk="Microsoft.NET.Sdk">
		  <PropertyGroup>
		    <TargetFramework>net10.0</TargetFramework>
		    <OutputType>Exe</OutputType>
		  </PropertyGroup>
		</Project>
		""";

	[Fact]
	public void FullyConvertedProjectIsNoOp()
	{
		var result = CsprojConverter.Transform(FullyConverted, out var changed);

		Assert.False(changed);
		Assert.Equal(FullyConverted, result);
	}

	[Fact]
	public void BomIsPreservedOnConvert()
	{
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0-windows</TargetFramework>
			    <UseWPF>true</UseWPF>
			  </PropertyGroup>
			</Project>
			""";

		var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".csproj");
		try
		{
			File.WriteAllText(tmp, input, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
			var before = File.ReadAllBytes(tmp);
			Assert.Equal(0xEF, before[0]);

			CsprojConverter.TryConvertFile(tmp);

			var after = File.ReadAllBytes(tmp);
			Assert.Equal(0xEF, after[0]);
			Assert.Equal(0xBB, after[1]);
			Assert.Equal(0xBF, after[2]);
		}
		finally
		{
			File.Delete(tmp);
		}
	}

	[Fact]
	public void NoBomStaysNoBomOnConvert()
	{
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0-windows</TargetFramework>
			    <UseWPF>true</UseWPF>
			  </PropertyGroup>
			</Project>
			""";

		var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".csproj");
		try
		{
			File.WriteAllText(tmp, input, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			var before = File.ReadAllBytes(tmp);
			Assert.NotEqual(0xEF, before[0]);

			CsprojConverter.TryConvertFile(tmp);

			var after = File.ReadAllBytes(tmp);
			Assert.Equal((byte)'<', after[0]);
		}
		finally
		{
			File.Delete(tmp);
		}
	}

	[Fact]
	public void SurroundingTextIsUnchanged()
	{
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0-windows</TargetFramework>
			    <UseWPF>true</UseWPF>
			    <AssemblyName>MyWpfApp</AssemblyName>
			    <RootNamespace>MyWpfApp</RootNamespace>
			  </PropertyGroup>
			  <ItemGroup>
			    <PackageReference Include="SomeLib" Version="2.0.0" />
			  </ItemGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out _);

		Assert.Contains("<AssemblyName>MyWpfApp</AssemblyName>", result);
		Assert.Contains("<RootNamespace>MyWpfApp</RootNamespace>", result);
		Assert.Contains("""<PackageReference Include="SomeLib" Version="2.0.0" />""", result);
		Assert.Contains("""<Project Sdk="Microsoft.NET.Sdk">""", result);
	}
}
