using Gtk.Forms.ProjectConverter;
using Xunit;

namespace Gtk.Forms.ProjectConverter.Tests;

/// <summary>Transform rules: OutputType normalisation for WPF projects.</summary>
public class CsprojConverterWpfOutputTypeTests
{
	[Fact]
	public void WinExeBecomesExe()
	{
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <OutputType>WinExe</OutputType>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<OutputType>Exe</OutputType>", result);
		Assert.DoesNotContain("WinExe", result);
	}

	[Fact]
	public void LibraryBecomesExe()
	{
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
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
			<Project Sdk="Microsoft.NET.Sdk">
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
	public void ExeOutputType_IsIdempotent()
	{
		var input =
			"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <OutputType>Exe</OutputType>
			  </PropertyGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.False(changed);
		Assert.Equal(input, result);
	}
}
