using System.Text;
using Gtk4.Forms.ProjectConverter;
using Xunit;

namespace Gtk4.Forms.ProjectConverter.Tests;

/// <summary>Transform rules: idempotency, BOM preservation, minimal diff.</summary>
public class CsprojConverterIdempotentBomTests
{
	const string FullyConverted =
		"""
		<Project Sdk="Microsoft.NET.Sdk">
		  <PropertyGroup>
		    <TargetFramework>net10.0</TargetFramework>
		    <OutputType>Exe</OutputType>
		    <StartupObject>Gtk4.Forms.Host.GtkEntryPoint</StartupObject>
		  </PropertyGroup>
		</Project>
		""";

	// ── idempotent no-op ───────────────────────────────────────────────────────

	[Fact]
	public void FullyConvertedProjectIsNoOp()
	{
		var result = CsprojConverter.Transform(FullyConverted, out var changed);

		Assert.False(changed);
		Assert.Equal(FullyConverted, result);
	}

	// ── BOM preservation ──────────────────────────────────────────────────────

	[Fact]
	public void BomIsPreservedOnConvert()
	{
		var input =
			"""
			<Project>
			  <PropertyGroup>
			    <TargetFramework>net8.0-android</TargetFramework>
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
			<Project>
			  <PropertyGroup>
			    <TargetFramework>net8.0-android</TargetFramework>
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

	// ── minimal diff ──────────────────────────────────────────────────────────

	[Fact]
	public void SurroundingTextIsUnchanged()
	{
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>netstandard2.0</TargetFramework>
			    <AssemblyName>MyApp</AssemblyName>
			  </PropertyGroup>
			  <ItemGroup>
			    <PackageReference Include="SomeLib" Version="2.0.0" />
			  </ItemGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out _);

		Assert.Contains("<AssemblyName>MyApp</AssemblyName>", result);
		Assert.Contains("""<PackageReference Include="SomeLib" Version="2.0.0" />""", result);
		Assert.Contains("""<Project Sdk="Microsoft.NET.Sdk">""", result);
	}
}
