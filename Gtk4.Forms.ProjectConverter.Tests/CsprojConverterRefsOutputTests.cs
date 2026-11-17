using System.Text;
using Gtk4.Forms.ProjectConverter;
using Xunit;

namespace Gtk4.Forms.ProjectConverter.Tests;

/// <summary>Transform rules: PackageReference removal, OutputType/StartupObject insertion, BOM and minimal-diff.</summary>
public class CsprojConverterRefsOutputTests
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

	// ── Gtk4.Forms PackageReference removal ─────────────────────────────────

	[Fact]
	public void RemovesXamarinFormsSelfClosed()
	{
		var input =
			"""
			<Project>
			  <ItemGroup>
			    <PackageReference Include="Gtk4.Forms" Version="1.0.0" />
			    <PackageReference Include="SomeOtherLib" Version="1.0.0" />
			  </ItemGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.DoesNotContain("Gtk4.Forms", result);
		Assert.Contains("SomeOtherLib", result);
	}

	[Fact]
	public void RemovesXamarinFormsWithBody()
	{
		var input =
			"""
			<Project>
			  <ItemGroup>
			    <PackageReference Include="Gtk4.Forms" Version="1.0.0">
			      <PrivateAssets>all</PrivateAssets>
			    </PackageReference>
			    <PackageReference Include="SomeOtherLib" Version="1.0.0" />
			  </ItemGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.DoesNotContain("Gtk4.Forms", result);
		Assert.Contains("SomeOtherLib", result);
	}

	[Fact]
	public void RemovesXamarinFormsMaps()
	{
		var input =
			"""
			<Project>
			  <ItemGroup>
			    <PackageReference Include="Gtk4.Forms.Maps" Version="1.0.0" />
			  </ItemGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.DoesNotContain("Gtk4.Forms.Maps", result);
	}

	[Fact]
	public void RemovesXamarinFormsVisualMaterial()
	{
		var input =
			"""
			<Project>
			  <ItemGroup>
			    <PackageReference Include="Gtk4.Forms.Visual.Material" Version="1.0.0" />
			  </ItemGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.DoesNotContain("Gtk4.Forms.Visual.Material", result);
	}

	[Fact]
	public void RemovesXamarinFormsPlatformRef()
	{
		var input =
			"""
			<Project>
			  <ItemGroup>
			    <PackageReference Include="Gtk4.Forms.Platform.Android" Version="1.0.0" />
			  </ItemGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.DoesNotContain("Gtk4.Forms.Platform.Android", result);
	}

	// ── OutputType and StartupObject insertion ─────────────────────────────────

	[Fact]
	public void ReplacesLibraryWithExe()
	{
		var input =
			"""
			<Project>
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <OutputType>Library</OutputType>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<OutputType>Exe</OutputType>", result);
		Assert.DoesNotContain("Library", result);
	}

	[Fact]
	public void InsertsOutputTypeWhenAbsent()
	{
		var input =
			"""
			<Project>
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<OutputType>Exe</OutputType>", result);
	}

	[Fact]
	public void InsertsStartupObject()
	{
		var input =
			"""
			<Project>
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <OutputType>Exe</OutputType>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<StartupObject>Gtk4.Forms.Host.GtkEntryPoint</StartupObject>", result);
	}

}
